using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class PurchaseOrderDto
{
    [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的供應商。")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "請選擇採購單狀態。")]
    [RegularExpression(
        "^(Pending|Approved|Cancelled)$",
        ErrorMessage = "採購單狀態不正確。")]
    public string Status { get; set; } = "Pending";

    [Required(ErrorMessage = "請輸入採購明細。")]
    [MinLength(1, ErrorMessage = "採購單至少需要一筆明細。")]
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}