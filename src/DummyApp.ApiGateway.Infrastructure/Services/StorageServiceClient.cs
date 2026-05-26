using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class StorageServiceClient : IStorageServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StorageServiceClient> _logger;

    public StorageServiceClient(HttpClient httpClient, ILogger<StorageServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CreateArtworkCommandResult> CreateArtworkAsync(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        var dto = new
        {
            request.Name,
            request.Description,
            request.CreationDate,
            request.ImgUrl,
            request.SmallImgUrl,
            request.IsActive
        };

        var response = await _httpClient.PostAsJsonAsync("api/artworks", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Storage service returned {StatusCode} when creating artwork: {Content}", response.StatusCode, content);
            throw new InvalidOperationException($"Storage service returned {(int)response.StatusCode}: {response.ReasonPhrase}");
        }

        var created = await response.Content.ReadFromJsonAsync<CreateArtworkCommandResult>(cancellationToken: cancellationToken);
        if (created is null)
        {
            throw new InvalidOperationException("Unexpected response from storage service when creating artwork.");
        }

        return created;
    }
}
