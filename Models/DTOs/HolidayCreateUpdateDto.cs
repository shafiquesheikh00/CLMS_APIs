using System.ComponentModel.DataAnnotations;

namespace CLMS_APIs.Models.DTOs;

public class HolidayCreateUpdateDto
{
    [Required(ErrorMessage = "Holiday date is required.")]
    public DateTime HolidayDate { get; set; }

    [Required(ErrorMessage = "Holiday description is required.")]
    [MaxLength(50, ErrorMessage = "Holiday description cannot exceed 50 characters.")]
    public string HolidayDesc { get; set; } = string.Empty;

    public bool IsPaid { get; set; }
}
