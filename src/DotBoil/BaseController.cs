using DotBoil.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckRoleAuthorizationFilter))]
    [ServiceFilter(typeof(CheckAppModuleAuthorizationFilter))]
    public abstract class BaseController : ControllerBase
    {
        [NonAction]
        public IActionResult Response(BaseResponse response)
        {
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
