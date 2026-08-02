using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _mediator.Send(new GetRolesQuery());
        return Ok(roles);
    }

    [HttpGet("print-sizes")]
    public async Task<IActionResult> GetPrintSizes()
    {
        var printSizes = await _mediator.Send(new GetPrintSizesQuery());
        return Ok(printSizes);
    }

    [HttpPost("invite")]
    public async Task<IActionResult> SendInvite([FromBody] SendInviteRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        var result = await _mediator.Send(new SendInviteCommand(request.Email));
        if (!result)
        {
            return BadRequest("Failed to send invite.");
        }

        return Ok();
    }

    [HttpPut("users/{id}/active")]
    public async Task<IActionResult> UpdateUserActiveState([FromRoute] string id, [FromBody] UpdateUserActiveStateRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var result = await _mediator.Send(new UpdateUserActiveStateCommand(id, request.IsActive.Value));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    public sealed record SendInviteRequest(string Email);
}
