using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IStorageServiceHttpClient
{
    Task<CreateArtworkCommandResult> CreateArtworkAsync(CreateArtworkCommand request, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>> GetArtworksAsync(CancellationToken cancellationToken);
}
