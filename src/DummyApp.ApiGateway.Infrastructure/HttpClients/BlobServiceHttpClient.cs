using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.Http;

public sealed class BlobServiceHttpClient : IBlobServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlobServiceHttpClient> _logger;
    private readonly string? _blobServiceSecretKey;

    public BlobServiceHttpClient(HttpClient httpClient, ILogger<BlobServiceHttpClient> logger, IOptions<BlobStorageOptions> blobStorageOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _blobServiceSecretKey = blobStorageOptions.Value.SecretKey;
    }

    public async Task<string?> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new { base64Image, fileName });
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = string.IsNullOrWhiteSpace(_blobServiceSecretKey)
            ? "api/images/upload"
            : $"api/images/upload?code={Uri.EscapeDataString(_blobServiceSecretKey)}";

        var response = await _httpClient.PostAsync(
            requestUri,
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
