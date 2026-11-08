using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class CategoryDto
{
    [Required(ErrorMessage = "請輸入分類名稱。")]
    [StringLength(100, ErrorMessage = "分類名稱不能超過 100 個字元。")]
    public string Name { get; set; } = string.Empty;
}