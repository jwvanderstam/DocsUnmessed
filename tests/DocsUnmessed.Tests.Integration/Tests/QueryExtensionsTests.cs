namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data.Extensions;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Integration tests for QueryExtensions
/// </summary>
[TestFixture]
public sealed class QueryExtensionsTests
{
    private DatabaseFixture? _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new DatabaseFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task Paginate_ReturnsCorrectPageSize()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 50);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var page1 = await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name)
            .Paginate(page: 1, pageSize: 10)
            .ToListAsync();

        // Assert
        Assert.That(page1.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task Paginate_DifferentPages_ReturnDifferentData()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 30);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var query = _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name);

        var page1 = await query.Paginate(page: 1, pageSize: 10).ToListAsync();
        var page2 = await query.Paginate(page: 2, pageSize: 10).ToListAsync();
        var page3 = await query.Paginate(page: 3, pageSize: 10).ToListAsync();

        // Assert
        Assert.That(page1.Count, Is.EqualTo(10));
        Assert.That(page2.Count, Is.EqualTo(10));
        Assert.That(page3.Count, Is.EqualTo(10));
        
        // Verify different data
        Assert.That(page1[0].ItemId, Is.Not.EqualTo(page2[0].ItemId));
        Assert.That(page2[0].ItemId, Is.Not.EqualTo(page3[0].ItemId));
    }

    [Test]
    public async Task ToPagedResultAsync_ReturnsCorrectMetadata()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 45);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name)
            .ToPagedResultAsync(page: 1, pageSize: 10);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(10));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.TotalCount, Is.EqualTo(45));
        Assert.That(result.TotalPages, Is.EqualTo(5));
        Assert.That(result.HasPreviousPage, Is.False);
        Assert.That(result.HasNextPage, Is.True);
    }

    [Test]
    public async Task ToPagedResultAsync_LastPage_CorrectMetadata()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 25);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name)
            .ToPagedResultAsync(page: 3, pageSize: 10);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(5)); // Last page has 5 items
        Assert.That(result.Page, Is.EqualTo(3));
        Assert.That(result.TotalPages, Is.EqualTo(3));
        Assert.That(result.HasPreviousPage, Is.True);
        Assert.That(result.HasNextPage, Is.False);
    }

    [Test]
    public async Task ProcessInBatchesAsync_ProcessesAllItems()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 35);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        var processedCount = 0;
        var batchCount = 0;

        // Act
        await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .ProcessInBatchesAsync(
                batchSize: 10,
                async batch =>
                {
                    await Task.CompletedTask;
                    processedCount += batch.Count();
                    batchCount++;
                });

        // Assert
        Assert.That(processedCount, Is.EqualTo(35));
        Assert.That(batchCount, Is.EqualTo(4)); // 10 + 10 + 10 + 5
    }

    [Test]
    public async Task ProcessInBatchesAsync_HandlesEmptyQuery()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        var batchCount = 0;

        // Act
        await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .ProcessInBatchesAsync(
                batchSize: 10,
                async batch =>
                {
                    await Task.CompletedTask;
                    batchCount++;
                });

        // Assert
        Assert.That(batchCount, Is.EqualTo(0)); // No batches for empty query
    }

    [Test]
    public void Paginate_WithInvalidPage_ThrowsArgumentException()
    {
        // Arrange
        var query = _fixture!.Context.Items.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            query.Paginate(page: 0, pageSize: 10));

        Assert.Throws<ArgumentException>(() =>
            query.Paginate(page: -1, pageSize: 10));
    }

    [Test]
    public void Paginate_WithInvalidPageSize_ThrowsArgumentException()
    {
        // Arrange
        var query = _fixture!.Context.Items.AsQueryable();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            query.Paginate(page: 1, pageSize: 0));

        Assert.Throws<ArgumentException>(() =>
            query.Paginate(page: 1, pageSize: -1));
    }

    [Test]
    public void ProcessInBatchesAsync_WithInvalidBatchSize_ThrowsArgumentException()
    {
        // Arrange
        var query = _fixture!.Context.Items.AsQueryable();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await query.ProcessInBatchesAsync(0, async batch => await Task.CompletedTask));

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await query.ProcessInBatchesAsync(-1, async batch => await Task.CompletedTask));
    }

    [Test]
    public void ProcessInBatchesAsync_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var query = _fixture!.Context.Items.AsQueryable();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await query.ProcessInBatchesAsync(10, null!));
    }
}
