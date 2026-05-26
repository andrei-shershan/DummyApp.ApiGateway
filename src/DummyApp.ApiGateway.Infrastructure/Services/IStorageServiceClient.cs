using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IStorageServiceClient
{
    Task<CreateArtworkCommandResult> CreateArtworkAsync(CreateArtworkCommand request, CancellationToken cancellationToken);
}
