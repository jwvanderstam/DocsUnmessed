namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a migration plan
/// </summary>
[Table("MigrationPlans")]
public sealed class MigrationPlanEntity
{
    [Key]
    [MaxLength(36)]
    public required string PlanId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string ScanId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";  // Draft, Ready, Running, Paused, Complete, Failed

    public int TotalOperations { get; set; }

    public int CompletedOperations { get; set; }

    public int FailedOperations { get; set; }

    public int TotalFiles { get; set; }

    public long TotalSize { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Configuration { get; set; }  // JSON

    public string? Metrics { get; set; }  // JSON

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ScanId))]
    public ScanEntity? Scan { get; set; }

    public ICollection<MigrationOperationEntity> Operations { get; set; } = new List<MigrationOperationEntity>();
}
