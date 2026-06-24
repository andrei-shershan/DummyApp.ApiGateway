using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IIdentityServiceHttpClient _identityServiceClient;

    public GetRolesQueryHandler(IIdentityServiceHttpClient identityServiceClient)
    {
        _identityServiceClient = identityServiceClient;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _identityServiceClient.GetRolesAsync(cancellationToken) ?? Array.Empty<RoleDto>();
    }
}
