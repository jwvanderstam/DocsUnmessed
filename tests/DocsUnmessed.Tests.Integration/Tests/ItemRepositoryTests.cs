namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data.Repositories;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using NUnit.Framework;

/// <summary>
/// Integration tests for ItemRepository
/// </summary>
[TestFixture]
public sealed class ItemRepositoryTests
{
    private DatabaseFixture? _fixture;
    private ItemRepository? _repository;

    [SetUp]
    public void Setup()
    {
        _fixture = new DatabaseFixture();
        _repository = new ItemRepository(_fixture.Context);
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task GetByScanAsync_ReturnsAllItemsForScan()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 10);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository!.GetByScanAsync(scan.ScanId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task GetByTypeAsync_FiltersItemsByType()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var files = TestDataGenerator.CreateTestItems(scan.ScanId, 5, "File");
        var folders = TestDataGenerator.CreateTestItems(scan.ScanId, 3, "Folder");
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(files);
        _fixture.Context.Items.AddRange(folders);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var fileResults = await _repository!.GetByTypeAsync(scan.ScanId, "File");
        var folderResults = await _repository.GetByTypeAsync(scan.ScanId, "Folder");

        // Assert
        Assert.That(fileResults.Count, Is.EqualTo(5));
        Assert.That(folderResults.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetByExtensionAsync_FiltersItemsByExtension()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var txtItems = new List<Data.Entities.ItemEntity>
        {
            TestDataGenerator.CreateTestItem(scan.ScanId, "File", "file1.txt"),
            TestDataGenerator.CreateTestItem(scan.ScanId, "File", "file2.txt")
        };
        var pdfItem = TestDataGenerator.CreateTestItem(scan.ScanId, "File", "file3.pdf");
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(txtItems);
        _fixture.Context.Items.Add(pdfItem);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var txtResults = await _repository!.GetByExtensionAsync(scan.ScanId, "txt");
        var pdfResults = await _repository.GetByExtensionAsync(scan.ScanId, "pdf");

        // Assert
        Assert.That(txtResults.Count, Is.EqualTo(2));
        Assert.That(pdfResults.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task FindDuplicatesAsync_GroupsItemsByHash()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItemsWithDuplicates(scan.ScanId, 4);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var duplicates = await _repository!.FindDuplicatesAsync(scan.ScanId);

        // Assert
        Assert.That(duplicates, Is.Not.Empty);
        Assert.That(duplicates.Count, Is.EqualTo(1)); // One duplicate group
        Assert.That(duplicates.First().Value.Count, Is.EqualTo(4)); // Four duplicates
    }

    [Test]
    public async Task GetLargeFilesAsync_ReturnsFilesAboveThreshold()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 10);
        
        // Set specific sizes
        items[0].SizeBytes = 1024 * 1024 * 10; // 10MB
        items[1].SizeBytes = 1024 * 1024 * 5;  // 5MB
        items[2].SizeBytes = 1024 * 100;       // 100KB
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var largeFiles = await _repository!.GetLargeFilesAsync(scan.ScanId, 1024 * 1024); // 1MB threshold

        // Assert
        Assert.That(largeFiles.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(largeFiles.All(f => f.SizeBytes >= 1024 * 1024), Is.True);
    }

    [Test]
    public async Task GetTotalSizeAsync_CalculatesTotalFileSize()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 5);
        
        foreach (var item in items)
        {
            item.SizeBytes = 1024; // 1KB each
        }
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var totalSize = await _repository!.GetTotalSizeAsync(scan.ScanId);

        // Assert
        Assert.That(totalSize, Is.EqualTo(5 * 1024));
    }

    [Test]
    public void GetByScanAsync_WithNullScanId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository!.GetByScanAsync(null!));
    }

    [Test]
    public void GetByScanAsync_WithEmptyScanId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository!.GetByScanAsync(string.Empty));
    }
}
