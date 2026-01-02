namespace DocsUnmessed.Services.Templates;

using System.Globalization;

/// <summary>
/// Resolves template variables to actual values
/// </summary>
public sealed class VariableResolver
{
    /// <summary>
    /// Resolves a variable from the context
    /// </summary>
    /// <param name="variableName">Variable name</param>
    /// <param name="context">Template context</param>
    /// <returns>Resolved value or empty string</returns>
    public string Resolve(string variableName, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return string.Empty;
        }

        return variableName.ToUpperInvariant() switch
        {
            "NAME" or "FILENAME" => context.FileName ?? string.Empty,
            "EXTENSION" or "EXT" => context.Extension ?? string.Empty,
            "TYPE" or "FILETYPE" => context.FileType ?? string.Empty,
            "PROVIDER" => context.Provider ?? string.Empty,
            "CATEGORY" => context.Category ?? string.Empty,
            "YEAR" => context.Date.Year.ToString(CultureInfo.InvariantCulture),
            "MONTH" => context.Date.Month.ToString("D2", CultureInfo.InvariantCulture),
            "DAY" => context.Date.Day.ToString("D2", CultureInfo.InvariantCulture),
            "DATE" => context.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "DATETIME" => context.Date.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture),
            "TIME" => context.Date.ToString("HH-mm-ss", CultureInfo.InvariantCulture),
            "COUNTER" => context.Counter.ToString(CultureInfo.InvariantCulture),
            _ => ResolveCustomVariable(variableName, context)
        };
    }

    /// <summary>
    /// Resolves a variable with format string
    /// </summary>
    /// <param name="variableName">Variable name</param>
    /// <param name="formatString">Format string</param>
    /// <param name="context">Template context</param>
    /// <returns>Formatted value</returns>
    public string ResolveWithFormat(string variableName, string formatString, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return string.Empty;
        }

        // Handle date formatting specially
        if (IsDateVariable(variableName))
        {
            try
            {
                return context.Date.ToString(formatString, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                // Invalid format, return unformatted
                return Resolve(variableName, context);
            }
        }

        // Handle numeric formatting
        if (variableName.Equals("COUNTER", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return context.Counter.ToString(formatString, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return context.Counter.ToString(CultureInfo.InvariantCulture);
            }
        }

        // Default: resolve without formatting
        return Resolve(variableName, context);
    }

    private static bool IsDateVariable(string variableName)
    {
        var upperName = variableName.ToUpperInvariant();
        return upperName is "DATE" or "DATETIME" or "TIME" or "YEAR" or "MONTH" or "DAY";
    }

    private static string ResolveCustomVariable(string variableName, TemplateContext context)
    {
        // Check custom variables (case-insensitive)
        foreach (var kvp in context.CustomVariables)
        {
            if (kvp.Key.Equals(variableName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return string.Empty;
    }
}
