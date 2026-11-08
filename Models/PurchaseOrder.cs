using System.ComponentModel.DataAnnotations;

namespace MiniERP.Models;

public class PurchaseOrder
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public ICollection<PurchaseOrderItem> Items
    { get; set; } = new List<PurchaseOrderItem>();
}