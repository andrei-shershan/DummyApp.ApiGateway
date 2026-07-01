using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IStorageServiceHttpClient
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto artwork, CancellationToken cancellationToken);
    Task<ArtworkDto?> GetArtworkByIdAsync(int id, bool activeOnly, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(string? creatorId, bool? isActive, CancellationToken cancellationToken);
}
