using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
[AutoValidateAntiforgeryToken]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Name,
                ProductCount = c.Products.Count
            })
            .OrderBy(c => c.Id)
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound("找不到此產品分類。");

        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("分類名稱不能為空白。");

        bool exists = await _context.Categories
            .AnyAsync(c => c.Name == name);

        if (exists)
            return Conflict("此分類名稱已存在。");

        var category = new Category
        {
            Name = name
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            new
            {
                category.Id,
                category.Name,
                ProductCount = 0
            });
    }

    // PUT: api/categories/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound("找不到此產品分類。");

        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("分類名稱不能為空白。");

        bool exists = await _context.Categories
            .AnyAsync(c => c.Name == name && c.Id != id);

        if (exists)
            return Conflict("此分類名稱已存在。");

        category.Name = name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/categories/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound("找不到此產品分類。");

        if (category.Products.Any())
            return Conflict("此分類仍有產品，無法刪除。");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}