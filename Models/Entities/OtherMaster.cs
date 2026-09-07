using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("OtherMaster")]
public class OtherMaster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("MasterTypeID")]
    public int MasterTypeId { get; set; }

    [Required]
    [Column("MasterID")]
    public int MasterId { get; set; }

    [Required]
    [Column("MasterName", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string MasterName { get; set; } = string.Empty;

    [Column("Description", TypeName = "varchar(200)")]
    [MaxLength(200)]
    public string? Description { get; set; }

    [Column("Status")]
    public bool? Status { get; set; } = true;

    [Required]
    [Column("MasterType", TypeName = "nvarchar(150)")]
    [MaxLength(150)]
    public string MasterType { get; set; } = string.Empty;
}
