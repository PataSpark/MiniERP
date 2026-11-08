using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class CreateUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Admin|Staff)$")]
    public string Role { get; set; } = "Staff";
}