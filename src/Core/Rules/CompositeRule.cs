namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;

/// <summary>
/// Composite rule that combines multiple child rules with AND or OR logic
/// </summary>
public sealed class CompositeRule : RuleBase
{
    private readonly List<IRule> _childRules;
    private readonly bool _requireAll; // true = AND logic, false = OR logic
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
