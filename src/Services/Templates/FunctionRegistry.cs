namespace DocsUnmessed.Services.Templates;

using System.Text.RegularExpressions;

/// <summary>
/// Registry of template functions
/// </summary>
public sealed class FunctionRegistry
{
    private readonly Dictionary<string, Func<string, string[], string>> _functions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the FunctionRegistry class
    /// </summary>
    public FunctionRegistry()
    {
        RegisterDefaultFunctions();
    }

    /// <summary>
    /// Registers a custom function
    /// </summary>
    /// <param name="name">Function name</param>
    /// <param name="function">Function implementation</param>
    public void Register(string name, Func<string, string[], string> function)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Function name cannot be null or empty", nameof(name));
        }

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        _functions[name] = function;
    }

    /// <summary>
    /// Executes a function
    /// </summary>
    /// <param name="functionName">Function name</param>
    /// <param name="input">Input value</param>
    /// <param name="arguments">Function arguments</param>
    /// <returns>Function result</returns>
    public string Execute(string functionName, string input, string[] arguments)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            return input;
        }

        if (_functions.TryGetValue(functionName, out var function))
        {
            try
            {
                return function(input, arguments);
            }
            catch
            {
                // If function fails, return input unchanged
                return input;
            }
        }

        // Unknown function, return input unchanged
        return input;
    }

    /// <summary>
    /// Checks if a function is registered
    /// </summary>
    /// <param name="functionName">Function name</param>
    /// <returns>True if function exists</returns>
    public bool HasFunction(string functionName)
    {
        return _functions.ContainsKey(functionName);
    }

    private void RegisterDefaultFunctions()
    {
        // String manipulation functions
        Register("upper", (input, _) => input.ToUpperInvariant());
        Register("lower", (input, _) => input.ToLowerInvariant());
        Register("title", (input, _) => ToTitleCase(input));
        Register("trim", (input, _) => input.Trim());
        
        // Path manipulation
        Register("replace", (input, args) => 
            args.Length >= 2 ? input.Replace(args[0], args[1]) : input);
        
        Register("remove", (input, args) => 
            args.Length >= 1 ? input.Replace(args[0], string.Empty) : input);
        
        Register("substring", (input, args) =>
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var start))
            {
                return input;
            }

            if (start < 0 || start >= input.Length)
            {
                return input;
            }

            if (args.Length >= 2 && int.TryParse(args[1], out var length))
            {
                return input.Substring(start, Math.Min(length, input.Length - start));
            }

            return input[start..];
        });

        // Sanitization
        Register("sanitize", (input, _) => SanitizeFileName(input));
        
        Register("alphanumeric", (input, _) => 
            Regex.Replace(input, @"[^a-zA-Z0-9_\-]", string.Empty));

        // Padding
        Register("pad", (input, args) =>
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var width))
            {
                return input;
            }

            var padChar = args.Length >= 2 && args[1].Length > 0 ? args[1][0] : '0';
            return input.PadLeft(width, padChar);
        });

        // Truncation
        Register("truncate", (input, args) =>
        {
            if (args.Length == 0 || !int.TryParse(args[0], out var maxLength))
            {
                return input;
            }

            if (input.Length <= maxLength)
            {
                return input;
            }

            var suffix = args.Length >= 2 ? args[1] : "...";
            return input[..(maxLength - suffix.Length)] + suffix;
        });
    }

    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + 
                          (words[i].Length > 1 ? words[i][1..].ToLowerInvariant() : string.Empty);
            }
        }

        return string.Join(" ", words);
    }

    private static string SanitizeFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove invalid filename characters
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", input.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        
        // Replace multiple underscores with single
        sanitized = Regex.Replace(sanitized, @"_+", "_");
        
        // Trim underscores from start/end
        return sanitized.Trim('_');
    }
}
