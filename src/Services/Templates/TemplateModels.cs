namespace DocsUnmessed.Services.Templates;

/// <summary>
/// Represents a parsed template with variables and functions
/// </summary>
public sealed class Template
{
    /// <summary>
    /// Gets the original template string
    /// </summary>
    public required string OriginalTemplate { get; init; }

    /// <summary>
    /// Gets the parsed template segments
    /// </summary>
    public required IReadOnlyList<TemplateSegment> Segments { get; init; }

    /// <summary>
    /// Gets whether the template is valid
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets validation errors if template is invalid
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents a segment of a template (literal text or variable)
/// </summary>
public abstract class TemplateSegment
{
    /// <summary>
    /// Gets the segment type
    /// </summary>
    public abstract TemplateSegmentType Type { get; }
}

/// <summary>
/// Represents literal text in a template
/// </summary>
public sealed class LiteralSegment : TemplateSegment
{
    /// <summary>
    /// Gets the literal text
    /// </summary>
    public required string Text { get; init; }

    /// <inheritdoc/>
    public override TemplateSegmentType Type => TemplateSegmentType.Literal;
}

/// <summary>
/// Represents a variable in a template
/// </summary>
public sealed class VariableSegment : TemplateSegment
{
    /// <summary>
    /// Gets the variable name
    /// </summary>
    public required string VariableName { get; init; }

    /// <summary>
    /// Gets the optional format string
    /// </summary>
    public string? FormatString { get; init; }

    /// <summary>
    /// Gets the optional function name
    /// </summary>
    public string? FunctionName { get; init; }

    /// <summary>
    /// Gets the function arguments
    /// </summary>
    public IReadOnlyList<string> FunctionArguments { get; init; } = Array.Empty<string>();

    /// <inheritdoc/>
    public override TemplateSegmentType Type => TemplateSegmentType.Variable;
}

/// <summary>
/// Template segment types
/// </summary>
public enum TemplateSegmentType
{
    /// <summary>
    /// Literal text
    /// </summary>
    Literal,

    /// <summary>
    /// Variable or function
    /// </summary>
    Variable
}

/// <summary>
/// Context for template variable resolution
/// </summary>
public sealed class TemplateContext
{
    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets or sets the file extension
    /// </summary>
    public string? Extension { get; init; }

    /// <summary>
    /// Gets or sets the file type
    /// </summary>
    public string? FileType { get; init; }

    /// <summary>
    /// Gets or sets the provider
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// Gets or sets the category
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets or sets the date
    /// </summary>
    public DateTime Date { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the counter value
    /// </summary>
    public int Counter { get; init; }

    /// <summary>
    /// Gets or sets custom variables
    /// </summary>
    public IReadOnlyDictionary<string, string> CustomVariables { get; init; } = 
        new Dictionary<string, string>();
}
