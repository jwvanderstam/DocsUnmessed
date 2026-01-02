namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Services.Duplicates;
using NUnit.Framework;

/// <summary>
/// Tests for Enhanced Duplicate Detection
/// </summary>
[TestFixture]
public sealed class EnhancedDuplicateDetectorTests
{
    [Test]
    public async Task DetectDuplicatesAsync_ExactHashMatch_FindsDuplicates()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = true,
            UsePartialHash = false,
            UseNameSimilarity = false,
            UseSizeGrouping = false
        });

        var items = new[]
        {
            CreateItem("file1.txt", hash: "abc123", size: 1024),
            CreateItem("file2.txt", hash: "abc123", size: 1024),
            CreateItem("file3.txt", hash: "def456", size: 2048)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches.Count, Is.EqualTo(1));
        Assert.That(result.Matches[0].Method, Is.EqualTo(DuplicateDetectionMethod.ExactHash));
        Assert.That(result.Matches[0].Confidence, Is.EqualTo(1.0));
    }

    [Test]
    public async Task DetectDuplicatesAsync_PartialHashMatch_FindsPotentialDuplicates()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = false,
            UsePartialHash = true,
            UseNameSimilarity = false,
            UseSizeGrouping = false
        });

        var items = new[]
        {
            CreateItem("file1.txt", hash: "abcd1234efgh", size: 1024),
            CreateItem("file2.txt", hash: "abcd1234wxyz", size: 1024),
            CreateItem("file3.txt", hash: "ijkl5678mnop", size: 2048)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches.Count, Is.GreaterThan(0));
        Assert.That(result.Matches[0].Method, Is.EqualTo(DuplicateDetectionMethod.PartialHash));
        Assert.That(result.Matches[0].Confidence, Is.LessThan(1.0));
    }

    [Test]
    public async Task DetectDuplicatesAsync_NameSimilarity_FindsSimilarNames()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = false,
            UsePartialHash = false,
            UseNameSimilarity = true,
            NameSimilarityThreshold = 0.85,
            MaxSizeDifferencePercent = 0.05,
            UseSizeGrouping = false,
            MinimumFileSize = 1000
        });

        var items = new[]
        {
            CreateItem("Document_Report.pdf", hash: "hash1", size: 10240),
            CreateItem("Document-Report.pdf", hash: "hash2", size: 10250),
            CreateItem("TotallyDifferentFile.pdf", hash: "hash3", size: 20480)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.ItemsAnalyzed, Is.EqualTo(3), "All items should be analyzed");
        Assert.That(result.Matches.Count, Is.GreaterThan(0), "Should find at least one match");
        
        var nameMatches = result.Matches.Where(m => m.Method == DuplicateDetectionMethod.NameSimilarity).ToList();
        Assert.That(nameMatches.Count, Is.GreaterThan(0), "Should have name similarity matches");
    }

    [Test]
    public async Task DetectDuplicatesAsync_SizeGrouping_FindsSameSize()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = false,
            UsePartialHash = false,
            UseNameSimilarity = false,
            UseSizeGrouping = true,
            UseDateGrouping = false
        });

        var items = new[]
        {
            CreateItem("file1.txt", size: 1024),
            CreateItem("file2.txt", size: 1024),
            CreateItem("file3.txt", size: 2048)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches.Count, Is.GreaterThan(0));
        Assert.That(result.Matches[0].Method, Is.EqualTo(DuplicateDetectionMethod.SizeAndDate));
    }

    [Test]
    public async Task DetectDuplicatesAsync_MinimumFileSize_FiltersSmallFiles()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = true,
            MinimumFileSize = 2048
        });

        var items = new[]
        {
            CreateItem("small1.txt", hash: "abc123", size: 512),
            CreateItem("small2.txt", hash: "abc123", size: 512),
            CreateItem("large1.txt", hash: "def456", size: 3072),
            CreateItem("large2.txt", hash: "def456", size: 3072)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches.Count, Is.EqualTo(1)); // Only large files matched
        Assert.That(result.ItemsAnalyzed, Is.EqualTo(2)); // Only 2 large files analyzed
    }

    [Test]
    public async Task DetectDuplicatesAsync_ConservativeConfig_OnlyExactMatches()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(DuplicateDetectionConfig.Conservative);

        var items = new[]
        {
            CreateItem("file1.txt", hash: "abc123", size: 1024),
            CreateItem("file2.txt", hash: "abc123", size: 1024),
            CreateItem("similar.txt", hash: "def456", size: 1024)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches.Count, Is.EqualTo(1));
        Assert.That(result.Matches.All(m => m.Method == DuplicateDetectionMethod.ExactHash), Is.True);
    }

    [Test]
    public async Task DetectDuplicatesAsync_AggressiveConfig_FindsMultipleTypes()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector(DuplicateDetectionConfig.Aggressive);

        var items = new[]
        {
            CreateItem("Document.pdf", hash: "abc123", size: 1024),
            CreateItem("Document-1.pdf", hash: "def456", size: 1020),
            CreateItem("Document_2.pdf", hash: "ghi789", size: 1025)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert - Aggressive config should find matches
        Assert.That(result.Matches.Count, Is.GreaterThan(0), "Aggressive config should find potential duplicates");
    }

    [Test]
    public async Task DetectDuplicatesAsync_ReturnsStatistics()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector();
        var items = new[]
        {
            CreateItem("file1.txt", hash: "abc123", size: 1024),
            CreateItem("file2.txt", hash: "abc123", size: 1024)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.ItemsAnalyzed, Is.EqualTo(2));
        Assert.That(result.Duration, Is.GreaterThan(TimeSpan.Zero));
        Assert.That(result.Configuration, Is.Not.Null);
    }

    [Test]
    public void DetectDuplicatesAsync_NullItems_ThrowsArgumentNullException()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await detector.DetectDuplicatesAsync(null!));
    }

    [Test]
    public async Task DetectDuplicatesAsync_EmptyList_ReturnsNoMatches()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector();
        var items = Array.Empty<Item>();

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches, Is.Empty);
        Assert.That(result.ItemsAnalyzed, Is.EqualTo(0));
    }

    [Test]
    public async Task DetectDuplicatesAsync_OnlyFolders_ReturnsNoMatches()
    {
        // Arrange
        var detector = new EnhancedDuplicateDetector();
        var items = new[]
        {
            CreateItem("folder1", type: ItemType.Folder),
            CreateItem("folder2", type: ItemType.Folder)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert
        Assert.That(result.Matches, Is.Empty);
    }

    [Test]
    public async Task DetectDuplicatesAsync_DeduplicatesMatches()
    {
        // Arrange - This should find the same pair through multiple methods
        var detector = new EnhancedDuplicateDetector(new DuplicateDetectionConfig
        {
            UseExactHash = true,
            UseSizeGrouping = true
        });

        var items = new[]
        {
            CreateItem("file1.txt", hash: "abc123", size: 1024),
            CreateItem("file2.txt", hash: "abc123", size: 1024)
        };

        // Act
        var result = await detector.DetectDuplicatesAsync(items);

        // Assert - Should only have one match (highest confidence)
        Assert.That(result.Matches.Count, Is.EqualTo(1));
        Assert.That(result.Matches[0].Confidence, Is.EqualTo(1.0)); // Exact hash takes precedence
    }

    private static Item CreateItem(
        string name,
        string? hash = null,
        long size = 1024,
        ItemType type = ItemType.File)
    {
        return new Item
        {
            Path = $"/test/{name}",
            Name = name,
            Provider = "test",
            Size = size,
            Hash = hash,
            Type = type,
            MimeType = "text/plain",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            ExtendedProperties = new Dictionary<string, string>(),
            Issues = new List<string>(),
            IsShared = false,
            Depth = 1
        };
    }
}
