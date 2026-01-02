namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Services.Migration;
using NUnit.Framework;

/// <summary>
/// Tests for Migration Planning
/// </summary>
[TestFixture]
public sealed class MigrationPlannerTests
{
    [Test]
    public async Task CreatePlanAsync_GeneratesOperations()
    {
        // Arrange
        var config = new MigrationPlanningConfig
        {
            TargetRootPath = "C:/Target",
            DefaultNamingTemplate = "{Year}/{Month}/{Name}.{Extension}"
        };
        var planner = new MigrationPlanner(config);
        var items = new[]
        {
            CreateItem("file1.txt"),
            CreateItem("file2.pdf")
        };

        // Act
        var plan = await planner.CreatePlanAsync("scan1", items);

        // Assert
        Assert.That(plan.Operations.Count, Is.EqualTo(2));
        Assert.That(plan.ScanId, Is.EqualTo("scan1"));
        Assert.That(plan.IsValid, Is.True);
    }

    [Test]
    public async Task CreatePlanAsync_DetectsConflicts()
    {
        // Arrange
        var config = new MigrationPlanningConfig
        {
            TargetRootPath = "C:/Target",
            DefaultNamingTemplate = "AllFiles/{Name}.{Extension}", // All to same folder
            DetectConflicts = true
        };
        var planner = new MigrationPlanner(config);
        var items = new[]
        {
            CreateItem("file.txt"),
            CreateItem("file.txt") // Duplicate name
        };

        // Act
        var plan = await planner.CreatePlanAsync("scan1", items);

        // Assert
        Assert.That(plan.ConflictCount, Is.GreaterThan(0));
        Assert.That(plan.IsValid, Is.False);
    }

    [Test]
    public async Task OptimizePlan_ResolvesConflicts()
    {
        // Arrange
        var config = new MigrationPlanningConfig
        {
            TargetRootPath = "C:/Target",
            DefaultNamingTemplate = "AllFiles/{Name}.{Extension}"
        };
        var planner = new MigrationPlanner(config);
        var items = new[]
        {
            CreateItem("file.txt"),
            CreateItem("file.txt")
        };

        var originalPlan = await planner.CreatePlanAsync("scan1", items);
        Assert.That(originalPlan.HasConflicts(), Is.True);

        // Act
        var optimizedPlan = planner.OptimizePlan(originalPlan);

        // Assert
        Assert.That(optimizedPlan.ConflictCount, Is.EqualTo(0));
        Assert.That(optimizedPlan.IsValid, Is.True);
        
        // Verify unique paths
        var paths = optimizedPlan.Operations.Select(op => op.TargetPath).Distinct().Count();
        Assert.That(paths, Is.EqualTo(optimizedPlan.Operations.Count));
    }

    [Test]
    public void CreatePlanAsync_NullScanId_ThrowsArgumentException()
    {
        // Arrange
        var config = new MigrationPlanningConfig { TargetRootPath = "C:/Target" };
        var planner = new MigrationPlanner(config);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await planner.CreatePlanAsync(null!, Array.Empty<Item>()));
    }

    [Test]
    public void CreatePlanAsync_NullItems_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MigrationPlanningConfig { TargetRootPath = "C:/Target" };
        var planner = new MigrationPlanner(config);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await planner.CreatePlanAsync("scan1", null!));
    }

    private static Item CreateItem(string name)
    {
        return new Item
        {
            Path = $"/source/{name}",
            Name = name,
            Provider = "test",
            Size = 1024,
            Type = ItemType.File,
            MimeType = "application/octet-stream",
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,
            ExtendedProperties = new Dictionary<string, string>(),
            Issues = new List<string>(),
            IsShared = false,
            Depth = 1
        };
    }
}
