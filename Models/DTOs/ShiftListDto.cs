namespace CLMS_APIs.Models.DTOs;

public class ShiftListDto
{
    public decimal ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public decimal ShiftHours { get; set; }
    public bool IsOvernight { get; set; }
    public double GraceTime { get; set; }
}
