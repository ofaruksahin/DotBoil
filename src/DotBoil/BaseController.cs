using DotBoil.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        public readonly IMediator _mediator;

        public BaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [NonAction]
        public IActionResult Response(BaseResponse response)
        {
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
