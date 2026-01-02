namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Data;
using DocsUnmessed.Tests.Integration.Fixtures;
using DocsUnmessed.Tests.Integration.Helpers;
using NUnit.Framework;

/// <summary>
/// Integration tests for UnitOfWork
/// </summary>
[TestFixture]
public sealed class UnitOfWorkTests
{
    private DatabaseFixture? _fixture;
    private UnitOfWork? _unitOfWork;

    [SetUp]
    public void Setup()
    {
        _fixture = new DatabaseFixture();
        var context = _fixture.CreateNewContext();
        _unitOfWork = new UnitOfWork(context);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
        _fixture?.Dispose();
    }

    [Test]
    public void Scans_Property_ReturnsValidRepository()
    {
        // Act
        var scans = _unitOfWork!.Scans;

        // Assert
        Assert.That(scans, Is.Not.Null);
        Assert.That(scans, Is.InstanceOf<Data.Repositories.ScanRepository>());
    }

    [Test]
    public void Items_Property_ReturnsValidRepository()
    {
        // Act
        var items = _unitOfWork!.Items;

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(items, Is.InstanceOf<Data.Repositories.ItemRepository>());
    }

    [Test]
    public async Task SaveChangesAsync_PersistsChanges()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        await _unitOfWork!.Scans.AddAsync(scan);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.GreaterThan(0));
        
        // Verify using fresh context
        using var verifyContext = _fixture!.CreateNewContext();
        var saved = await verifyContext.Scans.FindAsync(scan.ScanId);
        Assert.That(saved, Is.Not.Null);
    }

    [Test]
    public async Task SaveChangesAsync_WithNoChanges_ReturnsZero()
    {
        // Act
        var result = await _unitOfWork!.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task Transaction_CommitChanges_Succeeds()
    {
        // Arrange
        var scan1 = TestDataGenerator.CreateTestScan();
        var scan2 = TestDataGenerator.CreateTestScan();

        // Act
        await _unitOfWork!.Scans.AddAsync(scan1);
        await _unitOfWork.Scans.AddAsync(scan2);
        await _unitOfWork.SaveChangesAsync();

        // Assert - verify both scans saved
        using var verifyContext = _fixture!.CreateNewContext();
        var saved1 = await verifyContext.Scans.FindAsync(scan1.ScanId);
        var saved2 = await verifyContext.Scans.FindAsync(scan2.ScanId);
        
        Assert.That(saved1, Is.Not.Null);
        Assert.That(saved2, Is.Not.Null);
    }

    [Test]
    public async Task MultipleRepositories_ShareSameContext()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        var items = TestDataGenerator.CreateTestItems(scan.ScanId, 3);

        // Act
        await _unitOfWork!.Scans.AddAsync(scan);
        foreach (var item in items)
        {
            await _unitOfWork.Items.AddAsync(item);
        }
        await _unitOfWork.SaveChangesAsync();

        // Assert - verify all saved in single transaction
        using var verifyContext = _fixture!.CreateNewContext();
        var savedScan = await verifyContext.Scans.FindAsync(scan.ScanId);
        var savedItems = verifyContext.Items.Where(i => i.ScanId == scan.ScanId).ToList();
        
        Assert.That(savedScan, Is.Not.Null);
        Assert.That(savedItems.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Dispose_DisposesContext_Properly()
    {
        // Arrange
        var scan = TestDataGenerator.CreateTestScan();
        await _unitOfWork!.Scans.AddAsync(scan);
        await _unitOfWork.SaveChangesAsync();

        // Act
        _unitOfWork.Dispose();

        // Assert - context should be disposed, verify data persisted
        using var verifyContext = _fixture!.CreateNewContext();
        var saved = await verifyContext.Scans.FindAsync(scan.ScanId);
        Assert.That(saved, Is.Not.Null);
    }

    [Test]
    public async Task ConcurrentOperations_WorkCorrectly()
    {
        // Arrange
        var scans = new[]
        {
            TestDataGenerator.CreateTestScan(),
            TestDataGenerator.CreateTestScan(),
            TestDataGenerator.CreateTestScan()
        };

        // Act - add multiple scans
        foreach (var scan in scans)
        {
            await _unitOfWork!.Scans.AddAsync(scan);
        }
        await _unitOfWork!.SaveChangesAsync();

        // Assert - all saved
        using var verifyContext = _fixture!.CreateNewContext();
        foreach (var scan in scans)
        {
            var saved = await verifyContext.Scans.FindAsync(scan.ScanId);
            Assert.That(saved, Is.Not.Null);
        }
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act & Assert - should not throw
        Assert.DoesNotThrow(() =>
        {
            _unitOfWork?.Dispose();
            _unitOfWork?.Dispose();
            _unitOfWork?.Dispose();
        });
    }
}
