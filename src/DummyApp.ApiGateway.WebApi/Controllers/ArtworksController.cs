using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
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
    public async Task<IActionResult> GetArtworks()
    {
        var artworks = await _mediator.Send(new GetArtworksQuery());
        return Ok(artworks);
    }

    [HttpPost]
    [Authorize(Roles = "Creator")]
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
            body.IsActive,
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
}
