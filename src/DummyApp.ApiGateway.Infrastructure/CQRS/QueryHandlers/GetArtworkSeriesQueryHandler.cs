using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworkSeriesQueryHandler : IRequestHandler<GetArtworkSeriesQuery, IEnumerable<SeriesDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;

    public GetArtworkSeriesQueryHandler(IStorageServiceHttpClient storageServiceClient)
    {
        _storageServiceClient = storageServiceClient;
    }

    public async Task<IEnumerable<SeriesDto>> Handle(GetArtworkSeriesQuery request, CancellationToken cancellationToken)
    {
        var series = await _storageServiceClient.GetSeriesAsync(request.CreatorId, cancellationToken);
        return series ?? Array.Empty<SeriesDto>();
    }
}
