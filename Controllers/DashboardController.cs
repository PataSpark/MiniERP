using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;

namespace MiniERP.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var productCount = await _context.Products.CountAsync();
        var supplierCount = await _context.Suppliers.CountAsync();
        var orderCount = await _context.PurchaseOrders.CountAsync();

        var totalPurchaseAmount =
            await _context.PurchaseOrderItems
                .SumAsync(i => (decimal?)(i.Quantity * i.UnitPrice))
                ?? 0m;

        var orderStatus = await _context.PurchaseOrders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var supplierAmounts = await _context.PurchaseOrders
            .GroupBy(o => new
            {
                o.SupplierId,
                o.Supplier.Name
            })
            .Select(g => new
            {
                SupplierName = g.Key.Name,
                TotalAmount = g
                    .SelectMany(o => o.Items)
                    .Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();

        return Ok(new
        {
            ProductCount = productCount,
            SupplierCount = supplierCount,
            OrderCount = orderCount,
            TotalPurchaseAmount = totalPurchaseAmount,
            OrderStatus = orderStatus,
            SupplierAmounts = supplierAmounts
        });
    }
}