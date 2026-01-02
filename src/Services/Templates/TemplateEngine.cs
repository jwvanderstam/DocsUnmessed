namespace DocsUnmessed.Services.Templates;

using System.Text;

/// <summary>
/// Template engine for processing file naming templates
/// </summary>
public sealed class TemplateEngine
{
    private readonly TemplateParser _parser;
    private readonly VariableResolver _variableResolver;
    private readonly FunctionRegistry _functionRegistry;

    /// <summary>
    /// Initializes a new instance of the TemplateEngine class
    /// </summary>
    public TemplateEngine()
    {
        _parser = new TemplateParser();
        _variableResolver = new VariableResolver();
        _functionRegistry = new FunctionRegistry();
    }

    /// <summary>
    /// Gets the function registry for registering custom functions
    /// </summary>
    public FunctionRegistry Functions => _functionRegistry;

    /// <summary>
    /// Processes a template string with the given context
    /// </summary>
    /// <param name="templateString">Template string</param>
    /// <param name="context">Template context</param>
    /// <returns>Processed string</returns>
    public string Process(string templateString, TemplateContext context)
    {
        if (string.IsNullOrEmpty(templateString))
        {
            return string.Empty;
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var template = _parser.Parse(templateString);

        if (!template.IsValid)
        {
            throw new TemplateException(
                $"Invalid template: {string.Join(", ", template.ValidationErrors)}");
        }

        var result = new StringBuilder();

        foreach (var segment in template.Segments)
        {
            switch (segment)
            {
                case LiteralSegment literal:
                    result.Append(literal.Text);
                    break;

                case VariableSegment variable:
                    var value = ResolveVariable(variable, context);
                    result.Append(value);
                    break;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Validates a template string
    /// </summary>
    /// <param name="templateString">Template string</param>
    /// <returns>Validation result</returns>
    public TemplateValidationResult Validate(string templateString)
    {
        if (string.IsNullOrEmpty(templateString))
        {
            return new TemplateValidationResult
            {
                IsValid = false,
                Errors = new[] { "Template cannot be null or empty" }
            };
        }

        var template = _parser.Parse(templateString);

        var errors = new List<string>(template.ValidationErrors);

        // Validate functions
        foreach (var segment in template.Segments.OfType<VariableSegment>())
        {
            if (!string.IsNullOrEmpty(segment.FunctionName) && 
                !_functionRegistry.HasFunction(segment.FunctionName))
            {
                errors.Add($"Unknown function: {segment.FunctionName}");
            }
        }

        return new TemplateValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Template = template
        };
    }

    /// <summary>
    /// Gets all variables used in a template
    /// </summary>
    /// <param name="templateString">Template string</param>
    /// <returns>List of variable names</returns>
    public IReadOnlyList<string> GetVariables(string templateString)
    {
        if (string.IsNullOrEmpty(templateString))
        {
            return Array.Empty<string>();
        }

        var template = _parser.Parse(templateString);
        
        return template.Segments
            .OfType<VariableSegment>()
            .Select(v => v.VariableName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string ResolveVariable(VariableSegment variable, TemplateContext context)
    {
        // Resolve base value
        string value;
        
        if (!string.IsNullOrEmpty(variable.FormatString))
        {
            value = _variableResolver.ResolveWithFormat(
                variable.VariableName, 
                variable.FormatString, 
                context);
        }
        else
        {
            value = _variableResolver.Resolve(variable.VariableName, context);
        }

        // Apply function if specified
        if (!string.IsNullOrEmpty(variable.FunctionName))
        {
            value = _functionRegistry.Execute(
                variable.FunctionName, 
                value, 
                variable.FunctionArguments.ToArray());
        }

        return value;
    }
}

/// <summary>
/// Exception thrown when template processing fails
/// </summary>
public sealed class TemplateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the TemplateException class
    /// </summary>
    /// <param name="message">Error message</param>
    public TemplateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the TemplateException class
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public TemplateException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Result of template validation
/// </summary>
public sealed class TemplateValidationResult
{
    /// <summary>
    /// Gets whether the template is valid
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets validation errors
    /// </summary>
    public required IReadOnlyList<string> Errors { get; init; }

    /// <summary>
    /// Gets the parsed template if successful
    /// </summary>
    public Template? Template { get; init; }
}
