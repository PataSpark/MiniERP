using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiniERP.Controllers;

[Route("api/purchaseorders")]
[ApiController]
[Authorize]
[AutoValidateAntiforgeryToken]
public class PurchaseOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/purchaseorders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.Status,
                o.SupplierId,
                SupplierName = o.Supplier.Name,
                ItemCount = o.Items.Count,
                TotalAmount = o.Items.Sum(
                    i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET: api/purchaseorders/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.Status,
                o.SupplierId,
                SupplierName = o.Supplier.Name,

                Items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    Subtotal = i.Quantity * i.UnitPrice
                }).ToList(),

                TotalAmount = o.Items.Sum(
                    i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    // POST: api/purchaseorders
    [HttpPost]
    public async Task<IActionResult> Create(PurchaseOrderDto dto)
    {
        if (!await ValidateOrder(dto))
            return BadRequest("供應商或產品資料不正確。");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var order = new PurchaseOrder
        {
            SupplierId = dto.SupplierId,
            Status = dto.Status,
            OrderDate = DateTime.UtcNow,
            Items = dto.Items.Select(i => new PurchaseOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            new { order.Id });
    }

    // PUT: api/purchaseorders/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, PurchaseOrderDto dto)
    {
        if (!await ValidateOrder(dto))
            return BadRequest("供應商或產品資料不正確。");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var order = await _context.PurchaseOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        order.SupplierId = dto.SupplierId;
        order.Status = dto.Status;

        _context.PurchaseOrderItems.RemoveRange(order.Items);

        order.Items = dto.Items.Select(i => new PurchaseOrderItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return NoContent();
    }

    // DELETE: api/purchaseorders/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);

        if (order == null)
            return NotFound();

        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 驗證供應商與產品
    private async Task<bool> ValidateOrder(PurchaseOrderDto dto)
    {
        bool supplierExists = await _context.Suppliers
            .AnyAsync(s => s.Id == dto.SupplierId);

        if (!supplierExists)
            return false;

        var productIds = dto.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        int validCount = await _context.Products
            .CountAsync(p =>
                productIds.Contains(p.Id) &&
                p.SupplierId == dto.SupplierId);

        return validCount == productIds.Count;
    }
}