using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IIdentityServiceHttpClient _identityServiceClient;

    public GetUserProfileQueryHandler(IIdentityServiceHttpClient identityServiceClient)
    {
        _identityServiceClient = identityServiceClient;
    }

    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return null;
        }

        return await _identityServiceClient.GetUserByIdAsync(request.UserId, cancellationToken);
    }
}
