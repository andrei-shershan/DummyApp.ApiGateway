using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class BlobServiceHttpClient : IBlobServiceClient
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

        _logger.LogInformation("Sending upload request to BlobService. FileName: {FileName}, JsonLength: {Length}", fileName, json.Length);

        var response = await _httpClient.PostAsync(
            "api/images/upload",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var contentX = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("BlobService returned {StatusCode} when uploading image: {Content}", response.StatusCode, contentX);
            throw new InvalidOperationException($"BlobService returned {(int)response.StatusCode}: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<UploadImageResponse>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrEmpty(result.Url))
        {
            throw new InvalidOperationException("Unexpected response from BlobService when uploading image.");
        }

        return result.Url;
    }

    private sealed record UploadImageResponse(string Url);
}
