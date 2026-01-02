namespace DocsUnmessed.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity representing an audit log entry
/// </summary>
[Table("AuditLog")]
public sealed class AuditLogEntity
{
    [Key]
    [MaxLength(36)]
    public required string LogId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Action { get; set; }

    [Required]
    [MaxLength(50)]
    public required string EntityType { get; set; }

    [MaxLength(36)]
    public string? EntityId { get; set; }

    public string? Details { get; set; }  // JSON

    public bool Success { get; set; } = true;

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }
}
