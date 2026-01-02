namespace DocsUnmessed.Core.Interfaces;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Abstraction for mapping rules that determine target locations
/// </summary>
public interface IRule
{
    string Name { get; }
    int Priority { get; }
    
    bool Matches(Item item);
    TargetSuggestion Map(Item item);
}

/// <summary>
/// Service that evaluates rules and generates target suggestions
/// </summary>
public interface IRulesEngine
{
    Task<TargetSuggestion?> EvaluateAsync(Item item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IRule>> GetApplicableRulesAsync(Item item, CancellationToken cancellationToken = default);
    Task LoadRulesAsync(string configPath, CancellationToken cancellationToken = default);
}
