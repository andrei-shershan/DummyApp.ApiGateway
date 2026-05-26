using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/artworks")]
[Authorize]
public sealed class ArtworksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArtworksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Creator")]
    public async Task<IActionResult> CreateArtwork([FromBody] CreateArtworkCommand command)
    {
        var result = await _mediator.Send(command);
        return Created($"api/artworks/{result.Id}", result);
    }
}
