namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using System.Collections.Concurrent;

/// <summary>
/// In-memory inventory service (production version would use SQLite)
/// </summary>
public sealed class InMemoryInventoryService : IInventoryService
{
    private readonly ConcurrentDictionary<string, ScanResult> _scans = new();

    public Task<string> CreateScanAsync(string[] providers, CancellationToken cancellationToken = default)
    {
        var scanId = Guid.NewGuid().ToString("N")[..12];
        var scan = new ScanResult
        {
            ScanId = scanId,
            StartedUtc = DateTime.UtcNow,
            Providers = providers
        };

        _scans.TryAdd(scanId, scan);
        return Task.FromResult(scanId);
    }

    public Task AddItemsAsync(string scanId, IEnumerable<Item> items, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        foreach (var item in items)
        {
            scan.Items.Add(item);
            
            // Update statistics
            if (item.Type == ItemType.File)
            {
                scan.Statistics.TotalFiles++;
                scan.Statistics.TotalSize += item.Size;
            }
            else if (item.Type == ItemType.Folder)
            {
                scan.Statistics.TotalFolders++;
            }

            if (item.Depth > scan.Statistics.MaxDepth)
            {
                scan.Statistics.MaxDepth = item.Depth;
            }

            if (item.Issues.Count > 0)
            {
                scan.Statistics.FilesWithIssues++;
            }

            // Update provider statistics
            if (!scan.Statistics.CountByProvider.ContainsKey(item.Provider))
            {
                scan.Statistics.CountByProvider[item.Provider] = 0;
                scan.Statistics.SizeByProvider[item.Provider] = 0;
            }

            scan.Statistics.CountByProvider[item.Provider]++;
            scan.Statistics.SizeByProvider[item.Provider] += item.Size;
        }

        return Task.CompletedTask;
    }

    public Task<ScanResult> GetScanResultAsync(string scanId, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        return Task.FromResult(scan);
    }

    public Task<List<DuplicateSet>> FindDuplicatesAsync(string scanId, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var duplicates = scan.Items
            .Where(i => i.Type == ItemType.File && !string.IsNullOrEmpty(i.Hash))
            .GroupBy(i => i.Hash!)
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateSet
            {
                Hash = g.Key,
                Size = g.First().Size,
                Items = g.ToList(),
                Type = DuplicateType.Exact
            })
            .ToList();

        scan.Duplicates.Clear();
        scan.Duplicates.AddRange(duplicates);

        return Task.FromResult(duplicates);
    }

    public Task<List<ValidationIssue>> ValidateAsync(string scanId, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var issues = new List<ValidationIssue>();

        foreach (var item in scan.Items.Where(i => i.Type == ItemType.File))
        {
            foreach (var issue in item.Issues)
            {
                issues.Add(new ValidationIssue
                {
                    ItemPath = item.Path,
                    Type = ParseIssueType(issue),
                    Description = issue,
                    Severity = "Warning"
                });
            }
        }

        scan.Issues.Clear();
        scan.Issues.AddRange(issues);

        return Task.FromResult(issues);
    }

    public Task<ScanStatistics> GetStatisticsAsync(string scanId, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        return Task.FromResult(scan.Statistics);
    }

    public Task<IEnumerable<Item>> QueryItemsAsync(string scanId, Func<Item, bool> predicate, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var results = scan.Items.Where(predicate);
        return Task.FromResult(results);
    }

    public Task CompleteScanAsync(string scanId, CancellationToken cancellationToken = default)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        scan.CompletedUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private static IssueType ParseIssueType(string issue)
    {
        return issue switch
        {
            "NonCompliantName" => IssueType.NonCompliantName,
            "TooDeep" => IssueType.TooDeep,
            "OverDense" => IssueType.OverDense,
            "OutsidePrimaryCloud" => IssueType.OutsidePrimaryCloud,
            "InvalidCharacters" => IssueType.InvalidCharacters,
            "PathTooLong" => IssueType.PathTooLong,
            _ => IssueType.MissingMetadata
        };
    }
}
