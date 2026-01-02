namespace DocsUnmessed.Services.Duplicates;

/// <summary>
/// Calculates similarity between strings and files
/// </summary>
public sealed class SimilarityCalculator
{
    /// <summary>
    /// Calculates Levenshtein distance between two strings
    /// </summary>
    /// <param name="source">First string</param>
    /// <param name="target">Second string</param>
    /// <returns>Edit distance</returns>
    public int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;

        // Create matrix
        var matrix = new int[sourceLength + 1, targetLength + 1];

        // Initialize first column and row
        for (int i = 0; i <= sourceLength; i++)
        {
            matrix[i, 0] = i;
        }

        for (int j = 0; j <= targetLength; j++)
        {
            matrix[0, j] = j;
        }

        // Calculate distances
        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(
                        matrix[i - 1, j] + 1,      // Deletion
                        matrix[i, j - 1] + 1),     // Insertion
                    matrix[i - 1, j - 1] + cost);  // Substitution
            }
        }

        return matrix[sourceLength, targetLength];
    }

    /// <summary>
    /// Calculates similarity ratio between two strings (0.0 to 1.0)
    /// </summary>
    /// <param name="source">First string</param>
    /// <param name="target">Second string</param>
    /// <returns>Similarity ratio (1.0 = identical, 0.0 = completely different)</returns>
    public double CalculateStringSimilarity(string source, string target)
    {
        if (string.Equals(source, target, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return 0.0;
        }

        var distance = CalculateLevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        return 1.0 - ((double)distance / maxLength);
    }

    /// <summary>
    /// Calculates normalized file name similarity
    /// </summary>
    /// <param name="name1">First file name</param>
    /// <param name="name2">Second file name</param>
    /// <returns>Similarity ratio</returns>
    public double CalculateFileNameSimilarity(string name1, string name2)
    {
        if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
        {
            return 0.0;
        }

        // Normalize names (lowercase, remove extensions)
        var normalized1 = NormalizeFileName(name1);
        var normalized2 = NormalizeFileName(name2);

        return CalculateStringSimilarity(normalized1, normalized2);
    }

    /// <summary>
    /// Calculates size similarity based on difference percentage
    /// </summary>
    /// <param name="size1">First size in bytes</param>
    /// <param name="size2">Second size in bytes</param>
    /// <returns>Similarity ratio (1.0 = identical, 0.0 = very different)</returns>
    public double CalculateSizeSimilarity(long size1, long size2)
    {
        if (size1 == size2)
        {
            return 1.0;
        }

        if (size1 == 0 || size2 == 0)
        {
            return 0.0;
        }

        var maxSize = Math.Max(size1, size2);
        var difference = Math.Abs(size1 - size2);
        var differencePercent = (double)difference / maxSize;

        return Math.Max(0.0, 1.0 - differencePercent);
    }

    /// <summary>
    /// Calculates date similarity based on time difference
    /// </summary>
    /// <param name="date1">First date</param>
    /// <param name="date2">Second date</param>
    /// <param name="maxDifferenceHours">Maximum difference in hours to consider similar</param>
    /// <returns>Similarity ratio</returns>
    public double CalculateDateSimilarity(DateTime date1, DateTime date2, int maxDifferenceHours)
    {
        var difference = Math.Abs((date1 - date2).TotalHours);

        if (difference == 0)
        {
            return 1.0;
        }

        if (difference >= maxDifferenceHours)
        {
            return 0.0;
        }

        return 1.0 - (difference / maxDifferenceHours);
    }

    /// <summary>
    /// Calculates combined similarity score
    /// </summary>
    /// <param name="nameSimilarity">Name similarity (0-1)</param>
    /// <param name="sizeSimilarity">Size similarity (0-1)</param>
    /// <param name="dateSimilarity">Date similarity (0-1)</param>
    /// <param name="weights">Weights for each factor (must sum to 1.0)</param>
    /// <returns>Combined similarity score</returns>
    public double CalculateCombinedSimilarity(
        double nameSimilarity,
        double sizeSimilarity,
        double dateSimilarity,
        (double name, double size, double date) weights)
    {
        if (Math.Abs(weights.name + weights.size + weights.date - 1.0) > 0.01)
        {
            throw new ArgumentException("Weights must sum to 1.0", nameof(weights));
        }

        return (nameSimilarity * weights.name) +
               (sizeSimilarity * weights.size) +
               (dateSimilarity * weights.date);
    }

    private static string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        // Remove extension
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // Lowercase
        var normalized = nameWithoutExt.ToLowerInvariant();

        // Remove common separators
        normalized = normalized.Replace('-', ' ')
                              .Replace('_', ' ')
                              .Replace('.', ' ');

        // Remove extra spaces
        normalized = string.Join(" ", normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return normalized;
    }
}
