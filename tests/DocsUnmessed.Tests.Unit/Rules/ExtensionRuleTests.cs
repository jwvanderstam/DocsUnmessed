namespace DocsUnmessed.Tests.Unit.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Tests.Unit.Helpers;
using FluentAssertions;

public class ExtensionRuleTests
{
    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png" },
            "OneDrive://Photos/"
        );
        
        // Assert
        rule.Name.Should().Be("ImageRule");
        rule.Priority.Should().Be(150);
    }
    
    [Test]
    public void Matches_ReturnsTrueForMatchingExtension()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png", "gif" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateImageFile("jpg");
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForNonMatchingExtension()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png", "gif" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateTestItem(name: "document.pdf");
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_IsCaseInsensitive()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateTestItem(name: "photo.JPG");
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_HandlesExtensionWithoutDot()
    {
        // Arrange
        var rule = new ExtensionRule(
            "DocumentRule",
            150,
            new[] { "docx", "xlsx" },
            "OneDrive://Documents/"
        );
        
        var item = ItemFactory.CreateRecentDocument("docx");
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_SupportsMultipleExtensions()
    {
        // Arrange
        var rule = new ExtensionRule(
            "DocumentRule",
            150,
            new[] { "doc", "docx", "pdf", "txt" },
            "OneDrive://Documents/"
        );
        
        // Act & Assert
        var docxItem = ItemFactory.CreateTestItem(name: "file.docx");
        rule.Matches(docxItem).Should().BeTrue();
        
        var pdfItem = ItemFactory.CreateTestItem(name: "file.pdf");
        rule.Matches(pdfItem).Should().BeTrue();
        
        var txtItem = ItemFactory.CreateTestItem(name: "file.txt");
        rule.Matches(txtItem).Should().BeTrue();
        
        var xlsxItem = ItemFactory.CreateTestItem(name: "file.xlsx");
        rule.Matches(xlsxItem).Should().BeFalse();
    }
    
    [Test]
    public void Map_ReturnsTargetSuggestionWithCorrectPath()
    {
        // Arrange
        var targetLocation = "OneDrive://Photos/";
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png" },
            targetLocation
        );
        
        var item = ItemFactory.CreateImageFile();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetPath.Should().Be(targetLocation);
        suggestion.TargetName.Should().Be(item.Name);
        suggestion.RuleName.Should().Be("ImageRule");
    }
    
    [Test]
    public void Map_PreservesOriginalFileName()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateTestItem(name: "my-vacation-photo.jpg");
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetName.Should().Be("my-vacation-photo.jpg");
    }
    
    [Test]
    public void Map_HasGoodConfidence()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateImageFile();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Confidence.Should().Be(0.90);
    }
    
    [Test]
    public void Map_IncludesExtensionInReasons()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg", "png" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateImageFile("jpg");
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Reasons.Should().HaveCount(1);
        suggestion.Reasons[0].Should().Contain("Matched extension");
        suggestion.Reasons[0].Should().Contain(".jpg");
    }
    
    [Test]
    public void Map_UsesVersionSuffixConflictPolicy()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            150,
            new[] { "jpg" },
            "OneDrive://Photos/"
        );
        
        var item = ItemFactory.CreateImageFile();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.ConflictPolicy.Should().Be(ConflictResolution.VersionSuffix);
    }
    
    [TestCase("pdf", "document.pdf", true)]
    [TestCase("pdf", "document.PDF", true)]
    [TestCase("pdf", "document.docx", false)]
    [TestCase("jpg", "photo.jpg", true)]
    [TestCase("jpg", "photo.jpeg", false)]
    [TestCase("txt", "readme.txt", true)]
    [TestCase("txt", "readme.md", false)]
    public void Matches_SingleExtension_ReturnsExpectedResult(
        string extension,
        string fileName,
        bool expectedMatch)
    {
        // Arrange
        var rule = new ExtensionRule(
            "TestRule",
            100,
            new[] { extension },
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem(name: fileName);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().Be(expectedMatch);
    }
    
    [Test]
    public void Matches_EmptyExtensionArray_ReturnsFalse()
    {
        // Arrange
        var rule = new ExtensionRule(
            "NoExtensionRule",
            100,
            Array.Empty<string>(),
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem(name: "file.txt");
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
}
