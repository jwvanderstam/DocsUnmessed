namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data;
using DocsUnmessed.Data.Interfaces;
using DocsUnmessed.Services;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Integration tests for DatabaseInventoryService
/// </summary>
[TestFixture]
public sealed class DatabaseInventoryServiceTests
{
    private DatabaseFixture? _fixture;
    private IUnitOfWork? _unitOfWork;
    private DatabaseInventoryService? _service;

    [SetUp]
    public void Setup()
    {
        _fixture = new DatabaseFixture();
        
        // Create a new context for UnitOfWork - it will dispose this context
        var context = _fixture.CreateNewContext();
        _unitOfWork = new UnitOfWork(context);
        _service = new DatabaseInventoryService(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        // UnitOfWork disposes its context, so just dispose UnitOfWork
        _unitOfWork?.Dispose();
        
        // Fixture manages the shared in-memory database
        _fixture?.Dispose();
    }

    [Test]
    public async Task CreateScanAsync_CreatesNewScan_ReturnsScanId()
    {
        // Arrange
        var providers = new[] { "test_provider" };

        // Act
        var scanId = await _service!.CreateScanAsync(providers);

        // Assert
        Assert.That(scanId, Is.Not.Null);
        Assert.That(scanId, Is.Not.Empty);

        // Verify in database using a fresh context
        using var verifyContext = _fixture!.CreateNewContext();
        var scan = await verifyContext.Scans.FindAsync(scanId);
        Assert.That(scan, Is.Not.Null);
        Assert.That(scan!.Status, Is.EqualTo("Running"));
    }

    [Test]
    public async Task AddItemsAsync_AddsItemsToDatabase_UpdatesStatistics()
    {
        // Arrange
        var scanId = await _service!.CreateScanAsync(new[] { "test_provider" });
        var items = TestDataGenerator.CreateTestDomainItems(10);

        // Act
        await _service.AddItemsAsync(scanId, items);

        // Assert - verify using fresh context
        using var verifyContext = _fixture!.CreateNewContext();
        var storedItems = await verifyContext.Items
            .Where(i => i.ScanId == scanId)
            .ToListAsync();
        Assert.That(storedItems.Count, Is.EqualTo(10));

        var scan = await verifyContext.Scans.FindAsync(scanId);
        Assert.That(scan, Is.Not.Null);
        Assert.That(scan!.TotalItems, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetScanResultAsync_ReturnsCompleteResult_WithAllItems()
    {
        // Arrange
        var scanId = await _service!.CreateScanAsync(new[] { "test_provider" });
        var items = TestDataGenerator.CreateTestDomainItems(5);
        await _service.AddItemsAsync(scanId, items);
        await _service.CompleteScanAsync(scanId);

        // Act
        var result = await _service.GetScanResultAsync(scanId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ScanId, Is.EqualTo(scanId));
        Assert.That(result.Items.Count, Is.EqualTo(5));
        Assert.That(result.CompletedUtc, Is.Not.Null);
    }

    [Test]
    public async Task FindDuplicatesAsync_IdentifiesDuplicatesByHash()
    {
        // Arrange
        var scanId = await _service!.CreateScanAsync(new[] { "test_provider" });
        
        // Add test data directly to database
        using (var setupContext = _fixture!.CreateNewContext())
        {
            var items = TestDataGenerator.CreateTestItemsWithDuplicates(scanId, duplicateCount: 3);
            setupContext.Items.AddRange(items);
            await setupContext.SaveChangesAsync();
        }

        // Act
        var duplicates = await _service.FindDuplicatesAsync(scanId);

        // Assert
        Assert.That(duplicates, Is.Not.Empty);
        Assert.That(duplicates.Count, Is.EqualTo(1)); // One duplicate group
        Assert.That(duplicates[0].Items.Count, Is.EqualTo(3)); // Three duplicates
    }

    [Test]
    public async Task GetStatisticsAsync_ReturnsAccurateStatistics()
    {
        // Arrange
        var scanId = await _service!.CreateScanAsync(new[] { "test_provider" });
        var items = TestDataGenerator.CreateTestDomainItems(20);
        await _service.AddItemsAsync(scanId, items);

        // Act
        var statistics = await _service.GetStatisticsAsync(scanId);

        // Assert
        Assert.That(statistics, Is.Not.Null);
        Assert.That(statistics.TotalFiles, Is.GreaterThan(0));
    }

    [Test]
    public async Task CompleteScanAsync_UpdatesScanStatus()
    {
        // Arrange
        var scanId = await _service!.CreateScanAsync(new[] { "test_provider" });

        // Act
        await _service.CompleteScanAsync(scanId);

        // Assert - verify using fresh context
        using var verifyContext = _fixture!.CreateNewContext();
        var scan = await verifyContext.Scans.FindAsync(scanId);
        Assert.That(scan, Is.Not.Null);
        Assert.That(scan!.Status, Is.EqualTo("Complete"));
        Assert.That(scan.CompletedAt, Is.Not.Null);
    }

    [Test]
    public void CreateScanAsync_WithNullProviders_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _service!.CreateScanAsync(null!));
    }

    [Test]
    public void AddItemsAsync_WithInvalidScanId_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = TestDataGenerator.CreateTestDomainItems(5);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service!.AddItemsAsync("invalid-scan-id", items));
    }
}
