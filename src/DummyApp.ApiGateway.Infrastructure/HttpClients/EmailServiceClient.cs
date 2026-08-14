using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

    public async Task<bool> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        var requestUri = string.IsNullOrWhiteSpace(_emailServiceSecretKey)
            ? "api/email/send"
            : $"api/email/send?code={Uri.EscapeDataString(_emailServiceSecretKey)}";

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to send email to {Recipients}. Status code: {StatusCode}, Reason: {ReasonPhrase}", string.Join(", ", request.Recipients), response.StatusCode, response.ReasonPhrase);
            var requestBody = response.RequestMessage?.Content is not null
                ? await response.RequestMessage.Content.ReadAsStringAsync(cancellationToken)
                : null;
            _logger.LogError("Request body: {RequestBody}", requestBody);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Response body: {ResponseBody}", responseBody);
            _logger.LogError("Email service returned status {StatusCode} for request to {RequestUri}.", response.StatusCode, requestUri);
            return false;
        }

        _logger.LogInformation("Successfully sent email to {Recipients} using template {Template}.", string.Join(", ", request.Recipients), request.Template);

        return true;
    }
}
