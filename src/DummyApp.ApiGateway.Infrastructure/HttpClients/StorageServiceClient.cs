using System.Net.Http.Json;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.Http;

public sealed class StorageServiceClient : IStorageServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StorageServiceClient> _logger;

    public StorageServiceClient(HttpClient httpClient, ILogger<StorageServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto artwork, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/artworks", artwork, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create artwork via storage service. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        try
        {
            var created = await response.Content.ReadFromJsonAsync<ArtworkDto>(cancellationToken: cancellationToken);
            if (created is null)
            {
                _logger.LogError("Storage service returned a successful status code but the response content was null when creating artwork.");
                return null;
            }

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when creating artwork.");
            return null;
        }
    }

    public async Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/artworks/{id}/active", new { IsActive = isActive }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update artwork active state via storage service. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        try
        {
            var updatedArtwork = await response.Content.ReadFromJsonAsync<ArtworkDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            if (updatedArtwork is null)
            {
                _logger.LogError("Storage service returned a successful status code but the response content was null when updating artwork active state.");
                return null;
            }

            return updatedArtwork;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when updating artwork active state.");
            return null;
        }
    }

    public async Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(CancellationToken cancellationToken)
    {
        return await GetArtworksAsync(null, null, cancellationToken);
    }

    public async Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/artworks/{id}?activeOnly={activeOnly.ToString().ToLowerInvariant()}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var artwork = await response.Content.ReadFromJsonAsync<ArtworkDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return artwork;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service.");
            return null;
        }
    }

    public async Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(string? creatorId, bool? isActive, CancellationToken cancellationToken)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            queryParams.Add($"creatorId={Uri.EscapeDataString(creatorId)}");
        }

        if (isActive.HasValue)
        {
            queryParams.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri = "api/artworks";
        if (queryParams.Count > 0)
        {
            requestUri += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var artworks = await response.Content.ReadFromJsonAsync<IEnumerable<ArtworkDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return artworks ?? null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service.");
            return null;
        }
    }
}
