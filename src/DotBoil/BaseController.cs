using DotBoil.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotBoil
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IMediator _mediator;

        protected BaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Response(BaseResponse response)
        {
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
