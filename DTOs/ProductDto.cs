using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class ProductDto
{
    [Required(ErrorMessage = "請輸入產品名稱。")]
    [StringLength(100, ErrorMessage = "產品名稱不能超過 100 個字元。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入 SKU。")]
    [StringLength(50, ErrorMessage = "SKU 不能超過 50 個字元。")]
    public string SKU { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99",
        ErrorMessage = "價格必須介於 0 到 9999999999999999.99。")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "庫存不能為負數。")]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的產品分類。")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的供應商。")]
    public int SupplierId { get; set; }
}