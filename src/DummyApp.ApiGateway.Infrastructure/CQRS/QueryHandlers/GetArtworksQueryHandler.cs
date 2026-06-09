using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworksQueryHandler : IRequestHandler<GetArtworksQuery, IEnumerable<ArtworkDto>>
{
    private readonly IStorageServiceClient _storageServiceClient;
    private readonly IStorageUrlService _storageUrlService;
    private readonly ILogger<GetArtworksQueryHandler> _logger;

    public GetArtworksQueryHandler(
        IStorageServiceClient storageServiceClient,
        IStorageUrlService storageUrlService,
        ILogger<GetArtworksQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _storageUrlService = storageUrlService;
        _logger = logger;
    }

    public async Task<IEnumerable<ArtworkDto>> Handle(GetArtworksQuery request, CancellationToken cancellationToken)
    {
        var artworks = (await _storageServiceClient.GetArtworksAsync(cancellationToken)).ToList();
        _logger.LogInformation("Fetched {Count} artworks from storage service.", artworks.Count);

        var result = artworks.Select(art =>
        {
            var imgUrl = _storageUrlService.GetBlobUrl(art.ImgUrl);
            var smallImgUrl = _storageUrlService.GetBlobUrl(art.SmallImgUrl);

            _logger.LogDebug("Artwork {ArtworkId}: ImgUrl={OrigImgUrl} -> {ResolvedImgUrl}, SmallImgUrl={OrigSmallImgUrl} -> {ResolvedSmallImgUrl}",
                art.Id,
                art.ImgUrl,
                imgUrl,
                art.SmallImgUrl,
                smallImgUrl);

            return art with
            {
                ImgUrl = imgUrl,
                SmallImgUrl = smallImgUrl
            };
        }).ToList();

        return result;
    }
}
