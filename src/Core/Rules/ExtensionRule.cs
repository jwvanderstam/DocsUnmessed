namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Rule that matches files based on file extension
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
