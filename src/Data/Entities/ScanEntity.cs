namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a file system scan
/// </summary>
[Table("Scans")]
public sealed class ScanEntity
{
    [Key]
    [MaxLength(36)]
    public required string ScanId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string ProviderId { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string RootPath { get; set; }

    [Required]
    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Status { get; set; }  // Pending, Running, Complete, Failed

    public int TotalItems { get; set; }

    public long TotalSize { get; set; }

    public int TotalFiles { get; set; }

    public int TotalFolders { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    public string? Configuration { get; set; }  // JSON

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
    public ICollection<SuggestionEntity> Suggestions { get; set; } = new List<SuggestionEntity>();
    public ICollection<MigrationPlanEntity> MigrationPlans { get; set; } = new List<MigrationPlanEntity>();
}
