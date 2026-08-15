using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class FileServiceClient : IFileServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileServiceClient> _logger;
    private readonly string? _fileServiceSecretKey;

    public FileServiceClient(HttpClient httpClient, ILogger<FileServiceClient> logger, IOptions<FileServiceOptions> fileServiceOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _fileServiceSecretKey = fileServiceOptions.Value.SecretKey;
    }

    public async Task<string> GenerateQrCodeBase64Async(string text, int pixelsPerModule, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required to generate a QR code.", nameof(text));

        var requestUri = string.IsNullOrWhiteSpace(_fileServiceSecretKey)
            ? "api/file/qrcode"
            : $"api/file/qrcode?code={Uri.EscapeDataString(_fileServiceSecretKey)}";

        var requestBody = JsonSerializer.Serialize(new
        {
            text,
            pixelsPerModule
        });

        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }

        public async Task<byte[]> GeneratePdfAsync(GeneratePdfRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var requestUri = string.IsNullOrWhiteSpace(_fileServiceSecretKey)
                ? "api/file/pdf"
                : $"api/file/pdf?code={Uri.EscapeDataString(_fileServiceSecretKey)}";

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
