using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IStorageServiceClient
{
    Task<CreateArtworkCommandResult> CreateArtworkAsync(CreateArtworkCommand request, CancellationToken cancellationToken);
    Task<IEnumerable<ArtworkDto>> GetArtworksAsync(CancellationToken cancellationToken);
}
