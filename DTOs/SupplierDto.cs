using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class SupplierDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }
}