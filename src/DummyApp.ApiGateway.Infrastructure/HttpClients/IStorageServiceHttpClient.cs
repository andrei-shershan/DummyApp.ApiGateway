using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IStorageServiceHttpClient
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto artwork, CancellationToken cancellationToken);
    Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(string? creatorId, bool isActive, CancellationToken cancellationToken);
    Task<IEnumerable<SeriesDto>?> GetSeriesAsync(string creatorId, CancellationToken cancellationToken);
    Task<SeriesDto?> CreateSeriesAsync(string name, CancellationToken cancellationToken);
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, CancellationToken cancellationToken);
    Task<IEnumerable<OrderItemDto>?> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderStatusDto?> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> PayOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
