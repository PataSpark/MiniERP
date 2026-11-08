using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiniERP.Controllers;

[Route("api/products")]
[ApiController]
[Authorize]
[AutoValidateAntiforgeryToken]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await (
            from p in _context.Products.AsNoTracking()
            join c in _context.Categories
                on p.CategoryId equals c.Id
            join s in _context.Suppliers
                on p.SupplierId equals s.Id
            select new
            {
                p.Id,
                p.Name,
                p.SKU,
                p.Price,
                p.StockQuantity,
                p.CategoryId,
                CategoryName = c.Name,
                p.SupplierId,
                SupplierName = s.Name
            }
        ).ToListAsync();

        return Ok(products);
    }

    // GET: api/products/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await (
            from p in _context.Products.AsNoTracking()
            join c in _context.Categories
                on p.CategoryId equals c.Id
            join s in _context.Suppliers
                on p.SupplierId equals s.Id
            where p.Id == id
            select new
            {
                p.Id,
                p.Name,
                p.SKU,
                p.Price,
                p.StockQuantity,
                p.CategoryId,
                CategoryName = c.Name,
                p.SupplierId,
                SupplierName = s.Name
            }
        ).FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        if (!await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest("指定的產品分類不存在。");

        if (!await _context.Suppliers
            .AnyAsync(s => s.Id == dto.SupplierId))
            return BadRequest("指定的供應商不存在。");

        if (await _context.Products
            .AnyAsync(p => p.SKU == dto.SKU))
            return Conflict("SKU 已存在。");

        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            new { product.Id });
    }

    // PUT: api/products/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, ProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        if (!await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest("指定的產品分類不存在。");

        if (!await _context.Suppliers
            .AnyAsync(s => s.Id == dto.SupplierId))
            return BadRequest("指定的供應商不存在。");

        if (await _context.Products
            .AnyAsync(p => p.SKU == dto.SKU && p.Id != id))
            return Conflict("SKU 已存在。");

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        bool hasOrderItems = await _context.PurchaseOrderItems
            .AnyAsync(i => i.ProductId == id);

        if (hasOrderItems)
            return Conflict("產品已有採購紀錄，無法刪除。");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}