namespace DocsUnmessed.Tests.Unit.Services;

using DocsUnmessed.Core.Configuration;
using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Services;
using DocsUnmessed.Tests.Unit.Helpers;
using FluentAssertions;
using System.Text.Json;
using NUnit.Framework;

public class RulesEngineTests
{
    [Test]
    public async Task EvaluateAsync_NoRulesLoaded_ReturnsNull()
    {
        // Arrange
        var engine = new RulesEngine();
        var item = ItemFactory.CreateTestItem();
        
        // Act
        var suggestion = await engine.EvaluateAsync(item);
        
        // Assert
        suggestion.Should().BeNull();
    }
    
    [Test]
    public async Task EvaluateAsync_NoMatchingRules_ReturnsNull()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "PDFRule",
                Match = new MappingMatch { PathRegex = @"\.pdf$" },
                Target = new MappingTarget { Location = "Target" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        var txtItem = ItemFactory.CreateTestItem(name: "file.txt");
        
        // Act
        var suggestion = await engine.EvaluateAsync(txtItem);
        
        // Assert
        suggestion.Should().BeNull();
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task EvaluateAsync_SingleMatchingRule_ReturnsSuggestion()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "PDFRule",
                Match = new MappingMatch { PathRegex = @"\.pdf$" },
                Target = new MappingTarget { Location = "OneDrive://Archive/" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        var pdfItem = ItemFactory.CreateTestItem(name: "document.pdf", path: "C:/file.pdf");
        
        // Act
        var suggestion = await engine.EvaluateAsync(pdfItem);
        
        // Assert
        suggestion.Should().NotBeNull();
        suggestion!.TargetPath.Should().Be("OneDrive://Archive/");
        suggestion.RuleName.Should().Contain("PDFRule");
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task EvaluateAsync_MultipleMatchingRules_ReturnsHighestPriority()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "LowPriorityRule",
                Match = new MappingMatch { Extensions = new[] { "pdf" } },
                Target = new MappingTarget { Location = "LowPriority/" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            },
            new MappingRule
            {
                Name = "HighPriorityRule",
                Match = new MappingMatch { PathRegex = @"Downloads.*\.pdf$" },
                Target = new MappingTarget { Location = "HighPriority/" },
                ConflictPolicy = "VersionSuffix",
                Priority = 200
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        var pdfItem = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var suggestion = await engine.EvaluateAsync(pdfItem);
        
        // Assert
        suggestion.Should().NotBeNull();
        suggestion!.TargetPath.Should().Be("HighPriority/");
        suggestion.RuleName.Should().Contain("HighPriorityRule");
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task GetApplicableRulesAsync_NoMatchingRules_ReturnsEmpty()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "PDFRule",
                Match = new MappingMatch { PathRegex = @"\.pdf$" },
                Target = new MappingTarget { Location = "Target" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        var txtItem = ItemFactory.CreateTestItem(name: "file.txt");
        
        // Act
        var rules = await engine.GetApplicableRulesAsync(txtItem);
        
        // Assert
        rules.Should().BeEmpty();
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task GetApplicableRulesAsync_MultipleMatching_ReturnsAll()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "ExtensionRule",
                Match = new MappingMatch { Extensions = new[] { "pdf" } },
                Target = new MappingTarget { Location = "Target1" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            },
            new MappingRule
            {
                Name = "RegexRule",
                Match = new MappingMatch { PathRegex = @"Downloads" },
                Target = new MappingTarget { Location = "Target2" },
                ConflictPolicy = "VersionSuffix",
                Priority = 150
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        var pdfItem = ItemFactory.CreateOldPdfInDownloads();
        
        // Act
        var rules = await engine.GetApplicableRulesAsync(pdfItem);
        
        // Assert
        rules.Should().HaveCount(2);
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task LoadRulesAsync_ValidConfig_LoadsSuccessfully()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "TestRule",
                Match = new MappingMatch { PathRegex = @"test" },
                Target = new MappingTarget { Location = "Target" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            }
        });
        
        // Act
        await engine.LoadRulesAsync(configPath);
        
        // Assert - verify by checking if rules work
        var testItem = ItemFactory.CreateTestItem(path: "C:/test/file.txt");
        var suggestion = await engine.EvaluateAsync(testItem);
        suggestion.Should().NotBeNull();
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task LoadRulesAsync_CompositeRule_CreatesCorrectly()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "CompositeRule",
                Match = new MappingMatch
                {
                    PathRegex = @"Downloads",
                    Extensions = new[] { "pdf" },
                    AgeDaysMin = 90
                },
                Target = new MappingTarget { Location = "Archive" },
                ConflictPolicy = "VersionSuffix",
                Priority = 200
            }
        });
        
        await engine.LoadRulesAsync(configPath);
        
        // Test with matching item (all conditions met)
        var matchingItem = ItemFactory.CreateOldPdfInDownloads(daysOld: 100);
        var suggestion1 = await engine.EvaluateAsync(matchingItem);
        suggestion1.Should().NotBeNull("all conditions are met");
        
        // Test with non-matching item (age doesn't match)
        var newItem = ItemFactory.CreateOldPdfInDownloads(daysOld: 30);
        var suggestion2 = await engine.EvaluateAsync(newItem);
        suggestion2.Should().BeNull("age condition not met");
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task LoadRulesAsync_InvalidJson_ThrowsException()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(configPath, "invalid json content");
        
        // Act
        var act = async () => await engine.LoadRulesAsync(configPath);
        
        // Assert
        await act.Should().ThrowAsync<JsonException>();
        
        // Cleanup
        File.Delete(configPath);
    }
    
    [Test]
    public async Task LoadRulesAsync_FileNotFound_ThrowsException()
    {
        // Arrange
        var engine = new RulesEngine();
        var nonExistentPath = "nonexistent_" + Guid.NewGuid() + ".json";
        
        // Act
        var act = async () => await engine.LoadRulesAsync(nonExistentPath);
        
        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }
    
    [Test]
    public async Task LoadRulesAsync_RuleWithNoMatchCriteria_ThrowsException()
    {
        // Arrange
        var engine = new RulesEngine();
        var configPath = CreateTempRuleFile(new[]
        {
            new MappingRule
            {
                Name = "InvalidRule",
                Match = new MappingMatch(), // No criteria
                Target = new MappingTarget { Location = "Target" },
                ConflictPolicy = "VersionSuffix",
                Priority = 100
            }
        });
        
        // Act
        var act = async () => await engine.LoadRulesAsync(configPath);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no valid matching criteria*");
        
        // Cleanup
        File.Delete(configPath);
    }
    
    private static string CreateTempRuleFile(IEnumerable<MappingRule> rules)
    {
        var tempFile = Path.GetTempFileName();
        var json = JsonSerializer.Serialize(rules.ToList(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(tempFile, json);
        return tempFile;
    }
}
