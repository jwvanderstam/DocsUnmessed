namespace DocsUnmessed.Services.Templates;

using System.Text;

/// <summary>
/// Parses template strings into structured template objects
/// </summary>
public sealed class TemplateParser
{
    /// <summary>
    /// Parses a template string
    /// </summary>
    /// <param name="templateString">Template string to parse</param>
    /// <returns>Parsed template</returns>
    public Template Parse(string templateString)
    {
        if (string.IsNullOrEmpty(templateString))
        {
            return new Template
            {
                OriginalTemplate = templateString ?? string.Empty,
                Segments = Array.Empty<TemplateSegment>(),
                IsValid = false,
                ValidationErrors = new[] { "Template cannot be null or empty" }
            };
        }

        var segments = new List<TemplateSegment>();
        var errors = new List<string>();
        var currentPosition = 0;

        while (currentPosition < templateString.Length)
        {
            var openBrace = templateString.IndexOf('{', currentPosition);
            
            if (openBrace == -1)
            {
                // Rest is literal text
                if (currentPosition < templateString.Length)
                {
                    segments.Add(new LiteralSegment
                    {
                        Text = templateString[currentPosition..]
                    });
                }
                break;
            }

            // Add literal text before variable
            if (openBrace > currentPosition)
            {
                segments.Add(new LiteralSegment
                {
                    Text = templateString[currentPosition..openBrace]
                });
            }

            // Find matching close brace
            var closeBrace = FindMatchingBrace(templateString, openBrace);
            
            if (closeBrace == -1)
            {
                errors.Add($"Unclosed brace at position {openBrace}");
                break;
            }

            // Parse variable
            var variableContent = templateString[(openBrace + 1)..closeBrace];
            var variableSegment = ParseVariable(variableContent, openBrace, errors);
            segments.Add(variableSegment);

            currentPosition = closeBrace + 1;
        }

        return new Template
        {
            OriginalTemplate = templateString,
            Segments = segments,
            IsValid = errors.Count == 0,
            ValidationErrors = errors
        };
    }

    private static int FindMatchingBrace(string text, int openBracePosition)
    {
        var depth = 1;
        
        for (int i = openBracePosition + 1; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                depth++;
            }
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static VariableSegment ParseVariable(string content, int position, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add($"Empty variable at position {position}");
            return new VariableSegment
            {
                VariableName = string.Empty
            };
        }

        // Format: {VariableName} or {VariableName:format} or {VariableName|function(args)}
        var colonIndex = content.IndexOf(':');
        var pipeIndex = content.IndexOf('|');

        string variableName;
        string? formatString = null;
        string? functionName = null;
        List<string> functionArgs = new();

        if (pipeIndex != -1)
        {
            // Has function
            variableName = content[..pipeIndex].Trim();
            var functionPart = content[(pipeIndex + 1)..].Trim();
            
            var parenIndex = functionPart.IndexOf('(');
            if (parenIndex != -1)
            {
                functionName = functionPart[..parenIndex].Trim();
                var closeParen = functionPart.LastIndexOf(')');
                
                if (closeParen > parenIndex)
                {
                    var argsString = functionPart[(parenIndex + 1)..closeParen];
                    if (!string.IsNullOrWhiteSpace(argsString))
                    {
                        functionArgs = argsString.Split(',')
                            .Select(a => a.Trim())
                            .ToList();
                    }
                }
            }
            else
            {
                functionName = functionPart;
            }
        }
        else if (colonIndex != -1)
        {
            // Has format string
            variableName = content[..colonIndex].Trim();
            formatString = content[(colonIndex + 1)..].Trim();
        }
        else
        {
            // Simple variable
            variableName = content.Trim();
        }

        if (string.IsNullOrWhiteSpace(variableName))
        {
            errors.Add($"Invalid variable name at position {position}");
        }

        return new VariableSegment
        {
            VariableName = variableName,
            FormatString = formatString,
            FunctionName = functionName,
            FunctionArguments = functionArgs
        };
    }
}
