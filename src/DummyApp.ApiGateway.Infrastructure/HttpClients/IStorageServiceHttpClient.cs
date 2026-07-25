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
}
