namespace DocsUnmessed.Core.Configuration;

/// <summary>
/// Naming template configuration
/// </summary>
public sealed class NamingTemplate
{
    public required string Name { get; init; }
    public required string Pattern { get; init; }
    public required NamingRequirements Requirements { get; init; }
    public NamingNormalization? Normalization { get; init; }
    public required NamingConstraints Constraints { get; init; }
}

public sealed class NamingRequirements
{
    public required string[] Tokens { get; init; }
    public TokenRules? NN { get; init; }
}

public sealed class TokenRules
{
    public required int Min { get; init; }
    public required int Max { get; init; }
    public required int Pad { get; init; }
}

public sealed class NamingNormalization
{
    public Dictionary<string, TokenNormalization> TokenRules { get; init; } = new();
}

public sealed class TokenNormalization
{
    public string? Case { get; init; }
    public string? Separator { get; init; }
    public bool TrimStopwords { get; init; }
}

public sealed class NamingConstraints
{
    public required string[] InvalidChars { get; init; }
    public required int MaxFileNameLength { get; init; }
    public required int PathLengthBudget { get; init; }
}
