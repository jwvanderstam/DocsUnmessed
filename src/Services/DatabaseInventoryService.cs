namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Data;
using DocsUnmessed.Data.Entities;
using DocsUnmessed.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

/// <summary>
/// Database-backed inventory service using SQLite persistence
/// </summary>
public sealed class DatabaseInventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the DatabaseInventoryService class
    /// </summary>
    /// <param name="unitOfWork">Unit of work for database operations</param>
    public DatabaseInventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc/>
    public async Task<string> CreateScanAsync(string[] providers, CancellationToken cancellationToken = default)
    {
        var scanId = Guid.NewGuid().ToString("N")[..12];
        
        var scanEntity = new ScanEntity
        {
            ScanId = scanId,
            ProviderId = string.Join(",", providers),
            RootPath = "Multiple",
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Configuration = JsonSerializer.Serialize(new { Providers = providers })
        };

        await _unitOfWork.Scans.AddAsync(scanEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return scanId;
    }

    /// <inheritdoc/>
    public async Task AddItemsAsync(string scanId, IEnumerable<Item> items, CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetByIdAsync(scanId, cancellationToken);
        if (scan == null)
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var itemEntities = items.Select(item => MapToItemEntity(scanId, item)).ToList();
        
        await _unitOfWork.Items.AddRangeAsync(itemEntities, cancellationToken);
        
        // Update scan statistics
        UpdateScanStatistics(scan, items);
        await _unitOfWork.Scans.UpdateAsync(scan);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ScanResult> GetScanResultAsync(string scanId, CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetWithItemsAsync(scanId, cancellationToken);
        if (scan == null)
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var items = await _unitOfWork.Items.GetByScanAsync(scanId, cancellationToken);

        return MapToScanResult(scan, items);
    }

    /// <inheritdoc/>
    public async Task<List<DuplicateSet>> FindDuplicatesAsync(string scanId, CancellationToken cancellationToken = default)
    {
        var duplicates = await _unitOfWork.Items.FindDuplicatesAsync(scanId, cancellationToken);

        return duplicates.Select(kvp => new DuplicateSet
        {
            Hash = kvp.Key,
            Size = kvp.Value.First().SizeBytes,
            Items = kvp.Value.Select(MapToDomainItem).ToList(),
            Type = DuplicateType.Exact
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ValidationIssue>> ValidateAsync(string scanId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Items.GetByScanAsync(scanId, cancellationToken);
        
        var issues = new List<ValidationIssue>();

        foreach (var item in items.Where(i => i.Type == "File"))
        {
            // Parse issues from metadata if stored as JSON
            if (!string.IsNullOrEmpty(item.Metadata))
            {
                try
                {
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(item.Metadata);
                    if (metadata != null && metadata.ContainsKey("Issues"))
                    {
                        var itemIssues = JsonSerializer.Deserialize<List<string>>(metadata["Issues"].ToString() ?? "[]");
                        if (itemIssues != null)
                        {
                            foreach (var issue in itemIssues)
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
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }
        }

        return issues;
    }

    /// <inheritdoc/>
    public async Task<ScanStatistics> GetStatisticsAsync(string scanId, CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetByIdAsync(scanId, cancellationToken);
        if (scan == null)
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        var totalSize = await _unitOfWork.Items.GetTotalSizeAsync(scanId, cancellationToken);

        return new ScanStatistics
        {
            TotalFiles = scan.TotalFiles,
            TotalFolders = scan.TotalFolders,
            TotalSize = totalSize,
            MaxDepth = 0, // Would need to calculate from items
            FilesWithIssues = 0, // Would need to count from metadata
            SizeByProvider = new Dictionary<string, long>(),
            CountByProvider = new Dictionary<string, long>()
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Item>> QueryItemsAsync(
        string scanId, 
        Func<Item, bool> predicate, 
        CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Items.GetByScanAsync(scanId, cancellationToken);
        var domainItems = items.Select(MapToDomainItem);
        
        return domainItems.Where(predicate);
    }

    /// <inheritdoc/>
    public async Task CompleteScanAsync(string scanId, CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetByIdAsync(scanId, cancellationToken);
        if (scan == null)
        {
            throw new InvalidOperationException($"Scan {scanId} not found");
        }

        scan.Status = "Complete";
        scan.CompletedAt = DateTime.UtcNow;

        await _unitOfWork.Scans.UpdateAsync(scan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Private mapping methods

    private static ItemEntity MapToItemEntity(string scanId, Item item)
    {
        var metadata = new Dictionary<string, object>
        {
            ["Issues"] = item.Issues,
            ["IsShared"] = item.IsShared,
            ["ExtendedProperties"] = item.ExtendedProperties
        };

        return new ItemEntity
        {
            ItemId = Guid.NewGuid().ToString("N"),
            ScanId = scanId,
            Type = item.Type.ToString(),
            Name = item.Name,
            Path = item.Path,
            Extension = GetExtension(item.Name),
            SizeBytes = item.Size,
            Hash = item.Hash,
            CreatedUtc = item.CreatedUtc,
            ModifiedUtc = item.ModifiedUtc,
            ParentPath = GetParentPath(item.Path),
            Depth = item.Depth,
            Metadata = JsonSerializer.Serialize(metadata)
        };
    }

    private static Item MapToDomainItem(ItemEntity entity)
    {
        var metadata = new Dictionary<string, string>();
        var issues = new List<string>();
        var extendedProps = new Dictionary<string, string>();
        bool isShared = false;

        if (!string.IsNullOrEmpty(entity.Metadata))
        {
            try
            {
                var meta = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata);
                if (meta != null)
                {
                    if (meta.ContainsKey("Issues"))
                    {
                        issues = JsonSerializer.Deserialize<List<string>>(meta["Issues"].ToString() ?? "[]") ?? new List<string>();
                    }
                    if (meta.ContainsKey("IsShared"))
                    {
                        bool.TryParse(meta["IsShared"].ToString(), out isShared);
                    }
                    if (meta.ContainsKey("ExtendedProperties"))
                    {
                        extendedProps = JsonSerializer.Deserialize<Dictionary<string, string>>(
                            meta["ExtendedProperties"].ToString() ?? "{}") ?? new Dictionary<string, string>();
                    }
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return new Item
        {
            Path = entity.Path,
            Name = entity.Name,
            Provider = "fs_local", // Would need to be stored or derived
            Size = entity.SizeBytes,
            MimeType = GetMimeType(entity.Extension),
            CreatedUtc = entity.CreatedUtc ?? DateTime.UtcNow,
            ModifiedUtc = entity.ModifiedUtc ?? DateTime.UtcNow,
            Hash = entity.Hash,
            Type = Enum.Parse<ItemType>(entity.Type),
            ExtendedProperties = extendedProps,
            Issues = issues,
            IsShared = isShared,
            Depth = entity.Depth
        };
    }

    private static ScanResult MapToScanResult(ScanEntity scanEntity, IReadOnlyList<ItemEntity> itemEntities)
    {
        var providers = Array.Empty<string>();
        
        if (!string.IsNullOrEmpty(scanEntity.Configuration))
        {
            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(scanEntity.Configuration);
                if (config != null && config.ContainsKey("Providers"))
                {
                    providers = JsonSerializer.Deserialize<string[]>(config["Providers"].ToString() ?? "[]") ?? Array.Empty<string>();
                }
            }
            catch
            {
                providers = scanEntity.ProviderId.Split(',');
            }
        }

        return new ScanResult
        {
            ScanId = scanEntity.ScanId,
            StartedUtc = scanEntity.StartedAt,
            CompletedUtc = scanEntity.CompletedAt,
            Providers = providers,
            Items = itemEntities.Select(MapToDomainItem).ToList(),
            Statistics = new ScanStatistics
            {
                TotalFiles = scanEntity.TotalFiles,
                TotalFolders = scanEntity.TotalFolders,
                TotalSize = scanEntity.TotalSize
            }
        };
    }

    private static void UpdateScanStatistics(ScanEntity scan, IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            if (item.Type == ItemType.File)
            {
                scan.TotalFiles++;
                scan.TotalSize += item.Size;
            }
            else if (item.Type == ItemType.Folder)
            {
                scan.TotalFolders++;
            }

            scan.TotalItems++;
        }
    }

    private static string? GetExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return string.IsNullOrEmpty(ext) ? null : ext.TrimStart('.');
    }

    private static string? GetParentPath(string path)
    {
        try
        {
            return Path.GetDirectoryName(path);
        }
        catch
        {
            return null;
        }
    }

    private static string GetMimeType(string? extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            "txt" => "text/plain",
            "pdf" => "application/pdf",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
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
