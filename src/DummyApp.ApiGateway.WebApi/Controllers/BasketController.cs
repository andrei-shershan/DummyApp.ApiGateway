using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Configuration;
using DummyApp.ApiGateway.WebApi.Models;
using DummyApp.ApiGateway.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Linq;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class BasketController : ControllerBase
{
    private const string BasketCookieName = "BasketId";
    private readonly IMediator _mediator;
    private readonly ILogger<BasketController> _logger;
    private readonly ApiGatewaySettings _settings;
    private readonly IStripeSessionService _stripeSessionService;

    public BasketController(IMediator mediator, ILogger<BasketController> logger, IStripeSessionService stripeSessionService, ApiGatewaySettings settings)
    {
        _mediator = mediator;
        _logger = logger;
        _stripeSessionService = stripeSessionService;
        _settings = settings;
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

    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout()
    {
        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            _logger.LogWarning("Checkout failed because basket cookie is missing or invalid.");
            return BadRequest("Basket is required to start checkout.");
        }

        var summary = await _mediator.Send(new GetOrderSummaryQuery(orderId));
        if (summary is null || !summary.Items.Any())
        {
            _logger.LogWarning("Checkout failed because order summary is missing or empty for basket {BasketId}.", orderId);
            return BadRequest("Order summary is not available.");
        }

        if (!summary.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase)
            && !summary.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Checkout failed because order {BasketId} has invalid status {Status}.", orderId, summary.Status);
            return BadRequest("Order is not ready for checkout.");
        }

        var lineItems = new List<SessionLineItemOptions>();
        foreach (var item in summary.Items)
        {
            if (!item.PriceValue.HasValue || item.PriceValue.Value <= 0m)
            {
                _logger.LogWarning("Checkout failed because order {BasketId} contains invalid price for item {ArtworkId}.", orderId, item.ArtworkId);
                return BadRequest("Order contains invalid item pricing.");
            }

            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "pln",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name,
                        Description = item.Description ?? string.Empty
                    },
                    UnitAmount = (long)Math.Round(item.PriceValue.Value * 100m)
                },
                Quantity = item.Quantity
            });
        }

        var successUrl = !string.IsNullOrWhiteSpace(_settings.Stripe.SuccessUrl)
            ? _settings.Stripe.SuccessUrl!
            : $"{Request.Scheme}://{Request.Host}/";
        var cancelUrl = !string.IsNullOrWhiteSpace(_settings.Stripe.CancelUrl)
            ? _settings.Stripe.CancelUrl!
            : $"{Request.Scheme}://{Request.Host}/basket";

        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card", "blik" },
            Mode = "payment",
            LineItems = lineItems,
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = orderId.ToString("D"),
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString("D"),
                ["siteId"] = _settings.Stripe.SiteId ?? "unknown"
            }
        };

        var session = await _stripeSessionService.CreateAsync(sessionOptions);

        return Ok(new CheckoutResponse(session.Url ?? string.Empty));
    }

    [HttpPost("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetStatus([FromBody] SetBasketStatusRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("SetStatus failed due to invalid request body.");
            return BadRequest("Status is required.");
        }

        if (!request.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase)
            && !request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SetStatus failed due to unsupported status {Status}.", request.Status);
            return BadRequest("Invalid status.");
        }

        var basketId = Request.Cookies[BasketCookieName];
        if (!Guid.TryParse(basketId, out var orderId))
        {
            return BadRequest("Basket is required to update status.");
        }

        var result = await _mediator.Send(new SetOrderStatusCommand(orderId, request.Status));
        if (!result)
        {
            _logger.LogError("Failed to transition basket {BasketId} to status {Status}.", orderId, request.Status);
            return BadRequest("Unable to update basket status.");
        }

        return Ok();
    }
}

public sealed record CheckoutResponse(string Url);

