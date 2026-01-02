namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Interfaces;
using System.Text.RegularExpressions;

public interface IPathValidator
{
    ValidationResult ValidatePath(string path, ProviderLimits limits);
    ValidationResult ValidateFileName(string fileName, ProviderLimits limits);
    string SanitizeFileName(string fileName, ProviderLimits limits);
}

/// <summary>
/// Service for validating and sanitizing file paths and names
/// </summary>
public sealed class PathValidator : IPathValidator
{
    public ValidationResult ValidatePath(string path, ProviderLimits limits)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            issues.Add("Path cannot be empty");
            return new ValidationResult { IsValid = false, Issues = issues };
        }

        if (path.Length > limits.MaxPathLength)
        {
            issues.Add($"Path exceeds maximum length of {limits.MaxPathLength} characters");
        }

        var fileName = Path.GetFileName(path);
        var fileNameValidation = ValidateFileName(fileName, limits);
        if (!fileNameValidation.IsValid)
        {
            issues.AddRange(fileNameValidation.Issues);
        }

        return new ValidationResult
        {
            IsValid = issues.Count == 0,
            Issues = issues
        };
    }

    public ValidationResult ValidateFileName(string fileName, ProviderLimits limits)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            issues.Add("File name cannot be empty");
            return new ValidationResult { IsValid = false, Issues = issues };
        }

        if (fileName.Length > limits.MaxFileNameLength)
        {
            issues.Add($"File name exceeds maximum length of {limits.MaxFileNameLength} characters");
        }

        foreach (var invalidChar in limits.InvalidCharacters)
        {
            if (fileName.Contains(invalidChar))
            {
                issues.Add($"File name contains invalid character: '{invalidChar}'");
            }
        }

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        if (limits.ReservedNames.Contains(nameWithoutExt, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add($"File name '{nameWithoutExt}' is reserved by the system");
        }

        if (fileName.EndsWith('.') || fileName.EndsWith(' '))
        {
            issues.Add("File name cannot end with a period or space");
        }

        return new ValidationResult
        {
            IsValid = issues.Count == 0,
            Issues = issues
        };
    }

    public string SanitizeFileName(string fileName, ProviderLimits limits)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "unnamed";
        }

        var sanitized = fileName;

        // Replace invalid characters with underscore
        foreach (var invalidChar in limits.InvalidCharacters)
        {
            sanitized = sanitized.Replace(invalidChar, "_");
        }

        // Remove trailing periods and spaces
        sanitized = sanitized.TrimEnd('.', ' ');

        // Truncate if too long
        if (sanitized.Length > limits.MaxFileNameLength)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
            var maxNameLength = limits.MaxFileNameLength - extension.Length;
            sanitized = nameWithoutExt[..maxNameLength] + extension;
        }

        // Handle reserved names
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
        if (limits.ReservedNames.Contains(nameWithoutExtension, StringComparer.OrdinalIgnoreCase))
        {
            sanitized = $"_{sanitized}";
        }

        return sanitized;
    }
}

public sealed class ValidationResult
{
    public required bool IsValid { get; init; }
    public List<string> Issues { get; init; } = new();
}
