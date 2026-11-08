using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniERP.DTOs;
using MiniERP.Models;

namespace MiniERP.Controllers;

[Route("api/auth")]
[ApiController]
[AutoValidateAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return Unauthorized("帳號或密碼錯誤。");

        var result = await _signInManager.PasswordSignInAsync(
            user,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return Unauthorized("登入失敗，請檢查帳號密碼或帳號狀態。");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            Message = "登入成功",
            user.Id,
            user.Email,
            user.FullName,
            Roles = roles
        });
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "已登出" });
    }

    // GET: api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            Roles = roles
        });
    }
}