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
    
    /// <summary>
    /// Creates a rule implementation from configuration
    /// </summary>
    private static IRule CreateRuleFromConfig(MappingRule config)
    {
        var childRules = new List<IRule>();
        
        // Add regex rule if present
        if (!string.IsNullOrEmpty(config.Match.PathRegex))
        {
            childRules.Add(new 








































































































RegexPathRule(
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
        
        // If no rules matched, throw error
        if (childRules.Count == 0)
        {
            throw new InvalidOperationException($"Rule '{config.Name}' has no valid matching criteria");
        }
        
        // If multiple conditions, create composite rule with AND logic
        if (childRules.Count > 1)
        {
            return new CompositeRule(
                config.Name,
                config.Priority,
                childRules,
                requireAll: true, // AND logic by default
                config.Target.Location
            );
        }
        
        // Single condition, return the rule directly
        return childRules.First();
    }
}
