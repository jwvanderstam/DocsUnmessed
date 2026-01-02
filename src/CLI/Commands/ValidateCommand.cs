namespace DocsUnmessed.CLI.Commands;

using DocsUnmessed.Services;

/// <summary>
/// Command to validate rule configuration files
/// </summary>
public sealed class ValidateCommand
{
    public async Task<int> ExecuteAsync(ValidateOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("DocsUnmessed - Rule Validation");
        Console.WriteLine("================================\n");

        try
        {
            var validator = new RuleValidator();
            
            Console.WriteLine($"Validating rules from: {options.RulesPath}\n");
            
            var result = await Task.Run(() => validator.Validate(options.RulesPath), cancellationToken);
            
            result.PrintToConsole();

            return result.IsValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n? Error: {ex.Message}");
            if (options.Verbose)
            {
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            }
            return 1;
        }
    }
}

public sealed class ValidateOptions
{
    public required string RulesPath { get; init; }
    public bool Verbose { get; init; }
}
