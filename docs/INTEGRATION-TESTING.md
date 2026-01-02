# Integration Testing Guide

## Overview

This document provides integration testing scenarios for DocsUnmessed, covering end-to-end workflows, database operations, and performance verification.

---

## Test Scenarios

### Scenario 1: First-Time Scan

**Purpose**: Test complete workflow from scan creation to completion

**Steps**:
1. Start application (database should initialize automatically)
2. Run assess command with local file system
3. Verify scan is created in database
4. Verify items are stored
5. Verify statistics are calculated
6. Verify scan completion

**Expected Results**:
- Database file created: `docsunmessed.db`
- Scan record with status "Complete"
- All items stored with correct metadata
- Statistics match actual file counts
- No errors or exceptions

**Command**:
```bash
dotnet run -- assess --providers fs_local --root "C:\TestFiles" --out scan1.json
```

**Verification**:
```bash
# Check database file exists
Test-Path docsunmessed.db

# Verify scan in JSON output
Get-Content scan1.json | ConvertFrom-Json | Select ScanId, @{N='Files';E={$_.Statistics.TotalFiles}}
```

---

### Scenario 2: Multiple Scans (Data Persistence)

**Purpose**: Verify data persists between application runs

**Steps**:
1. Run first scan
2. Exit application
3. Restart application
4. Run second scan
5. Query both scans from database

**Expected Results**:
- First scan data still present after restart
- Second scan added to same database
- Both scans queryable
- No data loss

**Commands**:
```bash
# First scan
dotnet run -- assess --providers fs_local --root "C:\Folder1" --out scan1.json

# Exit and restart

# Second scan
dotnet run -- assess --providers fs_local --root "C:\Folder2" --out scan2.json
```

---

### Scenario 3: Large Dataset (Performance)

**Purpose**: Test with 10,000+ files

**Steps**:
1. Prepare folder with 10,000+ files
2. Run scan with performance monitoring
3. Measure query performance
4. Verify memory usage

**Expected Results**:
- Scan completes successfully
- Query times <100ms
- Memory usage <50MB
- No performance degradation

**Performance Targets**:
- Scan 10,000 files: <2 minutes
- Get scan result: <50ms
- Find duplicates: <100ms
- Get statistics: <50ms

---

### Scenario 4: Caching Behavior

**Purpose**: Verify caching works correctly

**Steps**:
1. Get scan result (first call - cold cache)
2. Get scan result again (should be cached)
3. Add items to scan
4. Get scan result (cache should be invalidated)
5. Get scan result again (should be cached again)

**Expected Results**:
- First call: 30-50ms (database hit)
- Cached call: 1-2ms (cache hit)
- After invalidation: 30-50ms (database hit)
- Re-cached: 1-2ms (cache hit)

**Code**:
```csharp
var monitor = new PerformanceMonitor();
var cache = new CacheService();
var baseService = new DatabaseInventoryService(unitOfWork);
var cachedService = new CachedInventoryService(baseService, cache);

// First call
var scan1 = await monitor.MeasureAsync("GetScan-Cold", 
    () => cachedService.GetScanResultAsync(scanId));

// Cached call
var scan2 = await monitor.MeasureAsync("GetScan-Cached", 
    () => cachedService.GetScanResultAsync(scanId));

// Should be much faster
var summary = monitor.GetSummary();
```

---

### Scenario 5: Duplicate Detection

**Purpose**: Test duplicate file detection

**Steps**:
1. Create test files with known duplicates
2. Run scan
3. Find duplicates
4. Verify results

**Expected Results**:
- All duplicates identified by hash
- Groups formed correctly
- File paths preserved
- No false positives

**Test Data**:
```
TestFiles/
??? file1.txt (hash: abc123)
??? copy1.txt (hash: abc123)  # duplicate of file1
??? copy2.txt (hash: abc123)  # duplicate of file1
??? unique.txt (hash: def456) # unique
```

**Expected**: 1 duplicate group with 3 files (hash: abc123)

---

### Scenario 6: Pagination

**Purpose**: Test query pagination

**Steps**:
1. Scan folder with 1,000+ items
2. Query first page (100 items)
3. Query second page
4. Query last page
5. Verify total counts

**Expected Results**:
- Pages return correct number of items
- Total count accurate
- No duplicates across pages
- HasNextPage/HasPreviousPage correct

**Code**:
```csharp
// Get first page
var page1 = await items
    .AsNoTracking()
    .Where(i => i.ScanId == scanId)
    .ToPagedResultAsync(page: 1, pageSize: 100);

Assert.Equal(100, page1.Items.Count);
Assert.True(page1.HasNextPage);
Assert.False(page1.HasPreviousPage);
```

---

### Scenario 7: Batch Processing

**Purpose**: Test batch processing of large datasets

**Steps**:
1. Query 10,000 items
2. Process in batches of 1,000
3. Track progress
4. Verify all processed

**Expected Results**:
- All items processed
- Memory usage controlled
- Progress tracking works
- No items skipped

**Code**:
```csharp
var processed = 0;
await items
    .AsNoTracking()
    .Where(i => i.ScanId == scanId)
    .ProcessInBatchesAsync(1000, async batch => {
        processed += batch.Count();
        Console.WriteLine($"Processed {processed} items...");
    });
```

---

### Scenario 8: Database Migration

**Purpose**: Test automatic migration on startup

**Steps**:
1. Delete database file
2. Start application
3. Verify database created
4. Verify schema correct
5. Verify migrations applied

**Expected Results**:
- Database file created automatically
- All tables created
- All indexes created
- Migration history recorded

**Verification**:
```sql
-- Check tables exist
SELECT name FROM sqlite_master WHERE type='table';

-- Check indexes
SELECT name FROM sqlite_master WHERE type='index';

-- Check migration history
SELECT * FROM __EFMigrationsHistory;
```

---

### Scenario 9: Error Handling

**Purpose**: Test error scenarios

**Test Cases**:
1. Invalid scan ID
2. Null parameters
3. Database connection failure
4. Invalid file paths
5. Permission errors

**Expected Results**:
- Clear error messages
- No data corruption
- Proper exception handling
- Graceful degradation

---

### Scenario 10: Concurrent Operations

**Purpose**: Test thread safety

**Steps**:
1. Start multiple scan operations
2. Query while scanning
3. Update caches concurrently
4. Verify data consistency

**Expected Results**:
- No race conditions
- Data remains consistent
- No deadlocks
- Proper isolation

---

## Performance Benchmarks

### Query Performance Test

```csharp
var monitor = new PerformanceMonitor();
var scanId = "test-scan-id";

// Warm up
await repository.GetByIdAsync(scanId);

// Run benchmark
for (int i = 0; i < 100; i++)
{
    await monitor.MeasureAsync("GetById", 
        () => repository.GetByIdAsync(scanId));
}

var summary = monitor.GetSummary();
Console.WriteLine($"Average: {summary.AverageDuration.TotalMilliseconds}ms");
Assert.True(summary.AverageDuration < TimeSpan.FromMilliseconds(10));
```

### Cache Performance Test

```csharp
var cache = new CacheService();
var stopwatch = Stopwatch.StartNew();

// Test cache hit rate
for (int i = 0; i < 1000; i++)
{
    var result = await cache.GetOrAddAsync(
        $"key-{i % 100}", // 10x reuse
        () => Task.FromResult($"value-{i}")
    );
}

stopwatch.Stop();
var stats = cache.GetStatistics();
var hitRate = (stats.ActiveEntries / 100.0) * 100;
Console.WriteLine($"Cache hit rate: {hitRate:F1}%");
Assert.True(hitRate > 90);
```

---

## Manual Testing Checklist

### Basic Operations
- [ ] Create scan
- [ ] Add items
- [ ] Get scan result
- [ ] Find duplicates
- [ ] Get statistics
- [ ] Complete scan

### Performance
- [ ] Query times <100ms
- [ ] Cache hit rate >90%
- [ ] Memory usage acceptable
- [ ] No performance degradation

### Data Integrity
- [ ] Data persists between runs
- [ ] Correct statistics
- [ ] Accurate duplicate detection
- [ ] Proper timestamp handling

### Error Handling
- [ ] Invalid input handled
- [ ] Clear error messages
- [ ] No data corruption
- [ ] Graceful failures

### Configuration
- [ ] Connection string works
- [ ] Environment-specific configs
- [ ] Database initialization
- [ ] Migration application

---

## Automated Test Template

```csharp
public class IntegrationTests
{
    private DocsUnmessedDbContext _context;
    private IUnitOfWork _unitOfWork;
    private IInventoryService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DocsUnmessedDbContext>()
            .UseSqlite("Data Source=test.db")
            .Options;

        _context = new DocsUnmessedDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _service = new DatabaseInventoryService(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateScan_StoresInDatabase()
    {
        // Arrange
        var providers = new[] { "fs_local" };

        // Act
        var scanId = await _service.CreateScanAsync(providers);

        // Assert
        var scan = await _unitOfWork.Scans.GetByIdAsync(scanId);
        Assert.NotNull(scan);
        Assert.Equal("Running", scan.Status);
    }

    [Test]
    public async Task AddItems_UpdatesStatistics()
    {
        // Arrange
        var scanId = await _service.CreateScanAsync(new[] { "fs_local" });
        var items = CreateTestItems(100);

        // Act
        await _service.AddItemsAsync(scanId, items);

        // Assert
        var stats = await _service.GetStatisticsAsync(scanId);
        Assert.Equal(100, stats.TotalFiles + stats.TotalFolders);
    }

    private List<Item> CreateTestItems(int count)
    {
        // Create test items
    }
}
```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Run Integration Tests
      run: |
        dotnet test --filter Category=Integration
    
    - name: Performance Tests
      run: |
        dotnet test --filter Category=Performance
```

---

## Troubleshooting

### Common Issues

**Issue**: Database file locked
**Solution**: Ensure all connections disposed, check for background processes

**Issue**: Slow queries
**Solution**: Verify indexes, check AsNoTracking usage, enable query logging

**Issue**: Cache not working
**Solution**: Check expiration times, verify decorator setup, review invalidation logic

**Issue**: Migration errors
**Solution**: Delete database, restart application, check migration files

---

*Last Updated: January 3, 2025*  
*Version: 1.0*
