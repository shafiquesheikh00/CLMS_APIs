using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("OtherMaster")]
public class OtherMaster
{
    [Key]
    [Column("MasterID")]
    public int MasterId { get; set; }

    [Column("MasterName", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? MasterName { get; set; }

    [Column("Description", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? Description { get; set; }

    [Column("Status")]
    public bool? Status { get; set; }

    [Column("MasterTypeID")]
    public int? MasterTypeId { get; set; }

    [Column("MasterType", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? MasterType { get; set; }
}
