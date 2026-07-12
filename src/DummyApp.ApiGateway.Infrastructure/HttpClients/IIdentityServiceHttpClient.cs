using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IIdentityServiceHttpClient
{
    Task<IEnumerable<UserDto>?> GetUsersAsync(CancellationToken cancellationToken);
    Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken);
    Task<bool> SaveInviteTokenAsync(string email, string token, CancellationToken cancellationToken);
    Task<UserDto?> UpdateUserActiveStateAsync(string userId, bool isActive, CancellationToken cancellationToken);
}
