using DotBoil.AuthGuard.Application.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil.AuthGuard.Controllers;

[Authorize]
public class ApplicationController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IPermissionService _permissionService;
    private readonly IUserService _userService;

    public ApplicationController(
        IMenuService menuService,
        IPermissionService permissionService,
        IUserService userService)
    {
        _menuService = menuService;
        _permissionService = permissionService;
        _userService = userService;
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

    [HttpPost("app/userinfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        var getUserInfo = await _userService.GetUserInfo();
        if (!getUserInfo.Claims.Any())
            return Unauthorized();
        
        return Ok(getUserInfo.Claims);
    }
}