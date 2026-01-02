namespace DocsUnmessed.Services.Duplicates;

using DocsUnmessed.Core.Domain;
using System.Diagnostics;

/// <summary>
/// Enhanced duplicate detection service with multiple detection strategies
/// </summary>
public sealed class EnhancedDuplicateDetector
{
    private readonly SimilarityCalculator _similarityCalculator;
    private readonly DuplicateDetectionConfig _config;

    /// <summary>
    /// Initializes a new instance of the EnhancedDuplicateDetector class
    /// </summary>
    /// <param name="config">Detection configuration</param>
    public EnhancedDuplicateDetector(DuplicateDetectionConfig? config = null)
    {
        _config = config ?? DuplicateDetectionConfig.Default;
        _similarityCalculator = new SimilarityCalculator();
    }

    /// <summary>
    /// Detects duplicates in a collection of items
    /// </summary>
    /// <param name="items">Items to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detection result</returns>
    public async Task<DuplicateDetectionResult> DetectDuplicatesAsync(
        IEnumerable<Item> items,
        CancellationToken cancellationToken = default)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var stopwatch = Stopwatch.StartNew();
        var matches = new List<DuplicateMatch>();
        var itemList = items.Where(i => i.Type == ItemType.File && i.Size >= _config.MinimumFileSize).ToList();

        // Strategy 1: Exact hash matching
        if (_config.UseExactHash)
        {
            var hashMatches = await Task.Run(() => DetectExactHashDuplicates(itemList), cancellationToken);
            matches.AddRange(hashMatches);
        }

        // Strategy 2: Partial hash matching (for large files)
        if (_config.UsePartialHash)
        {
            var partialMatches = await Task.Run(() => DetectPartialHashDuplicates(itemList), cancellationToken);
            matches.AddRange(partialMatches);
        }

        // Strategy 3: Name similarity
        if (_config.UseNameSimilarity)
        {
            var nameMatches = await Task.Run(() => DetectNameSimilarityDuplicates(itemList), cancellationToken);
            matches.AddRange(nameMatches);
        }

        // Strategy 4: Size and date grouping
        if (_config.UseSizeGrouping || _config.UseDateGrouping)
        {
            var sizeMatches = await Task.Run(() => DetectSizeAndDateDuplicates(itemList), cancellationToken);
            matches.AddRange(sizeMatches);
        }

        stopwatch.Stop();

        // Remove duplicate matches
        matches = DeduplicateMatches(matches);

        return new DuplicateDetectionResult
        {
            Matches = matches,
            ItemsAnalyzed = itemList.Count,
            Duration = stopwatch.Elapsed,
            Configuration = _config
        };
    }

    private List<DuplicateMatch> DetectExactHashDuplicates(List<Item> items)
    {
        var matches = new List<DuplicateMatch>();
        
        var hashGroups = items
            .Where(i => !string.IsNullOrEmpty(i.Hash))
            .GroupBy(i => i.Hash!)
            .Where(g => g.Count() > 1);

        foreach (var group in hashGroups)
        {
            var groupItems = group.ToList();
            for (int i = 0; i < groupItems.Count - 1; i++)
            {
                for (int j = i + 1; j < groupItems.Count; j++)
                {
                    matches.Add(new DuplicateMatch
                    {
                        ItemId1 = groupItems[i].Path ?? string.Empty,
                        ItemId2 = groupItems[j].Path ?? string.Empty,
                        Confidence = 1.0,
                        Method = DuplicateDetectionMethod.ExactHash,
                        Details = $"Identical hash: {group.Key}"
                    });
                }
            }
        }

        return matches;
    }

    private List<DuplicateMatch> DetectPartialHashDuplicates(List<Item> items)
    {
        var matches = new List<DuplicateMatch>();
        
        // Group by partial hash (first N bytes hash)
        // For now, we'll use the first 8 characters of the hash as "partial"
        var partialHashGroups = items
            .Where(i => !string.IsNullOrEmpty(i.Hash) && i.Hash!.Length >= 8)
            .GroupBy(i => i.Hash![..8])
            .Where(g => g.Count() > 1);

        foreach (var group in partialHashGroups)
        {
            var groupItems = group.ToList();
            for (int i = 0; i < groupItems.Count - 1; i++)
            {
                for (int j = i + 1; j < groupItems.Count; j++)
                {
                    // Check if they're not already matched by exact hash
                    if (groupItems[i].Hash != groupItems[j].Hash)
                    {
                        matches.Add(new DuplicateMatch
                        {
                            ItemId1 = groupItems[i].Path ?? string.Empty,
                            ItemId2 = groupItems[j].Path ?? string.Empty,
                            Confidence = 0.9,
                            Method = DuplicateDetectionMethod.PartialHash,
                            Details = "Partial hash match (potential duplicate)"
                        });
                    }
                }
            }
        }

        return matches;
    }

    private List<DuplicateMatch> DetectNameSimilarityDuplicates(List<Item> items)
    {
        var matches = new List<DuplicateMatch>();

        // Only compare items with similar sizes (within threshold)
        var sizeGroups = items
            .GroupBy(i => i.Size / 1024) // Group by KB
            .Where(g => g.Count() > 1);

        foreach (var sizeGroup in sizeGroups)
        {
            var groupItems = sizeGroup.ToList();
            
            for (int i = 0; i < groupItems.Count - 1; i++)
            {
                for (int j = i + 1; j < groupItems.Count; j++)
                {
                    var item1 = groupItems[i];
                    var item2 = groupItems[j];

                    var nameSimilarity = _similarityCalculator.CalculateFileNameSimilarity(
                        item1.Name, item2.Name);

                    if (nameSimilarity >= _config.NameSimilarityThreshold)
                    {
                        var sizeSimilarity = _similarityCalculator.CalculateSizeSimilarity(
                            item1.Size, item2.Size);

                        if (sizeSimilarity >= (1.0 - _config.MaxSizeDifferencePercent))
                        {
                            var confidence = (nameSimilarity + sizeSimilarity) / 2.0;

                            matches.Add(new DuplicateMatch
                            {
                                ItemId1 = item1.Path ?? string.Empty,
                                ItemId2 = item2.Path ?? string.Empty,
                                Confidence = confidence,
                                Method = DuplicateDetectionMethod.NameSimilarity,
                                Details = $"Name similarity: {nameSimilarity:P0}, Size similarity: {sizeSimilarity:P0}"
                            });
                        }
                    }
                }
            }
        }

        return matches;
    }

    private List<DuplicateMatch> DetectSizeAndDateDuplicates(List<Item> items)
    {
        var matches = new List<DuplicateMatch>();

        // Group by exact size
        var sizeGroups = items
            .GroupBy(i => i.Size)
            .Where(g => g.Count() > 1);

        foreach (var sizeGroup in sizeGroups)
        {
            var groupItems = sizeGroup.ToList();

            for (int i = 0; i < groupItems.Count - 1; i++)
            {
                for (int j = i + 1; j < groupItems.Count; j++)
                {
                    var item1 = groupItems[i];
                    var item2 = groupItems[j];

                    var dateSimilarity = 1.0;
                    
                    if (_config.UseDateGrouping)
                    {
                        dateSimilarity = _similarityCalculator.CalculateDateSimilarity(
                            item1.ModifiedUtc, 
                            item2.ModifiedUtc, 
                            _config.MaxDateDifferenceHours);
                    }

                    if (dateSimilarity >= 0.8) // 80% date similarity threshold
                    {
                        var confidence = 0.7 * dateSimilarity; // Lower confidence for size+date only

                        matches.Add(new DuplicateMatch
                        {
                            ItemId1 = item1.Path ?? string.Empty,
                            ItemId2 = item2.Path ?? string.Empty,
                            Confidence = confidence,
                            Method = DuplicateDetectionMethod.SizeAndDate,
                            Details = $"Same size ({item1.Size} bytes), similar dates"
                        });
                    }
                }
            }
        }

        return matches;
    }

    private static List<DuplicateMatch> DeduplicateMatches(List<DuplicateMatch> matches)
    {
        // Remove duplicate matches (same pair of items)
        var uniqueMatches = new Dictionary<string, DuplicateMatch>();

        foreach (var match in matches)
        {
            // Create a unique key for the pair (order-independent)
            var key = string.CompareOrdinal(match.ItemId1, match.ItemId2) < 0
                ? $"{match.ItemId1}|{match.ItemId2}"
                : $"{match.ItemId2}|{match.ItemId1}";

            // Keep the match with highest confidence
            if (!uniqueMatches.TryGetValue(key, out var existing) || 
                match.Confidence > existing.Confidence)
            {
                uniqueMatches[key] = match;
            }
        }

        return uniqueMatches.Values.OrderByDescending(m => m.Confidence).ToList();
    }
}
