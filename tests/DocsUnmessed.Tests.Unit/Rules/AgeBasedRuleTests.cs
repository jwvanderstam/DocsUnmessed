namespace DocsUnmessed.Tests.Unit.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Tests.Unit.Helpers;
using FluentAssertions;

public class AgeBasedRuleTests
{
    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        // Assert
        rule.Name.Should().Be("ArchiveOldFiles");
        rule.Priority.Should().Be(120);
    }
    
    [Test]
    public void Matches_ReturnsTrueForFileOlderThanMinAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForFileNewerThanMinAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 60);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_ReturnsTrueForFileYoungerThanMaxAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "RecentFiles",
            120,
            minAgeDays: null,
            maxAgeDays: 30,
            "OneDrive://Recent/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 15);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForFileOlderThanMaxAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "RecentFiles",
            120,
            minAgeDays: null,
            maxAgeDays: 30,
            "OneDrive://Recent/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 45);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_ReturnsTrueForFileWithinAgeRange()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "MediumAgeFiles",
            120,
            minAgeDays: 30,
            maxAgeDays: 90,
            "OneDrive://Medium/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 60);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsFalseForFileBelowMinInRange()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "MediumAgeFiles",
            120,
            minAgeDays: 30,
            maxAgeDays: 90,
            "OneDrive://Medium/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 15);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_ReturnsFalseForFileAboveMaxInRange()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "MediumAgeFiles",
            120,
            minAgeDays: 30,
            maxAgeDays: 90,
            "OneDrive://Medium/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_ReturnsTrueForExactMinAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 90);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_ReturnsTrueForExactMaxAge()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "RecentFiles",
            120,
            minAgeDays: null,
            maxAgeDays: 30,
            "OneDrive://Recent/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 30);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_NoConstraints_AlwaysReturnsTrue()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "AllFiles",
            120,
            minAgeDays: null,
            maxAgeDays: null,
            "OneDrive://All/"
        );
        
        var oldItem = ItemFactory.CreateOldPdfInDownloads(daysOld: 365);
        var newItem = ItemFactory.CreateOldPdfInDownloads(daysOld: 1);
        
        // Act & Assert
        rule.Matches(oldItem).Should().BeTrue();
        rule.Matches(newItem).Should().BeTrue();
    }
    
    [Test]
    public void Map_ReturnsTargetSuggestionWithCorrectPath()
    {
        // Arrange
        var targetLocation = "OneDrive://Archive/";
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            targetLocation
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetPath.Should().Be(targetLocation);
        suggestion.TargetName.Should().Be(item.Name);
        suggestion.RuleName.Should().Be("ArchiveOldFiles");
    }
    
    [Test]
    public void Map_HasReasonableConfidence()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Confidence.Should().Be(0.85);
    }
    
    [Test]
    public void Map_IncludesAgeInReasons()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 100);
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Reasons.Should().HaveCount(1);
        suggestion.Reasons[0].Should().Contain("File age");
        suggestion.Reasons[0].Should().Contain("100 days");
    }
    
    [Test]
    public void Map_UsesTimestampSuffixConflictPolicy()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            120,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.ConflictPolicy.Should().Be(ConflictResolution.TimestampSuffix);
    }
    
    [TestCase(30, null, 15, false)]
    [TestCase(30, null, 30, true)]
    [TestCase(30, null, 45, true)]
    [TestCase(null, 30, 15, true)]
    [TestCase(null, 30, 30, true)]
    [TestCase(null, 30, 45, false)]
    [TestCase(30, 90, 15, false)]
    [TestCase(30, 90, 60, true)]
    [TestCase(30, 90, 120, false)]
    public void Matches_VariousAgeConstraints_ReturnsExpectedResult(
        int? minAgeDays,
        int? maxAgeDays,
        int actualAgeDays,
        bool expectedMatch)
    {
        // Arrange
        var rule = new AgeBasedRule(
            "TestRule",
            100,
            minAgeDays,
            maxAgeDays,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: actualAgeDays);
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().Be(expectedMatch);
    }
}
