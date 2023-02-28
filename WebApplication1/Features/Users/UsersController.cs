using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GuidesApi.Features.Users
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [Produces(ContentTypes.Json, Type = typeof(bool))]
        public async Task<IActionResult> Post([FromBody] CreateEditUserCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

    }
}
