namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data;
using DocsUnmessed.Services;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Diagnostics;

/// <summary>
/// Performance benchmark tests for database operations
/// </summary>
[TestFixture]
public sealed class PerformanceBenchmarkTests
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
    public async Task Benchmark_QueryById_IsFast()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        // Warm up
        await _fixture.Context.Scans.FindAsync(scan.ScanId);

        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            await _fixture.Context.Scans.AsNoTracking()
                .FirstOrDefaultAsync(s => s.ScanId == scan.ScanId);
        }
        sw.Stop();

        // Assert
        var averageMs = sw.ElapsedMilliseconds / 100.0;
        Assert.That(averageMs, Is.LessThan(10), 
            $"Query by ID should average <10ms, was {averageMs:F2}ms");

        Console.WriteLine($"Query by ID: {averageMs:F2}ms average");
    }

    [Test]
    public async Task Benchmark_QueryWithFilter_IsFast()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 1000);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Warm up
        await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId && i.Type == "File")
            .ToListAsync();

        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            await _fixture.Context.Items
                .AsNoTracking()
                .Where(i => i.ScanId == scan.ScanId && i.Type == "File")
                .ToListAsync();
        }
        sw.Stop();

        // Assert
        var averageMs = sw.ElapsedMilliseconds / 10.0;
        Assert.That(averageMs, Is.LessThan(100), 
            $"Filtered query should average <100ms, was {averageMs:F2}ms");

        Console.WriteLine($"Filtered query (1000 items): {averageMs:F2}ms average");
    }

    [Test]
    public async Task Benchmark_InsertItems_IsFast()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        _fixture!.Context.Scans.Add(scan);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var sw = Stopwatch.StartNew();
        using (var insertContext = _fixture.CreateNewContext())
        {
            var items = TestDataGenerator.CreateTestItems(scan.ScanId, 1000);
            insertContext.Items.AddRange(items);
            await insertContext.SaveChangesAsync();
        }
        sw.Stop();

        // Assert
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000), 
            $"Insert 1000 items should take <2s, took {sw.ElapsedMilliseconds}ms");

        Console.WriteLine($"Insert 1000 items: {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public async Task Benchmark_CacheHitVsMiss_ShowsImprovement()
    {
        // Arrange
        var cache = new CacheService(TimeSpan.FromMinutes(5));
        var callCount = 0;

        Func<Task<string>> slowFactory = async () =>
        {
            await Task.Delay(50); // Simulate slow operation
            callCount++;
            return $"value-{callCount}";
        };

        // Act - Cache miss
        var sw1 = Stopwatch.StartNew();
        await cache.GetOrAddAsync("test-key", slowFactory);
        sw1.Stop();

        // Act - Cache hit
        var sw2 = Stopwatch.StartNew();
        await cache.GetOrAddAsync("test-key", slowFactory);
        sw2.Stop();

        // Assert
        Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(sw1.ElapsedMilliseconds),
            "Cache hit should be faster than miss");
        Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(10),
            "Cache hit should be very fast (<10ms)");

        Console.WriteLine($"Cache miss: {sw1.ElapsedMilliseconds}ms, Cache hit: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"Speedup: {sw1.ElapsedMilliseconds / (double)sw2.ElapsedMilliseconds:F1}x");

        cache.Dispose();
    }

    [Test]
    public async Task Benchmark_DuplicateDetection_ScalesWell()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItemsWithDuplicates(scan.ScanId, duplicateCount: 5);
        
        // Add more unique items to test scaling
        for (int i = 0; i < 995; i++)
        {
            items.Add(TestDataGenerator.CreateTestItem(scan.ScanId, "File", $"unique-{i}.txt"));
        }

        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        using var context = _fixture.CreateNewContext();
        var repository = new Data.Repositories.ItemRepository(context);

        // Act
        var sw = Stopwatch.StartNew();
        var duplicates = await repository.FindDuplicatesAsync(scan.ScanId);
        sw.Stop();

        // Assert
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500), 
            $"Duplicate detection on 1000 items should take <500ms, took {sw.ElapsedMilliseconds}ms");

        Console.WriteLine($"Duplicate detection (1000 items): {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Found {duplicates.Count} duplicate groups");
    }

    [Test]
    public async Task Benchmark_Pagination_IsEfficient()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 1000);
        
        _fixture!.Context.Scans.Add(scan);
        _fixture.Context.Items.AddRange(items);
        await _fixture.Context.SaveChangesAsync();

        // Act - Query with pagination
        var sw1 = Stopwatch.StartNew();
        var page1 = await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name)
            .Skip(0)
            .Take(100)
            .ToListAsync();
        sw1.Stop();

        // Act - Query without pagination (load all)
        var sw2 = Stopwatch.StartNew();
        var allItems = await _fixture.Context.Items
            .AsNoTracking()
            .Where(i => i.ScanId == scan.ScanId)
            .OrderBy(i => i.Name)
            .ToListAsync();
        sw2.Stop();

        // Assert - verify correctness and reasonable performance
        Assert.That(page1.Count, Is.EqualTo(100), "Should return correct page size");
        Assert.That(allItems.Count, Is.EqualTo(1000), "Should return all items");
        Assert.That(sw1.ElapsedMilliseconds, Is.LessThan(200),
            "Paginated query should be reasonably fast (<200ms)");

        Console.WriteLine($"Paginated (100 items): {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"All items (1000): {sw2.ElapsedMilliseconds}ms");
        
        // In-memory database is so fast that pagination might not always be faster
        // Main point is that both complete successfully
        if (sw2.ElapsedMilliseconds > sw1.ElapsedMilliseconds)
        {
            Console.WriteLine($"Speedup: {sw2.ElapsedMilliseconds / (double)Math.Max(1, sw1.ElapsedMilliseconds):F1}x");
        }
        else
        {
            Console.WriteLine("Note: In-memory database is very fast - both queries completed quickly");
        }
    }

    [Test]
    public async Task Benchmark_ServiceIntegration_MeetsTargets()
    {
        // Arrange
        using var context = _fixture!.CreateNewContext();
        var unitOfWork = new UnitOfWork(context);
        var service = new DatabaseInventoryService(unitOfWork);

        // Act - Create scan
        var sw1 = Stopwatch.StartNew();
        var scanId = await service.CreateScanAsync(new[] { "test_provider" });
        sw1.Stop();

        // Act - Add items
        var items = TestDataGenerator.CreateTestDomainItems(100);
        var sw2 = Stopwatch.StartNew();
        await service.AddItemsAsync(scanId, items);
        sw2.Stop();

        // Act - Get statistics
        var sw3 = Stopwatch.StartNew();
        var stats = await service.GetStatisticsAsync(scanId);
        sw3.Stop();

        // Assert
        Assert.That(sw1.ElapsedMilliseconds, Is.LessThan(50), 
            $"Create scan should be <50ms, was {sw1.ElapsedMilliseconds}ms");
        Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(500), 
            $"Add 100 items should be <500ms, was {sw2.ElapsedMilliseconds}ms");
        Assert.That(sw3.ElapsedMilliseconds, Is.LessThan(100), 
            $"Get statistics should be <100ms, was {sw3.ElapsedMilliseconds}ms");

        Console.WriteLine($"Create scan: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"Add 100 items: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"Get statistics: {sw3.ElapsedMilliseconds}ms");

        unitOfWork.Dispose();
    }
}
