using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class BasketController : ControllerBase
{
    private const string BasketCookieName = "BasketId";
    private readonly IMediator _mediator;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IMediator mediator, ILogger<BasketController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddArtworkToBasketRequest request)
    {
        if (request is null || request.ArtworkId == Guid.Empty)
        {
            _logger.LogWarning("AddItem failed due to invalid request body.");
            return BadRequest("ArtworkId is required.");
        }

        var quantity = request.Quantity ?? 1;
        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            orderId = Guid.NewGuid();
            Response.Cookies.Append(BasketCookieName, orderId.ToString("D"), new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        var command = new AddArtworkToBasketCommand(orderId, request.ArtworkId, quantity);
        var added = await _mediator.Send(command);
        if (!added)
        {
            _logger.LogError("Failed to update basket item {ArtworkId} in basket {BasketId} with quantity {Quantity}.", request.ArtworkId, orderId, quantity);
            return BadRequest("Unable to update basket item.");
        }

        return Ok(new { orderId });
    }

    [HttpGet("items")]
    [ProducesResponseType(typeof(IEnumerable<DummyApp.ApiGateway.Infrastructure.Models.Dtos.OrderItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItems()
    {
        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            return NotFound();
        }

        var result = await _mediator.Send(new DummyApp.ApiGateway.Infrastructure.CQRS.Queries.GetOrderItemsQuery(orderId));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
