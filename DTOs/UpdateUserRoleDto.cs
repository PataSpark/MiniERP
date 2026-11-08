using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression("^(Admin|Staff)$")]
    public string Role { get; set; } = string.Empty;
}