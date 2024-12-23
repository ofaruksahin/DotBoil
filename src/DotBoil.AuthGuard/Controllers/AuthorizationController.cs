using Microsoft.AspNetCore.Mvc;

namespace DotBoil.AuthGuard.Controllers;

public class AuthorizationController : Controller
{
    [HttpGet("/connect/authorize")]
    public IActionResult Authorize()
    {
        return View();
    }
}