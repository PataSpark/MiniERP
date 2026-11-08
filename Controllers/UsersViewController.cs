using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniERP.Controllers;

[Authorize(Roles = "Admin")]
[Route("Users")]
public class UsersViewController : Controller
{
    private readonly IAntiforgery _antiforgery;

    public UsersViewController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        ViewData["CsrfToken"] = tokens.RequestToken;

        return View();
    }
}