using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiniERP.Controllers;

[Route("api/suppliers")]
[ApiController]
[Authorize]
[AutoValidateAntiforgeryToken]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/suppliers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.ContactPerson,
                s.Email,
                s.Phone
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    // GET: api/suppliers/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.ContactPerson,
                s.Email,
                s.Phone
            })
            .FirstOrDefaultAsync();

        if (supplier == null)
            return NotFound();

        return Ok(supplier);
    }

    // POST: api/suppliers
    [HttpPost]
    public async Task<IActionResult> Create(SupplierDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = supplier.Id },
            new
            {
                supplier.Id,
                supplier.Name,
                supplier.ContactPerson,
                supplier.Email,
                supplier.Phone
            });
    }

    // PUT: api/suppliers/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, SupplierDto dto)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
            return NotFound();

        supplier.Name = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Email = dto.Email;
        supplier.Phone = dto.Phone;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/suppliers/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
            return NotFound();

        bool hasProducts = await _context.Products
            .AnyAsync(p => p.SupplierId == id);

        bool hasOrders = await _context.PurchaseOrders
            .AnyAsync(p => p.SupplierId == id);

        if (hasProducts || hasOrders)
        {
            return Conflict(
                "此供應商已有關聯產品或採購單，無法刪除。");
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}