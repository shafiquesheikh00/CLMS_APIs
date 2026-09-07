namespace CLMS_APIs.Models.DTOs;

public class HolidayDto
{
    public int Id { get; set; }
    public DateTime HolidayDate { get; set; }
    public string HolidayDesc { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
}
