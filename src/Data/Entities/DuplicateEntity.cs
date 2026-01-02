namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a group of duplicate files
/// </summary>
[Table("Duplicates")]
public sealed class DuplicateEntity
{
    [Key]
    [MaxLength(36)]
    public required string DuplicateGroupId { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Hash { get; set; }

    public int FileCount { get; set; }

    public long TotalSize { get; set; }

    [Required]
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<DuplicateItemEntity> DuplicateItems { get; set; } = new List<DuplicateItemEntity>();
}

/// <summary>
/// Entity linking items to duplicate groups
/// </summary>
[Table("DuplicateItems")]
public sealed class DuplicateItemEntity
{
    [Required]
    [MaxLength(36)]
    public required string DuplicateGroupId { get; set; }

    [Required]
    [MaxLength(36)]
    public required string ItemId { get; set; }

    public bool IsPrimary { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DuplicateGroupId))]
    public DuplicateEntity? DuplicateGroup { get; set; }

    [ForeignKey(nameof(ItemId))]
    public ItemEntity? Item { get; set; }
}
