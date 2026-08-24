using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        var profile = await _mediator.Send(new GetUserProfileQuery(userId), cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateCurrentUserProfileRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        if (request is null || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("FirstName and LastName are required.");
        }

        var updatedProfile = await _mediator.Send(new UpdateUserProfileCommand(userId, request.FirstName.Trim(), request.LastName.Trim()), cancellationToken);
        if (updatedProfile is null)
        {
            return NotFound();
        }

        return Ok(updatedProfile);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UpdateCurrentUserAvatar([FromBody] UpdateCurrentUserAvatarRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        if (request is null || string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.Base64Image))
        {
            return BadRequest("FileName and Base64Image are required.");
        }

        var updatedProfile = await _mediator.Send(new UpdateUserAvatarCommand(userId, request.FileName.Trim(), request.Base64Image.Trim()), cancellationToken);
        if (updatedProfile is null)
        {
            return NotFound();
        }

        return Ok(updatedProfile);
    }

    public sealed record UpdateCurrentUserProfileRequest(string FirstName, string LastName);
    public sealed record UpdateCurrentUserAvatarRequest(string FileName, string Base64Image);
}
