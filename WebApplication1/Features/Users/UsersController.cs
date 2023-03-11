using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GuidesApi.Features.Users
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("create-edit")]
        [Produces(ContentTypes.Json, Type = typeof(bool))]
        public async Task<IActionResult> Post([FromBody] CreateEditUserCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("collect")]
        [Produces(ContentTypes.Json, Type = typeof(bool))]
        public async Task<IActionResult> Get()
        {
            return Ok(await _mediator.Send(new GetUsersCommand()));
        }

        [HttpGet("collect/{id}")]
        [Produces(ContentTypes.Json, Type = typeof(bool))]
        public async Task<IActionResult> Get(string id)
        {
            return Ok((await _mediator.Send(new GetUsersCommand
            {
                Id = id
            })).FirstOrDefault());
        }

    }
}
