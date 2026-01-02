namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Validates rule configurations for correctness and completeness
/// </summary>
public sealed class RuleValidator
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    public RuleValidationResult Validate(string ruleFilePath)
    {
        _errors.Clear();
        _warnings.Clear();

        if (!File.Exists(ruleFilePath))
        {
            _errors.Add($"Rule file not found: {ruleFilePath}");
            return new RuleValidationResult(false, _errors, _warnings);
        }

        try
        {
            var json = File.ReadAllText(ruleFilePath);
            var rules = JsonSerializer.Deserialize<List<MappingRule>>(json);

            if (rules == null || rules.Count == 0)
            {
                _errors.Add("No rules found in configuration file");
                return new RuleValidationResult(false, _errors, _warnings);
            }

            ValidateRules(rules);

            return new RuleValidationResult(_errors.Count == 0, _errors, _warnings);
        }
        catch (JsonException ex)
        {
            _errors.Add($"Invalid JSON format: {ex.Message}");
            return new RuleValidationResult(false, _errors, _warnings);
        }
        catch (Exception ex)
        {
            _errors.Add($"Validation error: {ex.Message}");
            return new RuleValidationResult(false, _errors, _warnings);
        }
    }

    private void ValidateRules(List<MappingRule> rules)
    {
        var ruleNames = new HashSet<string>();
        var priorityGroups = new Dictionary<int, List<string>>();

        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            var ruleContext = $"Rule #{i + 1} ('{rule.Name}')";

            // Validate rule name
            ValidateRuleName(rule, ruleContext, ruleNames);

            // Validate match criteria
            ValidateMatchCriteria(rule, ruleContext);

            // Validate target configuration
            ValidateTarget(rule, ruleContext);

            // Validate conflict policy
            ValidateConflictPolicy(rule, ruleContext);

            // Validate priority
            ValidatePriority(rule, ruleContext, priorityGroups);
        }

        // Check for priority conflicts
        CheckPriorityConflicts(priorityGroups);
    }

    private void ValidateRuleName(MappingRule rule, string context, HashSet<string> ruleNames)
    {
        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            _errors.Add($"{context}: Rule name is required");
            return;
        }

        if (ruleNames.Contains(rule.Name))
        {
            _errors.Add($"{context}: Duplicate rule name '{rule.Name}'");
        }
        else
        {
            ruleNames.Add(rule.Name);
        }

        if (rule.Name.Length > 100)
        {
            _warnings.Add($"{context}: Rule name is very long ({rule.Name.Length} characters)");
        }

        if (rule.Name.Contains(' ') && !rule.Name.Contains('-'))
        {
            _warnings.Add($"{context}: Consider using hyphens instead of spaces in rule name");
        }
    }

    private void ValidateMatchCriteria(MappingRule rule, string context)
    {
        var hasAnyCriteria = false;

        // Check PathRegex
        if (!string.IsNullOrEmpty(rule.Match.PathRegex))
        {
            hasAnyCriteria = true;
            try
            {
                _ = new Regex(rule.Match.PathRegex, RegexOptions.Compiled);
            }
            catch (ArgumentException ex)
            {
                _errors.Add($"{context}: Invalid regex pattern: {ex.Message}");
            }

            // Check if regex is case-insensitive
            if (!rule.Match.PathRegex.Contains("(?i)", StringComparison.Ordinal))
            {
                _warnings.Add($"{context}: Regex pattern should include (?i) for case-insensitive matching");
            }
        }

        // Check Extensions
        if (rule.Match.Extensions != null && rule.Match.Extensions.Length > 0)
        {
            hasAnyCriteria = true;

            foreach (var ext in rule.Match.Extensions)
            {
                if (string.IsNullOrWhiteSpace(ext))
                {
                    _errors.Add($"{context}: Empty extension in extensions list");
                }
                else if (ext.StartsWith('.'))
                {
                    _warnings.Add($"{context}: Extension '{ext}' should not include leading dot");
                }
            }

            // Check for duplicates
            var duplicates = rule.Match.Extensions
                .GroupBy(e => e, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var dup in duplicates)
            {
                _warnings.Add($"{context}: Duplicate extension '{dup}'");
            }
        }

        // Check Age constraints
        if (rule.Match.AgeDaysMin.HasValue)
        {
            hasAnyCriteria = true;
            if (rule.Match.AgeDaysMin.Value < 0)
            {
                _errors.Add($"{context}: AgeDaysMin cannot be negative");
            }
            if (rule.Match.AgeDaysMin.Value > 3650)
            {
                _warnings.Add($"{context}: AgeDaysMin is very large ({rule.Match.AgeDaysMin} days = {rule.Match.AgeDaysMin / 365.0:F1} years)");
            }
        }

        if (rule.Match.AgeDaysMax.HasValue)
        {
            hasAnyCriteria = true;
            if (rule.Match.AgeDaysMax.Value < 0)
            {
                _errors.Add($"{context}: AgeDaysMax cannot be negative");
            }
        }

        // Check age range consistency
        if (rule.Match.AgeDaysMin.HasValue && rule.Match.AgeDaysMax.HasValue)
        {
            if (rule.Match.AgeDaysMin.Value > rule.Match.AgeDaysMax.Value)
            {
                _errors.Add($"{context}: AgeDaysMin ({rule.Match.AgeDaysMin}) cannot be greater than AgeDaysMax ({rule.Match.AgeDaysMax})");
            }
            else if (rule.Match.AgeDaysMin.Value == rule.Match.AgeDaysMax.Value)
            {
                _warnings.Add($"{context}: AgeDaysMin equals AgeDaysMax - rule will only match files of exact age");
            }
        }

        if (!hasAnyCriteria)
        {
            _errors.Add($"{context}: No match criteria specified (need at least one of: pathRegex, extensions, ageDaysMin, ageDaysMax)");
        }
    }

    private void ValidateTarget(MappingRule rule, string context)
    {
        if (string.IsNullOrWhiteSpace(rule.Target.Location))
        {
            _errors.Add($"{context}: Target location is required");
            return;
        }

        // Check if location has a provider prefix
        if (!rule.Target.Location.Contains("://", StringComparison.Ordinal))
        {
            _warnings.Add($"{context}: Target location should include provider (e.g., 'OneDrive://')");
        }

        // Check if location ends with /
        if (!rule.Target.Location.EndsWith('/'))
        {
            _warnings.Add($"{context}: Target location should end with '/' for clarity");
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (rule.Target.Location.Any(c => invalidChars.Contains(c) && c != ':'))
        {
            _errors.Add($"{context}: Target location contains invalid path characters");
        }

        // Validate naming template if specified
        if (!string.IsNullOrEmpty(rule.Target.NamingTemplate))
        {
            var validTemplates = new[] { "StandardDateContextTitle" };
            if (!validTemplates.Contains(rule.Target.NamingTemplate))
            {
                _warnings.Add($"{context}: Unknown naming template '{rule.Target.NamingTemplate}'");
            }
        }
    }

    private void ValidateConflictPolicy(MappingRule rule, string context)
    {
        var validPolicies = new[] { "VersionSuffix", "TimestampSuffix", "Overwrite", "Skip" };

        if (string.IsNullOrWhiteSpace(rule.ConflictPolicy))
        {
            _errors.Add($"{context}: Conflict policy is required");
        }
        else if (!validPolicies.Contains(rule.ConflictPolicy, StringComparer.OrdinalIgnoreCase))
        {
            _errors.Add($"{context}: Invalid conflict policy '{rule.ConflictPolicy}'. Valid options: {string.Join(", ", validPolicies)}");
        }

        // Warn about potentially dangerous policies
        if (string.Equals(rule.ConflictPolicy, "Overwrite", StringComparison.OrdinalIgnoreCase))
        {
            _warnings.Add($"{context}: 'Overwrite' conflict policy can cause data loss - use with caution");
        }
    }

    private void ValidatePriority(MappingRule rule, string context, Dictionary<int, List<string>> priorityGroups)
    {
        if (rule.Priority < 1)
        {
            _errors.Add($"{context}: Priority must be a positive integer");
        }

        if (rule.Priority > 1000)
        {
            _warnings.Add($"{context}: Very high priority ({rule.Priority}) - consider using lower values");
        }

        // Track rules by priority for conflict detection
        if (!priorityGroups.ContainsKey(rule.Priority))
        {
            priorityGroups[rule.Priority] = new List<string>();
        }
        priorityGroups[rule.Priority].Add(rule.Name);
    }

    private void CheckPriorityConflicts(Dictionary<int, List<string>> priorityGroups)
    {
        foreach (var group in priorityGroups.Where(g => g.Value.Count > 1))
        {
            _warnings.Add($"Multiple rules with priority {group.Key}: {string.Join(", ", group.Value)}. First matching rule will be used.");
        }
    }
}

/// <summary>
/// Result of rule validation
/// </summary>
public sealed record RuleValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public void PrintToConsole()
    {
        if (IsValid && Warnings.Count == 0)
        {
            Console.WriteLine("? Rule validation passed with no issues");
            return;
        }

        if (Errors.Count > 0)
        {
            Console.WriteLine($"\n? Validation Failed ({Errors.Count} error(s)):\n");
            foreach (var error in Errors)
            {
                Console.WriteLine($"  ERROR: {error}");
            }
        }

        if (Warnings.Count > 0)
        {
            Console.WriteLine($"\n? Warnings ({Warnings.Count}):\n");
            foreach (var warning in Warnings)
            {
                Console.WriteLine($"  WARNING: {warning}");
            }
        }

        if (IsValid)
        {
            Console.WriteLine($"\n? Validation passed with {Warnings.Count} warning(s)");
        }
    }
}
