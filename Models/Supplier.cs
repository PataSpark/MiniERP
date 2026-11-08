using System.ComponentModel.DataAnnotations;

namespace MiniERP.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    public ICollection<Product> Products { get; set; }
        = new List<Product>();

    public ICollection<PurchaseOrder> PurchaseOrders
    { get; set; } = new List<PurchaseOrder>();
}