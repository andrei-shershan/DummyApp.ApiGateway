using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class AnalyticsServiceClient : IAnalyticsServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnalyticsServiceClient> _logger;
    private readonly string? _analyticsServiceSecretKey;

    public AnalyticsServiceClient(
        HttpClient httpClient,
        ILogger<AnalyticsServiceClient> logger,
        IOptions<AnalyticsServiceOptions> analyticsServiceOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _analyticsServiceSecretKey = analyticsServiceOptions.Value.SecretKey;
    }

    public async Task PublishEventAsync(AnalyticsEventRequest request, CancellationToken cancellationToken)
    {
        var requestUri = string.IsNullOrWhiteSpace(_analyticsServiceSecretKey)
            ? "api/analytics/event"
            : $"api/analytics/event?code={Uri.EscapeDataString(_analyticsServiceSecretKey)}";

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to publish analytics event. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, ResponseBody: {ResponseBody}",
                response.StatusCode,
                response.ReasonPhrase,
                responseBody);
            throw new InvalidOperationException($"Analytics service returned status {response.StatusCode}.");
        }
    }

    public async Task<IEnumerable<AnalyticsEventDto>> GetAnalyticsAsync(int periodDays, CancellationToken cancellationToken)
    {
        var requestUri = string.IsNullOrWhiteSpace(_analyticsServiceSecretKey)
            ? $"api/analytics?periodDays={periodDays}"
            : $"api/analytics?periodDays={periodDays}&code={Uri.EscapeDataString(_analyticsServiceSecretKey)}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to get analytics data. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, ResponseBody: {ResponseBody}",
                response.StatusCode,
                response.ReasonPhrase,
                responseBody);

            throw new InvalidOperationException($"Analytics service returned status {response.StatusCode}.");
        }

        try
        {
            var analytics = await response.Content.ReadFromJsonAsync<IEnumerable<AnalyticsEventDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return analytics ?? Array.Empty<AnalyticsEventDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize analytics response.");
            throw;
        }
    }
}
