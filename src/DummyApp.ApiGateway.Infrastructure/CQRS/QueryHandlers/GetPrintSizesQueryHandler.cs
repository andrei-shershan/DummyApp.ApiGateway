using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetPrintSizesQueryHandler : IRequestHandler<GetPrintSizesQuery, IEnumerable<PrintSizeDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;

    public GetPrintSizesQueryHandler(IStorageServiceHttpClient storageServiceClient)
    {
        _storageServiceClient = storageServiceClient;
    }

    public async Task<IEnumerable<PrintSizeDto>> Handle(GetPrintSizesQuery request, CancellationToken cancellationToken)
    {
        return await _storageServiceClient.GetPrintSizesAsync(cancellationToken) ?? Array.Empty<PrintSizeDto>();
    }
}
