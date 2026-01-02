namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a file organization suggestion
/// </summary>
[Table("Suggestions")]
public sealed class SuggestionEntity
{
    [Key]
    [MaxLength(36)]
    public required string SuggestionId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string ScanId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string ItemId { get; set; }

    [MaxLength(36)]
    public string? RuleId { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string SourcePath { get; set; }

    [Required]
    [MaxLength(500)]
    public required string SourceName { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string TargetPath { get; set; }

    [Required]
    [MaxLength(500)]
    public required string TargetName { get; set; }

    [Required]
    public double Confidence { get; set; }

    public string? Reasons { get; set; }  // JSON array

    [Required]
    [MaxLength(50)]
    public required string ConflictPolicy { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";  // Pending, Accepted, Rejected, Applied

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AppliedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ScanId))]
    public ScanEntity? Scan { get; set; }

    [ForeignKey(nameof(ItemId))]
    public ItemEntity? Item { get; set; }

    [ForeignKey(nameof(RuleId))]
    public RuleEntity? Rule { get; set; }
}
