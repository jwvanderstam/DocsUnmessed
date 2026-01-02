namespace DocsUnmessed.Core.Rules;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;

/// <summary>
/// Base class for all rule implementations
/// </summary>
public abstract class RuleBase : IRule
{
    public string Name { get; init; } = string.Empty;
    public int Priority { get; init; }
    
    public abstract bool Matches(Item item);
    public abstract TargetSuggestion Map(Item item);
}
