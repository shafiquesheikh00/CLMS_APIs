using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("EmployeeMaster")]
public class EmployeeMaster
{
    [Key]
    [Column("SID")]
    public decimal Sid { get; set; }

    [Column("LabContID")]
    public int? LabContID { get; set; }

    [Column("STATUS")]
    public int? Status { get; set; }
}
