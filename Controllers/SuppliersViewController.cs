using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniERP.Controllers;

[Authorize]
[Route("Suppliers")]
public class SuppliersViewController : Controller
{
    private readonly IAntiforgery _antiforgery;

    public SuppliersViewController(IAntiforgery antiforgery)
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