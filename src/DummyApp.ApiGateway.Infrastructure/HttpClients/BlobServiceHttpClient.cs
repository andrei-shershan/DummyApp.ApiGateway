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

    public async Task<string> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new { base64Image, fileName });
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            "api/images/upload",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var contentX = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("BlobService returned {StatusCode} when uploading image: {Content}", response.StatusCode, contentX);
            return null!;
        }

        var result = await response.Content.ReadFromJsonAsync<UploadImageResponse>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrEmpty(result.Url))
        {
            _logger.LogError("Unexpected response from BlobService when uploading image.");
            return null!;
        }

        return result.Url;
    }

    private sealed record UploadImageResponse(string Url);
}
