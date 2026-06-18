using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworksQueryHandler : IRequestHandler<GetArtworksQuery, IEnumerable<ArtworkDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IStorageUrlService _storageUrlService;

    public GetArtworksQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IStorageUrlService storageUrlService)
    {
        _storageServiceClient = storageServiceClient;
        _storageUrlService = storageUrlService;
    }

    public async Task<IEnumerable<ArtworkDto>> Handle(GetArtworksQuery request, CancellationToken cancellationToken)
    {
        var artworks = await _storageServiceClient.GetArtworksAsync(cancellationToken);

        return artworks?.Select(art =>
        {
            var imgUrl = _storageUrlService.GetBlobUrl(art.ImgUrl);
            var smallImgUrl = _storageUrlService.GetBlobUrl(art.SmallImgUrl);

            return art with
            {
                ImgUrl = imgUrl,
                SmallImgUrl = smallImgUrl
            };
        }) ?? [];
    }
}
