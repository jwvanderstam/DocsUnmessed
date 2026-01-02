namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a file or folder
/// </summary>
[Table("Items")]
public sealed class ItemEntity
{
    [Key]
    [MaxLength(36)]
    public required string ItemId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string ScanId { get; set; }

    [Required]
    [MaxLength(10)]
    public required string Type { get; set; }  // File, Folder

    [Required]
    [MaxLength(500)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Path { get; set; }

    [MaxLength(50)]
    public string? Extension { get; set; }

    public long SizeBytes { get; set; }

    [MaxLength(64)]
    public string? Hash { get; set; }

    public DateTime? CreatedUtc { get; set; }

    public DateTime? ModifiedUtc { get; set; }

    public DateTime? AccessedUtc { get; set; }

    [MaxLength(2000)]
    public string? ParentPath { get; set; }

    public int Depth { get; set; }

    public bool IsHidden { get; set; }

    public bool IsSystem { get; set; }

    public string? Attributes { get; set; }  // JSON

    public string? Metadata { get; set; }  // JSON

    // Navigation properties
    [ForeignKey(nameof(ScanId))]
    public ScanEntity? Scan { get; set; }

    public ICollection<SuggestionEntity> Suggestions { get; set; } = new List<SuggestionEntity>();
    public ICollection<DuplicateItemEntity> DuplicateItems { get; set; } = new List<DuplicateItemEntity>();
}
