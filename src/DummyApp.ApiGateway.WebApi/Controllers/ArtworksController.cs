using System.Security.Claims;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/artworks")]
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
        _logger.LogInformation("CreateArtwork called. User authenticated: {IsAuthenticated}, name: {Name}, authType: {AuthType}",
            User.Identity?.IsAuthenticated,
            User.Identity?.Name,
            User.Identity?.AuthenticationType);

        foreach (var claim in User.Claims)
        {
            _logger.LogInformation("User claim: {ClaimType} = {ClaimValue}", claim.Type, claim.Value);
        }

        // In JWT bearer auth, the standard OIDC "sub" claim is often mapped to
        // ClaimTypes.NameIdentifier, so the raw "sub" may not be available here.
        var creatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(creatorId))
        {
            _logger.LogWarning("CreateArtwork failed: creatorId is missing. User claims logged above.");
            return Forbid();
        }

        var command = new CreateArtworkCommand(
            body.Name,
            body.Description,
            body.CreationDate,
            body.ImgUrl,
            body.SmallImgUrl,
            body.IsActive,
            body.UploadedImage,
            creatorId);

        var result = await _mediator.Send(command);
        return Created($"api/artworks/{result.Id}", result);
    }
}
