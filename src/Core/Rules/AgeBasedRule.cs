namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Rule that matches files based on their age (days since last modification)
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
        // Calculate age in whole days (truncated, not rounded) to avoid floating point issues
        var ageDays = (int)(DateTime.UtcNow - item.ModifiedUtc).TotalDays;
        
        // File must be AT LEAST minAgeDays old (inclusive)
        if (_minAgeDays.HasValue && ageDays < _minAgeDays.Value)
            return false;
        
        // File must be AT MOST maxAgeDays old (inclusive)
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
