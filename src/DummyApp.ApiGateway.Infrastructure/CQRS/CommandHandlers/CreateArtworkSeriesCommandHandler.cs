using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class CreateArtworkSeriesCommandHandler : IRequestHandler<CreateArtworkSeriesCommand, SeriesDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;

    public CreateArtworkSeriesCommandHandler(IStorageServiceHttpClient storageServiceClient)
    {
        _storageServiceClient = storageServiceClient;
    }

    public async Task<SeriesDto?> Handle(CreateArtworkSeriesCommand request, CancellationToken cancellationToken)
    {
        return await _storageServiceClient.CreateSeriesAsync(request.Name, cancellationToken);
    }
}
