using DotBoil.AuthGuard.Application.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil.AuthGuard.Controllers;

[Authorize]
public class ApplicationController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IPermissionService _permissionService;

    public ApplicationController(
        IMenuService menuService,
        IPermissionService permissionService)
    {
        _menuService = menuService;
        _permissionService = permissionService;
    }
    
    [HttpPost("app/menu")]
    public async Task<IActionResult> GetMenuList()
    {
        var menuList = await _menuService.GetMenuItems();
        return Ok(menuList);
    }

    [HttpPost("app/permission")]
    public async Task<IActionResult> CheckPermission([FromQuery] string controller, [FromQuery] string action)
    {
        var hasPermission = await _permissionService.CheckPermission(controller, action);

        if (!hasPermission)
            return Forbid();

        return Ok();
    }
}