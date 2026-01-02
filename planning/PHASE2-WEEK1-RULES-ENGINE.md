# Phase 2 Implementation Guide: Core Features

## Quick Start - Week 1: Rules Engine

This guide walks you through implementing the rules engine, the first major feature in Phase 2.

---

## Step 1: Create Rules Engine Interface (Day 1)

### 1.1 Update IRulesEngine Interface

**File**: `src/Core/Interfaces/IRule.cs` (already exists, enhance it)

Add concrete rule implementations:

```csharp
namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for all rule implementations
/// </summary>
public abstract class RuleBase : IRule
{
    public required string Name { get; init; }
    public required int Priority { get; init; }
    
    public abstract bool Matches(Item item);
    public abstract TargetSuggestion Map(Item item);
}

/// <summary>
/// Rule that matches based on regex pattern
/// </summary>
public sealed class RegexPathRule : RuleBase
{
    private readonly Regex _pathRegex;
    private readonly string _targetLocation;
    private readonly string? _namingTemplate;
    
    public RegexPathRule(string name, int priority, string pattern, string targetLocation, string? namingTemplate = null)
    {
        Name = name;
        Priority = priority;
        _pathRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        _targetLocation = targetLocation;
        _namingTemplate = namingTemplate;
    }
    
    public override bool Matches(Item item)
    {
        return _pathRegex.IsMatch(item.Path);
    }
    
    public override TargetSuggestion Map(Item item)
    {
        return new TargetSuggestion
        {
            TargetPath = _targetLocation,
            TargetName = _namingTemplate ?? item.Name,
            RuleName = Name,
            Confidence = 0.95,
            Reasons = new List<string> { $"Matched regex pattern: {_pathRegex}" },
            ConflictPolicy = ConflictResolution.VersionSuffix
        };
    }
}

/// <summary>
/// Rule that matches based on file extension
/// </summary>
public sealed class ExtensionRule : RuleBase
{
    private readonly HashSet<string> _extensions;
    private readonly string _targetLocation;
    
    public ExtensionRule(string name, int priority, string[] extensions, string targetLocation)
    {
        Name = name;
        Priority = priority;
        _extensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        _targetLocation = targetLocation;
    }
    
    public override bool Matches(Item item)
    {
        var extension = Path.GetExtension(item.Name).TrimStart('.');
        return _extensions.Contains(extension);
    }
    
    public override TargetSuggestion Map(Item item)
    {
        return new TargetSuggestion
        {
            TargetPath = _targetLocation,
            TargetName = item.Name,
            RuleName = Name,
            Confidence = 0.90,
            Reasons = new List<string> { $"Matched extension: {Path.GetExtension(item.Name)}" },
            ConflictPolicy = ConflictResolution.VersionSuffix
        };
    }
}

/// <summary>
/// Rule that matches based on file age
/// </summary>
public sealed class AgeBasedRule : RuleBase
{
    private readonly int? _minAgeDays;
    private readonly int? _maxAgeDays;
    private readonly string _targetLocation;
    
    public AgeBasedRule(string name, int priority, int? minAgeDays, int? maxAgeDays, string targetLocation)
    {
        Name = name;
        Priority = priority;
        _minAgeDays = minAgeDays;
        _maxAgeDays = maxAgeDays;
        _targetLocation = targetLocation;
    }
    
    public override bool Matches(Item item)
    {
        var ageDays = (DateTime.UtcNow - item.ModifiedUtc).TotalDays;
        
        if (_minAgeDays.HasValue && ageDays < _minAgeDays.Value)
            return false;
            
        if (_maxAgeDays.HasValue && ageDays > _maxAgeDays.Value)
            return false;
            
        return true;
    }
    
    public override TargetSuggestion Map(Item item)
    {
        var ageDays = (int)(DateTime.UtcNow - item.ModifiedUtc).TotalDays;
        return new TargetSuggestion
        {
            TargetPath = _targetLocation,
            TargetName = item.Name,
            RuleName = Name,
            Confidence = 0.85,
            Reasons = new List<string> { $"File age: {ageDays} days" },
            ConflictPolicy = ConflictResolution.TimestampSuffix
        };
    }
}

/// <summary>
/// Composite rule that combines multiple conditions
/// </summary>
public sealed class CompositeRule : RuleBase
{
    private readonly List<IRule> _childRules;
    private readonly bool _requireAll; // AND vs OR logic
    private readonly string _targetLocation;
    
    public CompositeRule(string name, int priority, List<IRule> childRules, bool requireAll, string targetLocation)
    {
        Name = name;
        Priority = priority;
        _childRules = childRules;
        _requireAll = requireAll;
        _targetLocation = targetLocation;
    }
    
    public override bool Matches(Item item)
    {
        if (_requireAll)
            return _childRules.All(r => r.Matches(item));
        else
            return _childRules.Any(r => r.Matches(item));
    }
    
    public override TargetSuggestion Map(Item item)
    {
        var reasons = _childRules
            .Where(r => r.Matches(item))
            .SelectMany(r => r.Map(item).Reasons)
            .ToList();
            
        return new TargetSuggestion
        {
            TargetPath = _targetLocation,
            TargetName = item.Name,
            RuleName = Name,
            Confidence = 0.88,
            Reasons = reasons,
            ConflictPolicy = ConflictResolution.VersionSuffix
        };
    }
}
```

**Create file**: `src/Core/Rules/RuleTypes.cs`

---

### 1.2 Implement RulesEngine Service

**Create file**: `src/Services/RulesEngine.cs`

```csharp
namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Configuration;
using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Core.Rules;
using System.Text.Json;

/// <summary>
/// Service that evaluates rules and generates target suggestions
/// </summary>
public sealed class RulesEngine : IRulesEngine
{
    private readonly List<IRule> _rules = new();
    
    public async Task<TargetSuggestion?> EvaluateAsync(Item item, CancellationToken cancellationToken = default)
    {
        var applicableRules = await GetApplicableRulesAsync(item, cancellationToken);
        
        if (applicableRules.Count == 0)
            return null;
            
        // Use highest priority rule
        var bestRule = applicableRules.OrderByDescending(r => r.Priority).First();
        return bestRule.Map(item);
    }
    
    public async Task<IReadOnlyList<IRule>> GetApplicableRulesAsync(Item item, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            return _rules.Where(r => r.Matches(item)).ToList();
        }, cancellationToken);
    }
    
    public async Task LoadRulesAsync(string configPath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(configPath, cancellationToken);
        var mappingRules = JsonSerializer.Deserialize<List<MappingRule>>(json);
        
        if (mappingRules == null)
            throw new InvalidOperationException("Failed to deserialize mapping rules");
            
        foreach (var rule in mappingRules)
        {
            _rules.Add(CreateRuleFromConfig(rule));
        }
    }
    
    private static IRule CreateRuleFromConfig(MappingRule config)
    {
        var childRules = new List<IRule>();
        
        // Add regex rule if present
        if (!string.IsNullOrEmpty(config.Match.PathRegex))
        {
            childRules.Add(new RegexPathRule(
                $"{config.Name}_Regex",
                config.Priority,
                config.Match.PathRegex,
                config.Target.Location,
                config.Target.NamingTemplate
            ));
        }
        
        // Add extension rule if present
        if (config.Match.Extensions?.Length > 0)
        {
            childRules.Add(new ExtensionRule(
                $"{config.Name}_Extension",
                config.Priority,
                config.Match.Extensions,
                config.Target.Location
            ));
        }
        
        // Add age rule if present
        if (config.Match.AgeDaysMin.HasValue || config.Match.AgeDaysMax.HasValue)
        {
            childRules.Add(new AgeBasedRule(
                $"{config.Name}_Age",
                config.Priority,
                config.Match.AgeDaysMin,
                config.Match.AgeDaysMax,
                config.Target.Location
            ));
        }
        
        // If multiple conditions, create composite rule with AND logic
        if (childRules.Count > 1)
        {
            return new CompositeRule(
                config.Name,
                config.Priority,
                childRules,
                requireAll: true, // AND logic
                config.Target.Location
            );
        }
        
        return childRules.First();
    }
}
```

---

## Step 2: Test Rules Engine (Day 2)

### 2.1 Create Unit Tests

**Create file**: `tests/DocsUnmessed.Tests.Unit/Services/RulesEngineTests.cs`

```csharp
namespace DocsUnmessed.Tests.Unit.Services;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Rules;
using DocsUnmessed.Services;
using Xunit;

public class RulesEngineTests
{
    [Fact]
    public async Task RegexPathRule_MatchesCorrectPath()
    {
        // Arrange
        var rule = new RegexPathRule(
            "TestRule",
            100,
            @"(?i)(Downloads/).*\.pdf$",
            "OneDrive://Archive/",
            null
        );
        
        var item = new Item
        {
            Path = "C:/Users/Test/Downloads/document.pdf",
            Name = "document.pdf",
            Provider = "fs_local",
            Size = 1024,
            MimeType = "application/pdf",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            Type = ItemType.File,
            Depth = 3
        };
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        Assert.True(matches);
    }
    
    [Fact]
    public async Task ExtensionRule_MatchesCorrectExtension()
    {
        // Arrange
        var rule = new ExtensionRule(
            "ImageRule",
            100,
            new[] { "jpg", "png", "gif" },
            "OneDrive://Photos/"
        );
        
        var item = new Item
        {
            Path = "C:/Users/Test/Pictures/photo.jpg",
            Name = "photo.jpg",
            Provider = "fs_local",
            Size = 2048,
            MimeType = "image/jpeg",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            Type = ItemType.File,
            Depth = 3
        };
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        Assert.True(matches);
    }
    
    [Fact]
    public async Task AgeBasedRule_MatchesOldFiles()
    {
        // Arrange
        var rule = new AgeBasedRule(
            "ArchiveOldFiles",
            100,
            minAgeDays: 90,
            maxAgeDays: null,
            "OneDrive://Archive/"
        );
        
        var item = new Item
        {
            Path = "C:/Users/Test/Documents/old.txt",
            Name = "old.txt",
            Provider = "fs_local",
            Size = 512,
            MimeType = "text/plain",
            CreatedUtc = DateTime.UtcNow.AddDays(-120),
            ModifiedUtc = DateTime.UtcNow.AddDays(-120),
            Type = ItemType.File,
            Depth = 3
        };
        
        // Act
        var matches = rule.Matches(item);
        
        // Assert
        Assert.True(matches);
    }
    
    [Fact]
    public async Task RulesEngine_ReturnsHighestPriorityRule()
    {
        // Arrange
        var engine = new RulesEngine();
        
        // TODO: Add rules to engine
        // TODO: Evaluate item
        // TODO: Assert highest priority rule is used
        
        Assert.True(true); // Placeholder
    }
}
```

---

## Step 3: Integrate with CLI (Day 3)

### 3.1 Update SimulateCommand

**Update file**: `src/CLI/Commands/SimulateCommand.cs`

Add rules engine evaluation:

```csharp
// In SimulateCommand.ExecuteAsync, add:

var rulesEngine = new RulesEngine();
await rulesEngine.LoadRulesAsync("examples/mapping-rules.json", cancellationToken);

Console.WriteLine("Evaluating rules for all items...");
var suggestions = new List<(Item item, TargetSuggestion suggestion)>();

foreach (var item in scan.Items)
{
    var suggestion = await rulesEngine.EvaluateAsync(item, cancellationToken);
    if (suggestion != null)
    {
        suggestions.Add((item, suggestion));
    }
}

Console.WriteLine($"Generated {suggestions.Count} target suggestions");
```

---

## Step 4: Create Rule Configuration Loader (Day 4)

### 4.1 Support Multiple Rule Files

**Create file**: `src/Services/ConfigurationLoader.cs`

```csharp
namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Configuration;
using System.Text.Json;
using YamlDotNet.Serialization;

public interface IConfigurationLoader
{
    Task<TiaBlueprint> LoadTiaBlueprintAsync(string path, CancellationToken cancellationToken = default);
    Task<List<MappingRule>> LoadMappingRulesAsync(string path, CancellationToken cancellationToken = default);
    Task<NamingTemplate> LoadNamingTemplateAsync(string path, CancellationToken cancellationToken = default);
}

public sealed class ConfigurationLoader : IConfigurationLoader
{
    public async Task<TiaBlueprint> LoadTiaBlueprintAsync(string path, CancellationToken cancellationToken = default)
    {
        var yaml = await File.ReadAllTextAsync(path, cancellationToken);
        var deserializer = new DeserializerBuilder().Build();
        var blueprint = deserializer.Deserialize<TiaBlueprint>(yaml);
        return blueprint ?? throw new InvalidOperationException("Failed to load TIA blueprint");
    }
    
    public async Task<List<MappingRule>> LoadMappingRulesAsync(string path, CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(path))
        {
            // Load all JSON files from directory
            var rules = new List<MappingRule>();
            var files = Directory.GetFiles(path, "*.json");
            
            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var rule = JsonSerializer.Deserialize<MappingRule>(json);
                if (rule != null)
                    rules.Add(rule);
            }
            
            return rules;
        }
        else
        {
            // Load single file
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var rule = JsonSerializer.Deserialize<MappingRule>(json);
            return rule != null ? new List<MappingRule> { rule } : new();
        }
    }
    
    public async Task<NamingTemplate> LoadNamingTemplateAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var template = JsonSerializer.Deserialize<NamingTemplate>(json);
        return template ?? throw new InvalidOperationException("Failed to load naming template");
    }
}
```

**NuGet**: Add `YamlDotNet` package for YAML support:

```xml
<PackageReference Include="YamlDotNet" Version="15.1.0" />
```

---

## Step 5: Testing & Validation (Day 5)

### 5.1 Create Integration Test

**Create file**: `tests/DocsUnmessed.Tests.Integration/RulesEngineIntegrationTests.cs`

```csharp
namespace DocsUnmessed.Tests.Integration;

using DocsUnmessed.Services;
using Xunit;

public class RulesEngineIntegrationTests
{
    [Fact]
    public async Task CanLoadAndEvaluateRules()
    {
        // Arrange
        var engine = new RulesEngine();
        await engine.LoadRulesAsync("examples/mapping-rule-downloads.json");
        
        // Create test item
        var item = new Item
        {
            Path = "C:/Users/Test/Downloads/old-document.pdf",
            Name = "old-document.pdf",
            Provider = "fs_local",
            Size = 1024,
            MimeType = "application/pdf",
            CreatedUtc = DateTime.UtcNow.AddDays(-100),
            ModifiedUtc = DateTime.UtcNow.AddDays(-100),
            Type = ItemType.File,
            Depth = 3
        };
        
        // Act
        var suggestion = await engine.EvaluateAsync(item);
        
        // Assert
        Assert.NotNull(suggestion);
        Assert.Contains("Archive", suggestion.TargetPath);
    }
}
```

---

## Quick Commands

```bash
# Create test project
dotnet new xunit -n DocsUnmessed.Tests.Unit -o tests/DocsUnmessed.Tests.Unit
dotnet sln add tests/DocsUnmessed.Tests.Unit/DocsUnmessed.Tests.Unit.csproj

# Add YamlDotNet
dotnet add package YamlDotNet --version 15.1.0

# Run tests
dotnet test

# Test rules engine with CLI
dotnet run -- assess --providers fs_local --root "./test-data" --out scan.json
dotnet run -- simulate --scan-id <scan-id> --out plan.json
```

---

## Next Steps

After completing the rules engine (Week 1), proceed to:

1. **Week 2**: SQLite Persistence
2. **Week 3**: Naming Template Engine
3. **Week 4**: Enhanced Duplicate Detection

Each week follows a similar pattern:
1. Design interfaces
2. Implement core logic
3. Write unit tests
4. Create integration tests
5. Integrate with CLI
6. Test end-to-end

---

## Checklist

### Rules Engine Completion Criteria

- [ ] RegexPathRule implemented and tested
- [ ] ExtensionRule implemented and tested
- [ ] AgeBasedRule implemented and tested
- [ ] CompositeRule implemented and tested
- [ ] RulesEngine loads from config files
- [ ] Priority-based rule selection works
- [ ] Integration with SimulateCommand complete
- [ ] Unit tests achieve >80% coverage
- [ ] Integration tests pass
- [ ] Documentation updated

---

*Next Guide: [Week 2 - SQLite Persistence Implementation](PHASE2-WEEK2-SQLITE.md)*
