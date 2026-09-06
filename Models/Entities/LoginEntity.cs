using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("Login")]
public class LoginEntity
{
    [Key]
    [Column("Uid", TypeName = "nvarchar(5)")]
    [MaxLength(5)]
    public string Uid { get; set; } = string.Empty;

    [Required]
    [Column("Username", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("Password", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string Password { get; set; } = string.Empty;

    [Column("Role", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? Role { get; set; }

    [Column("Mod_dt", TypeName = "datetime")]
    public DateTime? ModDt { get; set; }

    [Column("Ent_Log", TypeName = "datetime")]
    public DateTime? EntLog { get; set; }

    [Column("IsLoginUser", TypeName = "char(1)")]
    [MaxLength(1)]
    public string? IsLoginUser { get; set; }

    [Column("device_token", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? DeviceToken { get; set; }

    [Column("EmployeeName", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? EmployeeName { get; set; }

    [Column("ContractorID")]
    public int? ContractorId { get; set; }

    [Column("sid", TypeName = "numeric(18,0)")]
    public decimal? Sid { get; set; }

    [Column("Email", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Email { get; set; }

    [Column("Comp_Logo", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? CompLogo { get; set; }
}
