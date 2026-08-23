using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IStorageServiceHttpClient
{
    Task<ArtworkDto?> CreateArtworkAsync(CreateArtworkRequestDto artwork, CancellationToken cancellationToken);
    Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(string? creatorId, bool isActive, CancellationToken cancellationToken);
    Task<PaginatedResult<ArtworkDto>?> GetArtworksPageAsync(string? creatorId, bool isActive, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<PrintSizeDto>?> GetPrintSizesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TagDto>?> GetTagsAsync(CancellationToken cancellationToken);
    Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderItemDto>?> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IEnumerable<OrderSummaryDto>?> GetCompletedOrdersByTokenAsync(Guid token, CancellationToken cancellationToken);
    Task<OrderAddressDto?> GetOrderAddressAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> SaveOrderAddressAsync(Guid orderId, OrderAddressDto address, CancellationToken cancellationToken);
    Task<bool> CreateVerificationCodeAsync(string email, string code, DateTime expiresAt, CancellationToken cancellationToken);
    Task<bool> CreateCompletedOrdersTokenAsync(string email, Guid token, DateTime expiresAt, CancellationToken cancellationToken);
    Task<bool> VerifyVerificationCodeAsync(string email, string code, CancellationToken cancellationToken);
    Task<OrderStatusDto?> GetOrderStatusAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool> SetOrderStatusAsync(Guid orderId, string status, CancellationToken cancellationToken);
}
