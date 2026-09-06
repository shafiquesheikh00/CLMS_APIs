using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RecordId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;
}
