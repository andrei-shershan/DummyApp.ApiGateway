using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IArtworkQueryFilterService
{
    ArtworkQueryFilter ApplyFilter(GetArtworksQuery request);
    bool GetArtworkByIdActiveOnly();
}
