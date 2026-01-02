namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;
using System.Text.RegularExpressions;

/// <summary>
/// Rule that matches files based on regex pattern against the file path
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
