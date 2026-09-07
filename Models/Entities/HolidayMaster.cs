using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("HolidayMaster")]
public class HolidayMaster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("HolidayDate", TypeName = "datetime")]
    public DateTime HolidayDate { get; set; }

    [Required]
    [Column("Holiday_Desc", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string Holiday_Desc { get; set; } = string.Empty;

    [Required]
    [Column("Ispaid", TypeName = "bit")]
    public bool Ispaid { get; set; }
}
