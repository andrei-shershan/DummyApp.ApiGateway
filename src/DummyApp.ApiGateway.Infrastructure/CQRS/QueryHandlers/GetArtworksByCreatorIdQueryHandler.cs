using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworksByCreatorIdQueryHandler : IRequestHandler<GetArtworksByCreatorIdQuery, IEnumerable<ArtworkDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IStorageUrlService _storageUrlService;

    public GetArtworksByCreatorIdQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IStorageUrlService storageUrlService)
    {
        _storageServiceClient = storageServiceClient;
        _storageUrlService = storageUrlService;
    }

    public async Task<IEnumerable<ArtworkDto>> Handle(GetArtworksByCreatorIdQuery request, CancellationToken cancellationToken)
    {
        var artworks = await _storageServiceClient.GetArtworksByCreatorIdAsync(request.CreatorId, cancellationToken);

        return artworks?.Select(art =>
        {
            var imgUrl = _storageUrlService.GetBlobUrl(art.ImgUrl);
            var thumbnailUrl = _storageUrlService.GetBlobUrl(art.ThumbnailUrl);

            return art with
            {
                ImgUrl = imgUrl,
                ThumbnailUrl = thumbnailUrl
            };
        }) ?? Array.Empty<ArtworkDto>();
    }
}
