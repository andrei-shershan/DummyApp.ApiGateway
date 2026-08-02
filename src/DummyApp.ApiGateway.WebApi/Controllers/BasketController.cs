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
        if (quantity <= 0)
        {
            _logger.LogWarning("AddItem failed because quantity {Quantity} is not positive.", quantity);
            return BadRequest("Quantity must be greater than zero.");
        }

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

        var command = new AddArtworkToBasketCommand(orderId, request.ArtworkId, quantity, request.PrintSizeId, request.PriceId);
        var added = await _mediator.Send(command);
        if (!added)
        {
            _logger.LogError("Failed to add basket item {ArtworkId} to basket {BasketId} with quantity {Quantity}.", request.ArtworkId, orderId, quantity);
            return BadRequest("Unable to add basket item.");
        }

        return Ok(new { orderId });
    }

    [HttpPatch("items/{artworkId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItem([FromRoute] Guid artworkId, [FromBody] UpdateBasketItemRequest request)
    {
        if (artworkId == Guid.Empty || request is null || request.Quantity < 0)
        {
            _logger.LogWarning("UpdateItem failed due to invalid request body.");
            return BadRequest("Valid artworkId and non-negative quantity are required.");
        }

        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            return BadRequest("Basket is required to update an item.");
        }

        var command = new UpdateArtworkInBasketCommand(orderId, artworkId, request.Quantity, request.PrintSizeId, request.PriceId);
        var updated = await _mediator.Send(command);
        if (!updated)
        {
            _logger.LogError("Failed to update basket item {ArtworkId} in basket {BasketId} with quantity {Quantity}.", artworkId, orderId, request.Quantity);
            return BadRequest("Unable to update basket item.");
        }

        return Ok();
    }

    [HttpGet]
    [ProducesResponseType(typeof(OrderSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            return NotFound();
        }

        var result = await _mediator.Send(new GetOrderSummaryQuery(orderId));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("print-sizes")]
    [ProducesResponseType(typeof(IEnumerable<PrintSizeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrintSizes()
    {
        var result = await _mediator.Send(new GetPrintSizesQuery());
        return Ok(result);
    }

    [HttpPost("pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pay()
    {
        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            return BadRequest("Basket is required to pay order.");
        }

        var result = await _mediator.Send(new PayOrderCommand(orderId));
        if (!result)
        {
            _logger.LogError("Failed to transition basket {BasketId} to processing.", orderId);
            return BadRequest("Unable to pay order.");
        }

        return Ok();
    }
}
