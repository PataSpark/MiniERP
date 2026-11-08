using System.ComponentModel.DataAnnotations;

namespace MiniERP.DTOs;

public class PurchaseOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的產品。")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "採購數量至少為 1。")]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99",
        ErrorMessage = "採購單價必須大於或等於 0。")]
    public decimal UnitPrice { get; set; }
}