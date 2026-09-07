using System.ComponentModel.DataAnnotations;

namespace CLMS_APIs.Models.DTOs;

public class OtherMasterUpdateDto
{
    [Required(ErrorMessage = "Master type is required.")]
    [MaxLength(150, ErrorMessage = "Master type cannot exceed 150 characters.")]
    public string MasterType { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }
}
