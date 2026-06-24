using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IIdentityServiceHttpClient _identityServiceClient;

    public GetUsersQueryHandler(IIdentityServiceHttpClient identityServiceClient)
    {
        _identityServiceClient = identityServiceClient;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _identityServiceClient.GetUsersAsync(cancellationToken) ?? Array.Empty<UserDto>();
    }
}
