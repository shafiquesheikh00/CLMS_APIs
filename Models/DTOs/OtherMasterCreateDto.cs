namespace CLMS_APIs.Models.DTOs;

public class OtherMasterCreateDto
{
    /// <summary>
    /// ID of an existing category. Pass null when creating a new category with NewMasterName.
    /// </summary>
    public int? MasterId { get; set; }

    /// <summary>
    /// Category name required when MasterId is null (creating a brand-new category).
    /// </summary>
    public string? NewMasterName { get; set; }

    /// <summary>
    /// The master type value (e.g., "Civil", "Electrical").
    /// </summary>
    public string MasterType { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for this master type entry.
    /// </summary>
    public string? Description { get; set; }
}
