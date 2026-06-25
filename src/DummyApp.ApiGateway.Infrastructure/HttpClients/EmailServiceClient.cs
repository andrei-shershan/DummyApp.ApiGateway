using System.Net.Http.Json;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class EmailServiceClient : IEmailServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailServiceClient> _logger;
    private readonly string? _emailServiceSecretKey;

    public EmailServiceClient(HttpClient httpClient, ILogger<EmailServiceClient> logger, IOptions<EmailServiceOptions> emailServiceOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _emailServiceSecretKey = emailServiceOptions.Value.SecretKey;
    }

public sealed class InviteEmailRequest
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}

    public async Task<bool> SendInviteAsync(string email, string token, CancellationToken cancellationToken)
    {
        var request = new InviteEmailRequest { Email = email, Token = token };
        var requestUri = string.IsNullOrWhiteSpace(_emailServiceSecretKey)
            ? "api/email/invite"
            : $"api/email/invite?code={Uri.EscapeDataString(_emailServiceSecretKey)}";

        var response = await _httpClient.PostAsJsonAsync(requestUri, request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to send invite email to {Email}. Status code: {StatusCode}, Reason: {ReasonPhrase}", email, response.StatusCode, response.ReasonPhrase);
            var requestBody = await response.RequestMessage?.Content?.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Request body: {RequestBody}", requestBody);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Response body: {ResponseBody}", responseBody);
            _logger.LogError("Email service returned status {StatusCode} for invite to {Email}.", response.StatusCode, email);
            return false;
        }

        return true;
    }
}
