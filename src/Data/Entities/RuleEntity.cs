namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing a file organization rule
/// </summary>
[Table("Rules")]
public sealed class RuleEntity
{
    [Key]
    [MaxLength(36)]
    public required string RuleId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Type { get; set; }  // RegexPath, Extension, AgeBased, Composite

    [Required]
    public int Priority { get; set; } = 100;

    public bool IsEnabled { get; set; } = true;

    [Required]
    public required string Configuration { get; set; }  // JSON

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    public int UsageCount { get; set; }

    // Navigation properties
    public ICollection<SuggestionEntity> Suggestions { get; set; } = new List<SuggestionEntity>();
}
