namespace CLMS_APIs.Models.DTOs;

public class OtherMasterListDto
{
    public int MasterTypeId { get; set; }
    public int MasterId { get; set; }
    public string MasterName { get; set; } = string.Empty;
    public string MasterType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? Status { get; set; }
}
