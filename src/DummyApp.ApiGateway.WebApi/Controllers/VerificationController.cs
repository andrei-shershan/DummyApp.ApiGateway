using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class VerificationController : ControllerBase
{
    private const string CompletedOrdersCookieName = "CompletedOrders";
    private readonly IMediator _mediator;
    private readonly ILogger<VerificationController> _logger;

    public VerificationController(IMediator mediator, ILogger<VerificationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("send-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        var command = new SendVerificationCodeCommand(request.Email.Trim());
        var result = await _mediator.Send(command);
        if (!result)
        {
            _logger.LogError("SendVerificationCode command failed for email {Email}.", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to send verification code.");
        }

        return Ok();
    }

    [HttpPost("verify-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyVerificationCode([FromBody] VerifyVerificationCodeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Email and code are required.");
        }

        var command = new VerifyVerificationCodeCommand(request.Email.Trim(), request.Code.Trim());
        var result = await _mediator.Send(command);
        if (result is null || !result.Success)
        {
            return result?.IsServerError == true
                ? StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage ?? "Unable to verify verification code.")
                : BadRequest(result?.ErrorMessage ?? "Invalid or expired verification code.");
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = result.ExpiresAt
        };

        Response.Cookies.Append(CompletedOrdersCookieName, result.Token.ToString("D"), cookieOptions);

        return Ok();
    }
}
