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
    Task<IEnumerable<PrintSizeDto>?> GetPrintSizesAsync(CancellationToken cancellationToken);
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderItemDto>?> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderAddressDto?> GetOrderAddressAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> SaveOrderAddressAsync(Guid orderId, OrderAddressDto address, CancellationToken cancellationToken);
    Task<OrderStatusDto?> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> SetOrderStatusAsync(Guid orderId, string status, CancellationToken cancellationToken);
}
