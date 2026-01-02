namespace DocsUnmessed.Tests.Unit.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Tests.Unit.Helpers;
using FluentAssertions;

public class CompositeRuleTests
{
    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true),
            CreateMockRule("Rule2", true)
        };
        
        // Act
        var rule = new CompositeRule(
            "CompositeRule",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Target/"
        );
        
        // Assert
        rule.Name.Should().Be("CompositeRule");
        rule.Priority.Should().Be(200);
    }
    
    [Test]
    public void Matches_AndLogic_ReturnsTrueWhenAllMatch()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true),
            CreateMockRule("Rule2", true),
            CreateMockRule("Rule3", true)
        };
        
        var rule = new CompositeRule(
            "AllMustMatch",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_AndLogic_ReturnsFalseWhenOneDoesNotMatch()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true),
            CreateMockRule("Rule2", false),
            CreateMockRule("Rule3", true)
        };
        
        var rule = new CompositeRule(
            "AllMustMatch",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_OrLogic_ReturnsTrueWhenAtLeastOneMatches()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", false),
            CreateMockRule("Rule2", true),
            CreateMockRule("Rule3", false)
        };
        
        var rule = new CompositeRule(
            "AnyCanMatch",
            200,
            childRules,
            requireAll: false,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue();
    }
    
    [Test]
    public void Matches_OrLogic_ReturnsFalseWhenNoneMatch()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", false),
            CreateMockRule("Rule2", false),
            CreateMockRule("Rule3", false)
        };
        
        var rule = new CompositeRule(
            "AnyCanMatch",
            200,
            childRules,
            requireAll: false,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse();
    }
    
    [Test]
    public void Matches_RealRules_CombinesRegexAndAge()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            new RegexPathRule("PDFRule", 100, @"\.pdf$", "Target"),
            new AgeBasedRule("OldRule", 100, 90, null, "Target")
        };
        
        var rule = new CompositeRule(
            "OldPDFs",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Archive/"
        );
        
        var oldPdf = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        var newPdf = ItemFactory.CreateOldPdfInDownloads(daysOld: 30);
        var oldTxtBase = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        var oldTxt = ItemFactory.CreateTestItem(
            name: "file.txt", 
            path: "C:/Users/Test/file.txt",
            modifiedUtc: DateTime.UtcNow.AddDays(-120));
        
        // Act & Assert
        rule.Matches(oldPdf).Should().BeTrue("old PDF matches both rules");
        rule.Matches(newPdf).Should().BeFalse("new PDF doesn't match age rule");
        rule.Matches(oldTxt).Should().BeFalse("old TXT doesn't match regex rule");
    }
    
    [Test]
    public void Matches_RealRules_CombinesExtensionAndAge()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            new ExtensionRule("DocRule", 100, new[] { "docx", "xlsx" }, "Target"),
            new AgeBasedRule("RecentRule", 100, null, 30, "Target")
        };
        
        var rule = new CompositeRule(
            "RecentDocs",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Recent/"
        );
        
        var recentDoc = ItemFactory.CreateRecentDocument("docx");
        var oldDocBase = ItemFactory.CreateOldPdfInDownloads(daysOld: 60);
        var oldDoc = ItemFactory.CreateTestItem(
            name: "doc.docx",
            modifiedUtc: DateTime.UtcNow.AddDays(-60));
        
        // Act & Assert
        rule.Matches(recentDoc).Should().BeTrue("recent doc matches both rules");
        rule.Matches(oldDoc).Should().BeFalse("old doc doesn't match age rule");
    }
    
    [Test]
    public void Map_AggregatesReasonsFromMatchingRules()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            new RegexPathRule("PDFRule", 100, @"\.pdf$", "Target"),
            new AgeBasedRule("OldRule", 100, 90, null, "Target")
        };
        
        var rule = new CompositeRule(
            "OldPDFs",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Archive/"
        );
        
        var item = ItemFactory.CreateOldPdfInDownloads(daysOld: 120);
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Reasons.Should().HaveCountGreaterThan(1, "should have reasons from multiple rules");
        suggestion.Reasons.Should().Contain(r => r.Contains("regex"), "should include regex reason");
        suggestion.Reasons.Should().Contain(r => r.Contains("age"), "should include age reason");
    }
    
    [Test]
    public void Map_ReturnsTargetSuggestionWithCorrectPath()
    {
        // Arrange
        var targetLocation = "OneDrive://Combined/";
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true),
            CreateMockRule("Rule2", true)
        };
        
        var rule = new CompositeRule(
            "CompositeRule",
            200,
            childRules,
            requireAll: true,
            targetLocation
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.TargetPath.Should().Be(targetLocation);
        suggestion.RuleName.Should().Be("CompositeRule");
    }
    
    [Test]
    public void Map_HasReasonableConfidence()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true),
            CreateMockRule("Rule2", true)
        };
        
        var rule = new CompositeRule(
            "CompositeRule",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.Confidence.Should().Be(0.88);
    }
    
    [Test]
    public void Map_UsesVersionSuffixConflictPolicy()
    {
        // Arrange
        var childRules = new List<IRule>
        {
            CreateMockRule("Rule1", true)
        };
        
        var rule = new CompositeRule(
            "CompositeRule",
            200,
            childRules,
            requireAll: true,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var suggestion = rule.Map(item);
        
        // Assert
        suggestion.ConflictPolicy.Should().Be(ConflictResolution.VersionSuffix);
    }
    
    [Test]
    public void Matches_EmptyChildRules_AndLogic_ReturnsTrue()
    {
        // Arrange
        var rule = new CompositeRule(
            "EmptyComposite",
            200,
            new List<IRule>(),
            requireAll: true,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeTrue("empty AND returns true (all conditions met vacuously)");
    }
    
    [Test]
    public void Matches_EmptyChildRules_OrLogic_ReturnsFalse()
    {
        // Arrange
        var rule = new CompositeRule(
            "EmptyComposite",
            200,
            new List<IRule>(),
            requireAll: false,
            "OneDrive://Target/"
        );
        
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        matches.Should().BeFalse("empty OR returns false (no conditions met)");
    }
    
    // Helper method to create a simple mock rule
    private static IRule CreateMockRule(string name, bool matchResult)
    {
        return new SimpleMockRule(name, 100, matchResult, "MockTarget");
    }
    
    // Simple mock rule for testing
    private sealed class SimpleMockRule : RuleBase
    {
        private readonly bool _matchResult;
        private readonly string _targetLocation;
        
        public SimpleMockRule(string name, int priority, bool matchResult, string targetLocation)
        {
            Name = name;
            Priority = priority;
            _matchResult = matchResult;
            _targetLocation = targetLocation;
        }
        
        public override bool Matches(Item item) => _matchResult;
        
        public override TargetSuggestion Map(Item item)
        {
            return new TargetSuggestion
            {
                TargetPath = _targetLocation,
                TargetName = item.Name,
                RuleName = Name,
                Confidence = 0.80,
                Reasons = new List<string> { $"Mock rule {Name}" },
                ConflictPolicy = ConflictResolution.VersionSuffix
            };
        }
    }
}
