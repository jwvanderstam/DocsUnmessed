namespace DocsUnmessed.Tests.Integration.Helpers;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Data.Entities;

/// <summary>
/// Helper class for generating test data
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// Creates a test scan entity
    /// </summary>
    /// <param name="scanId">Optional scan ID</param>
    /// <param name="providerId">Optional provider ID</param>
    /// <returns>Test scan entity</returns>
    public static ScanEntity CreateTestScan(string? scanId = null, string? providerId = null)
    {
        return new ScanEntity
        {
            ScanId = scanId ?? Guid.NewGuid().ToString("N")[..12],
            ProviderId = providerId ?? "test_provider",
            RootPath = "C:/TestPath",
            Status = "Complete",
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow,
            TotalItems = 0,
            TotalFiles = 0,
            TotalFolders = 0,
            TotalSize = 0,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test item entity
    /// </summary>
    /// <param name="scanId">Scan ID to associate with</param>
    /// <param name="type">Item type (File or Folder)</param>
    /// <param name="name">Optional item name</param>
    /// <returns>Test item entity</returns>
    public static ItemEntity CreateTestItem(
        string scanId, 
        string type = "File",
        string? name = null)
    {
        var itemName = name ?? $"test-{Guid.NewGuid():N}.txt";
        var extension = type == "File" ? Path.GetExtension(itemName)?.TrimStart('.') : null;

        return new ItemEntity
        {
            ItemId = Guid.NewGuid().ToString("N"),
            ScanId = scanId,
            Type = type,
            Name = itemName,
            Path = $"C:/TestPath/{itemName}",
            Extension = extension,
            SizeBytes = type == "File" ? Random.Shared.Next(1024, 1024 * 1024) : 0,
            Hash = type == "File" ? Guid.NewGuid().ToString("N") : null,
            CreatedUtc = DateTime.UtcNow.AddDays(-30),
            ModifiedUtc = DateTime.UtcNow.AddDays(-1),
            ParentPath = "C:/TestPath",
            Depth = 1
        };
    }

    /// <summary>
    /// Creates multiple test items
    /// </summary>
    /// <param name="scanId">Scan ID to associate with</param>
    /// <param name="count">Number of items to create</param>
    /// <param name="type">Item type</param>
    /// <returns>Collection of test items</returns>
    public static List<ItemEntity> CreateTestItems(
        string scanId,
        int count,
        string type = "File")
    {
        var items = new List<ItemEntity>(count);
        for (int i = 0; i < count; i++)
        {
            items.Add(CreateTestItem(scanId, type, $"test-file-{i:D4}.txt"));
        }
        return items;
    }

    /// <summary>
    /// Creates test items with duplicate hashes for duplicate detection testing
    /// </summary>
    /// <param name="scanId">Scan ID to associate with</param>
    /// <param name="duplicateCount">Number of duplicates to create</param>
    /// <returns>Collection of test items with duplicates</returns>
    public static List<ItemEntity> CreateTestItemsWithDuplicates(
        string scanId,
        int duplicateCount = 3)
    {
        var items = new List<ItemEntity>();
        var duplicateHash = Guid.NewGuid().ToString("N");

        // Create duplicates with same hash
        for (int i = 0; i < duplicateCount; i++)
        {
            var item = CreateTestItem(scanId, "File", $"duplicate-{i}.txt");
            item.Hash = duplicateHash;
            item.SizeBytes = 1024; // Same size
            items.Add(item);
        }

        // Add some unique items
        for (int i = 0; i < 5; i++)
        {
            items.Add(CreateTestItem(scanId, "File", $"unique-{i}.txt"));
        }

        return items;
    }

    /// <summary>
    /// Creates a test domain Item
    /// </summary>
    /// <param name="name">Optional item name</param>
    /// <returns>Test domain item</returns>
    public static Item CreateTestDomainItem(string? name = null)
    {
        var itemName = name ?? $"test-{Guid.NewGuid():N}.txt";

        return new Item
        {
            Path = $"C:/TestPath/{itemName}",
            Name = itemName,
            Provider = "test_provider",
            Size = Random.Shared.Next(1024, 1024 * 1024),
            MimeType = "text/plain",
            CreatedUtc = DateTime.UtcNow.AddDays(-30),
            ModifiedUtc = DateTime.UtcNow.AddDays(-1),
            Hash = Guid.NewGuid().ToString("N"),
            Type = ItemType.File,
            ExtendedProperties = new Dictionary<string, string>(),
            Issues = new List<string>(),
            IsShared = false,
            Depth = 1
        };
    }

    /// <summary>
    /// Creates multiple test domain items
    /// </summary>
    /// <param name="count">Number of items to create</param>
    /// <returns>Collection of test domain items</returns>
    public static List<Item> CreateTestDomainItems(int count)
    {
        var items = new List<Item>(count);
        for (int i = 0; i < count; i++)
        {
            items.Add(CreateTestDomainItem($"test-file-{i:D4}.txt"));
        }
        return items;
    }
}
