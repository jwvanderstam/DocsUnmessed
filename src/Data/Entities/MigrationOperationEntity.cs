namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a migration operation
/// </summary>
[Table("MigrationOperations")]
public sealed class MigrationOperationEntity
{
    [Key]
    [MaxLength(36)]
    public required string OperationId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string PlanId { get; set; }

    [MaxLength(36)]
    public string? ItemId { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Type { get; set; }  // Copy, Move, Delete, Rename

    [Required]
    [MaxLength(2000)]
    public required string SourcePath { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string TargetPath { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";  // Pending, Running, Complete, Failed, Skipped

    public int Priority { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(64)]
    public string? VerificationHash { get; set; }

    public string? Metadata { get; set; }  // JSON

    // Navigation properties
    [ForeignKey(nameof(PlanId))]
    public MigrationPlanEntity? Plan { get; set; }

    [ForeignKey(nameof(ItemId))]
    public ItemEntity? Item { get; set; }
}
