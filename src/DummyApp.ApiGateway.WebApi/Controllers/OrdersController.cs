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
public sealed class OrdersController : ControllerBase
{
    private const string CompletedOrdersCookieName = "CompletedOrders";
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("completed")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompletedOrders()
    {
        var cookieValue = Request.Cookies[CompletedOrdersCookieName];
        if (string.IsNullOrWhiteSpace(cookieValue) || !Guid.TryParse(cookieValue, out var token))
        {
            _logger.LogWarning("Completed orders cookie is missing or invalid.");
            return NotFound();
        }

        var summaries = await _mediator.Send(new GetCompletedOrdersQuery(token));
        if (summaries is null)
        {
            return NotFound();
        }

        return Ok(summaries);
    }
}
