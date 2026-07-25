using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IArtworkQueryFilterService
{
    bool ShouldRequestActiveOnly(bool requestedActiveOnly);
    bool CanAccessArtworkById(ArtworkDto artwork);
    bool AdminOrCreatorsArtwork(ArtworkDto artwork);
}
