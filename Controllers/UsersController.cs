using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP.Controllers;

[Route("api/users")]
[ApiController]
[AutoValidateAntiforgeryToken]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .ToListAsync();

        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new
            {
                user.Id,
                user.Email,
                user.FullName,
                Roles = roles,
                IsLocked = user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return Ok(result);
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return Conflict("此 Email 已被使用。");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(
            user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(
            user, dto.Role);

        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = user.Id },
            new
            {
                user.Id,
                user.Email,
                user.FullName,
                dto.Role
            });
    }

    // PUT: api/users/{id}/role
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(
    string id,
    UpdateUserRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound("找不到使用者。");

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId)
            return BadRequest("不能修改自己的角色。");

        if (dto.Role != "Admin" && dto.Role != "Staff")
            return BadRequest("角色只能是 Admin 或 Staff。");

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count == 1 && currentRoles[0] == dto.Role)
            return NoContent();

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(
                user, currentRoles);

            if (!removeResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(removeResult.Errors);
            }

            var addResult = await _userManager.AddToRoleAsync(
                user, dto.Role);

            if (!addResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(addResult.Errors);
            }

            var stampResult =
                await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(stampResult.Errors);
            }

            await transaction.CommitAsync();

            return NoContent();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // PUT: api/users/{id}/lock
    [HttpPut("{id}/lock")]
    public async Task<IActionResult> LockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound("找不到使用者。");

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId)
            return BadRequest("不能停用自己的帳號。");

        var lockResult = await _userManager.SetLockoutEndDateAsync(
            user,
            DateTimeOffset.MaxValue
        );

        if (!lockResult.Succeeded)
            return BadRequest(lockResult.Errors);

        var stampResult =
            await _userManager.UpdateSecurityStampAsync(user);

        if (!stampResult.Succeeded)
            return BadRequest(stampResult.Errors);

        return NoContent();
    }

    // PUT: api/users/{id}/unlock
    [HttpPut("{id}/unlock")]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound("找不到使用者。");

        var result = await _userManager.SetLockoutEndDateAsync(
            user,
            null
        );

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

}