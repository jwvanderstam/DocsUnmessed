namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data.Repositories;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

/// <summary>
/// Integration tests for ScanRepository
/// </summary>
[TestFixture]
public sealed class ScanRepositoryTests
{
    private DatabaseFixture? _fixture;
    private ScanRepository? _repository;

    [SetUp]
    public void Setup()
    {
        _fixture = new DatabaseFixture();
        _repository = new ScanRepository(_fixture.Context);
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ExistingScan_ReturnsScan()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository!.GetByIdAsync(scan.ScanId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ScanId, Is.EqualTo(scan.ScanId));
        Assert.That(result.ProviderId, Is.EqualTo(scan.ProviderId));
    }

    [Test]
    public async Task GetByIdAsync_NonExistentScan_ReturnsNull()
    {
        // Act
        var result = await _repository!.GetByIdAsync("non-existent-id");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByProviderAsync_FiltersScansCorrectly()
    {
        // Arrange
        var scan1 = TestDataGenerator.CreateTestScan(providerId: "provider1");
        var scan2 = TestDataGenerator.CreateTestScan(providerId: "provider1");
        var scan3 = TestDataGenerator.CreateTestScan(providerId: "provider2");
        
        _fixture!.Context.Scans.AddRange(scan1, scan2, scan3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var provider1Scans = await _repository!.GetByProviderAsync("provider1");
        var provider2Scans = await _repository.GetByProviderAsync("provider2");

        // Assert
        Assert.That(provider1Scans.Count, Is.EqualTo(2));
        Assert.That(provider2Scans.Count, Is.EqualTo(1));
        Assert.That(provider1Scans.All(s => s.ProviderId == "provider1"), Is.True);
    }

    [Test]
    public async Task GetByStatusAsync_FiltersScansCorrectly()
    {
        // Arrange
        var runningScans = new[]
        {
            TestDataGenerator.CreateTestScan(),
            TestDataGenerator.CreateTestScan()
        };
        foreach (var scan in runningScans)
        {
            scan.Status = "Running";
        }

        var completeScan = TestDataGenerator.CreateTestScan();
        completeScan.Status = "Complete";
        
        _fixture!.Context.Scans.AddRange(runningScans);
        _fixture.Context.Scans.Add(completeScan);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var running = await _repository!.GetByStatusAsync("Running");
        var complete = await _repository.GetByStatusAsync("Complete");

        // Assert
        Assert.That(running.Count, Is.EqualTo(2));
        Assert.That(complete.Count, Is.EqualTo(1));
        Assert.That(running.All(s => s.Status == "Running"), Is.True);
    }

    [Test]
    public async Task GetRecentAsync_ReturnsScansInDescendingOrder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var scan1 = TestDataGenerator.CreateTestScan();
        scan1.StartedAt = now.AddHours(-3);
        
        var scan2 = TestDataGenerator.CreateTestScan();
        scan2.StartedAt = now.AddHours(-2);
        
        var scan3 = TestDataGenerator.CreateTestScan();
        scan3.StartedAt = now.AddHours(-1);
        
        _fixture!.Context.Scans.AddRange(scan1, scan2, scan3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var recentScans = await _repository!.GetRecentAsync(count: 2);

        // Assert
        Assert.That(recentScans.Count, Is.EqualTo(2));
        Assert.That(recentScans[0].ScanId, Is.EqualTo(scan3.ScanId)); // Most recent
        Assert.That(recentScans[1].ScanId, Is.EqualTo(scan2.ScanId));
    }

    [Test]
    public async Task GetWithItemsAsync_IncludesRelatedItems()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 5);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository!.GetWithItemsAsync(scan.ScanId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task AddAsync_AddsNewScan()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();

        // Act
        await _repository!.AddAsync(scan);
        await _fixture!.Context.SaveChangesAsync();

        // Assert
        var saved = await _fixture.Context.Scans.FindAsync(scan.ScanId);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.ScanId, Is.EqualTo(scan.ScanId));
    }

    [Test]
    public async Task UpdateAsync_UpdatesExistingScan()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        // Act
        scan.Status = "Complete";
        scan.CompletedAt = DateTime.UtcNow;
        await _repository!.UpdateAsync(scan);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _fixture.Context.Scans.FindAsync(scan.ScanId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Status, Is.EqualTo("Complete"));
        Assert.That(updated.CompletedAt, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_RemovesScan()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository!.DeleteAsync(scan);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _fixture.Context.Scans.FindAsync(scan.ScanId);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllScans()
    {
        // Arrange
        var scans = new[]
        {
            TestDataGenerator.CreateTestScan(),
            TestDataGenerator.CreateTestScan(),
            TestDataGenerator.CreateTestScan()
        };
        _fixture!.Context.Scans.AddRange(scans);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var allScans = await _repository!.GetAllAsync();

        // Assert
        Assert.That(allScans.Count, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void GetByProviderAsync_WithNullProvider_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository!.GetByProviderAsync(null!));
    }

    [Test]
    public void AddAsync_WithNullScan_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _repository!.AddAsync(null!));
    }
}
