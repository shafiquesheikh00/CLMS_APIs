using System.ComponentModel.DataAnnotations;

namespace CLMS_APIs.Models.DTOs;

public class ShiftCreateUpdateDto
{
    [Required(ErrorMessage = "Shift name is required.")]
    [MaxLength(50, ErrorMessage = "Shift name cannot exceed 50 characters.")]
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>
    /// Start time in "HH:mm" format (e.g., "09:00", "22:00").
    /// </summary>
    [Required(ErrorMessage = "Start time is required.")]
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// End time in "HH:mm" format (e.g., "17:30", "06:00").
    /// </summary>
    [Required(ErrorMessage = "End time is required.")]
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// Flag indicating if the shift crosses midnight into the next day.
    /// </summary>
    public bool IsOvernight { get; set; }

    /// <summary>
    /// Grace time in minutes for attendance marking (must be >= 0).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Grace time must be greater than or equal to 0.")]
    public double GraceTime { get; set; }
}
