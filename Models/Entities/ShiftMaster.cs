using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("ShiftMaster")]
public class ShiftMaster
{
    [Key]
    [Column("ShiftID", TypeName = "numeric(18,0)")]
    public decimal ShiftId { get; set; }

    [Required]
    [Column("ShiftName", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string ShiftName { get; set; } = string.Empty;

    [Required]
    [Column("Start_Time", TypeName = "datetime")]
    public DateTime Start_Time { get; set; }

    [Required]
    [Column("End_Time", TypeName = "datetime")]
    public DateTime End_Time { get; set; }

    [Column("Shift_Flag", TypeName = "bit")]
    public bool? Shift_Flag { get; set; }

    [Column("ShiftHours", TypeName = "decimal(18,2)")]
    public decimal? ShiftHours { get; set; }

    [Column("GressTime")]
    public double? GressTime { get; set; }
}
