using System.Net.Http;
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

    public async Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(string? creatorId, bool isActive, CancellationToken cancellationToken)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            queryParams.Add($"creatorId={Uri.EscapeDataString(creatorId)}");
        }

        queryParams.Add($"isActive={isActive.ToString().ToLowerInvariant()}");

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

    public async Task<IEnumerable<SeriesDto>?> GetSeriesAsync(string creatorId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/series?creatorId={Uri.EscapeDataString(creatorId)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var series = await response.Content.ReadFromJsonAsync<IEnumerable<SeriesDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return series;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service.");
            return null;
        }
    }

    public async Task<IEnumerable<PrintSizeDto>?> GetPrintSizesAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/printsizes", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var printSizes = await response.Content.ReadFromJsonAsync<IEnumerable<PrintSizeDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return printSizes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when getting print sizes.");
            return null;
        }
    }

    public async Task<SeriesDto?> CreateSeriesAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/series", new { Name = name }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create series via storage service. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        try
        {
            var series = await response.Content.ReadFromJsonAsync<SeriesDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return series;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when creating series.");
            return null;
        }
    }

    public async Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId, int? priceId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/orders/{orderId}/items", new { ArtworkId = artworkId, Quantity = quantity, PrintSizeId = printSizeId, PriceId = priceId }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to add order item via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId, int? priceId, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/orders/{orderId}/items/{artworkId}")
        {
            Content = JsonContent.Create(new { Quantity = quantity, PrintSizeId = printSizeId, PriceId = priceId })
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update order item via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<OrderItemDto>?> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}/items", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var items = await response.Content.ReadFromJsonAsync<IEnumerable<OrderItemDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when getting order items.");
            return null;
        }
    }

    public async Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var summary = await response.Content.ReadFromJsonAsync<OrderSummaryDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when getting order summary.");
            return null;
        }
    }

    public async Task<OrderAddressDto?> GetOrderAddressAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}/address", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var address = await response.Content.ReadFromJsonAsync<OrderAddressDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return address;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when getting order address.");
            return null;
        }
    }

    public async Task<bool> SaveOrderAddressAsync(Guid orderId, OrderAddressDto address, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/orders/{orderId}/address", address, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to save order address via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> CreateVerificationCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/verification", new { Email = email, Code = code, ExpiresAt = expiresAt }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create verification code via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> CreateCompletedOrdersTokenAsync(string email, Guid token, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/verification/completed-orders", new { Email = email, Token = token, ExpiresAt = expiresAt }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create completed orders token via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<bool> VerifyVerificationCodeAsync(string email, string code, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/verification/verify", new { Email = email, Code = code }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to verify verification code via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<OrderStatusDto?> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}/status", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            var status = await response.Content.ReadFromJsonAsync<OrderStatusDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from storage service when getting order status.");
            return null;
        }
    }

    public async Task<bool> SetOrderStatusAsync(Guid orderId, string status, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/orders/{orderId}/status", new { Status = status }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to change order status via storage service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }
}
