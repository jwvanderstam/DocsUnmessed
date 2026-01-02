namespace DocsUnmessed.Tests.Unit.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Tests.Unit.Helpers;
using FluentAssertions;

public class RegexPathRuleTests
{
    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var rule = new RegexPathRule(
            "TestRule",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        // Assert
        rule.Name.Should().Be("TestRule");
        rule.Priority.Should().Be(100);
    }
    
    [Test]
    public void Matches_ReturnsTrueForMatchingPath()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateTestItem(
            path: "C:/Users/Test/Downloads/document.pdf",
            name: "document.pdf"
        );
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForNonMatchingPath()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateTestItem(
            path: "C:/Users/Test/Documents/document.pdf",
            name: "document.pdf"
        );
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_IsCaseInsensitive()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateTestItem(
            path: "C:/Users/Test/DOWNLOADS/document.PDF",
            name: "document.PDF"
        );
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForWrongExtension()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateTestItem(
            path: "C:/Users/Test/Downloads/document.docx",
            name: "document.docx"
        );
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Map_ReturnsTargetSuggestionWithCorrectPath()
    {
        // Arrange
        var targetLocation = "OneDrive://Archive/";
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            targetLocation
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetPath.Should().Be(targetLocation);
        suggestion.TargetName.Should().Be(item.Name);
        suggestion.RuleName.Should().Be("DownloadsPDF");
    }
    
    [Test]
    public void Map_UsesNamingTemplateWhenProvided()
    {
        // Arrange
        var namingTemplate = "StandardDateContextTitle";
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/",
            namingTemplate
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetName.Should().Be(namingTemplate);
    }
    
    [Test]
    public void Map_HasHighConfidence()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Confidence.Should().Be(0.95);
    }
    
    [Test]
    public void Map_IncludesReasonForMatching()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Reasons.Should().HaveCount(1);
        suggestion.Reasons[0].Should().Contain("Matched regex pattern");
    }
    
    [Test]
    public void Map_UsesVersionSuffixConflictPolicy()
    {
        // Arrange
        var rule = new RegexPathRule(
            "DownloadsPDF",
            100,
            @"(?i)Downloads/.*\.pdf$",
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.ConflictPolicy.Should().Be(ConflictResolution.VersionSuffix);
    }
    
    [TestCase(@"\.txt$", "document.txt", true)]
    [TestCase(@"\.txt$", "document.pdf", false)]
    [TestCase(@"Pictures/", "C:/Users/Test/Pictures/photo.jpg", true)]
    [TestCase(@"Pictures/", "C:/Users/Test/Documents/photo.jpg", false)]
    [TestCase(@"\d{4}-\d{2}-\d{2}", "report-2025-01-03.pdf", true)]
    [TestCase(@"\d{4}-\d{2}-\d{2}", "report-no-date.pdf", false)]
    public void Matches_VariousPatterns_ReturnsExpectedResult(
        string pattern, 
        string path, 
        bool expectedMatch)
    {
        // Arrange
        var rule = new RegexPathRule(
            "TestRule",
            100,
            pattern,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem(path: path, name: Path.GetFileName(path));
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().Be(expectedMatch);
    }
}
