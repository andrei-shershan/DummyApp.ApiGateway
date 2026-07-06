using System.Security.Claims;
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
[Authorize]
public sealed class ArtworksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ArtworksController> _logger;

    public ArtworksController(IMediator mediator, ILogger<ArtworksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetArtworks([FromQuery] string? creatorId, [FromQuery] bool? isActive)
    {
        var artworks = await _mediator.Send(new GetArtworksQuery(creatorId, isActive));
        return Ok(artworks);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArtworkById([FromRoute] int id, [FromQuery] bool activeOnly = true)
    {
        var artwork = await _mediator.Send(new GetArtworkByIdQuery(id, activeOnly));
        if (artwork is null)
        {
            return NotFound();
        }

        return Ok(artwork);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Creator)]
    public async Task<IActionResult> CreateArtwork([FromBody] CreateArtworkBodyRequest body)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateArtwork failed due to invalid model state: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        var creatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            _logger.LogError("CreateArtwork failed: creatorId is missing. User claims logged above.");
            return Forbid();
        }

        var command = new CreateArtworkCommand(
            body.Name,
            body.FileName,
            body.Description,
            body.CreationDate,
            false, // isActive is set to false by default
            body.UploadedImage,
            creatorId);

        var result = await _mediator.Send(command);
        if (result is null)
        {
            _logger.LogError("CreateArtwork failed: result is null after sending command.");
            return BadRequest("An error occurred while creating the artwork.");
        }

        return Created($"api/artworks/{result.Id}", result);
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = RoleNames.Creator)]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateArtworkActive([FromRoute] int id, [FromBody] UpdateArtworkIsActiveRequest body)
    {
        if (body is null)
        {
            _logger.LogWarning("UpdateArtworkActive failed due to null request body.");
            return BadRequest("Request body is required.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("UpdateArtworkActive failed due to invalid model state: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(new UpdateArtworkIsActiveCommand(id, body.IsActive.Value));
        if (result is null)
        {
            _logger.LogError("UpdateArtworkActive failed: result is null after sending command.");
            return BadRequest("An error occurred while updating artwork active state.");
        }

        return Ok(result);
    }
}
