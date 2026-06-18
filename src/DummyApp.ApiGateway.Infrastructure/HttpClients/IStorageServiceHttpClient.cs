using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IStorageServiceHttpClient
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto artwork, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>?> GetArtworksAsync(CancellationToken cancellationToken);
}
