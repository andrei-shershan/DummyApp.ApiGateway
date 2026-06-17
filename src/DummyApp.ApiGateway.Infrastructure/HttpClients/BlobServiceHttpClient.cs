using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.Http;

public sealed class BlobServiceHttpClient : IBlobServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlobServiceHttpClient> _logger;

    public BlobServiceHttpClient(HttpClient httpClient, ILogger<BlobServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new { base64Image, fileName });
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            "api/images/upload",
            httpContent,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("BlobService returned {StatusCode} when uploading image: {Content}", response.StatusCode, content);
            return null;
        }

        try
        {
            var result = await response.Content.ReadFromJsonAsync<UploadImageResponse>(cancellationToken: cancellationToken);
            if (result is null || string.IsNullOrEmpty(result.Url))
            {
                _logger.LogError("Unexpected response from BlobService when uploading image.");
                return null;
            }

            return result.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse response from BlobService when uploading image.");
            return null;
        }
    }

    private sealed record UploadImageResponse(string Url);
}
