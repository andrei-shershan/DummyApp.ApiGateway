using System.Net.Http.Json;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class IdentityServiceClient : IIdentityServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityServiceClient> _logger;

    public IdentityServiceClient(
        HttpClient httpClient,
        ILogger<IdentityServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/admin/users", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve users from Identity service. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read users response content from Identity service.");
            return null;
        }
    }

    public async Task<IEnumerable<RoleDto>?> GetRolesAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/admin/roles", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve roles from Identity service. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<RoleDto>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read roles response content from Identity service.");
            return null;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/admin/users/{userId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve user {UserId} from Identity service. Status code: {StatusCode}", userId, response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<UserDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read user response content from Identity service for user {UserId}.", userId);
            return null;
        }
    }

    public async Task<bool> SaveInviteTokenAsync(string email, string token, CancellationToken cancellationToken)
    {
        var request = new { Email = email, Token = token };
        var response = await _httpClient.PostAsJsonAsync("api/admin/invite", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to save invite token in Identity service. Status code: {StatusCode}", response.StatusCode);
            return false;
        }

        return true;
    }

    public async Task<UserDto?> UpdateUserProfileAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken)
    {
        var request = new { FirstName = firstName, LastName = lastName };
        var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{userId}", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update profile for user {UserId} in Identity service. Status code: {StatusCode}", userId, response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<UserDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read update user profile response from Identity service for user {UserId}.", userId);
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserAvatarAsync(string userId, string avatarUrl, string avatarSmallUrl, CancellationToken cancellationToken)
    {
        var request = new { AvatarUrl = avatarUrl, AvatarSmallUrl = avatarSmallUrl };
        var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{userId}/avatar", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update avatar for user {UserId} in Identity service. Status code: {StatusCode}", userId, response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<UserDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read update user avatar response from Identity service for user {UserId}.", userId);
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserActiveStateAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var request = new { IsActive = isActive };
        var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{userId}/active", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to update active state for user {UserId} in Identity service. Status code: {StatusCode}", userId, response.StatusCode);
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<UserDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read update user active state response from Identity service for user {UserId}.", userId);
            return null;
        }
    }
}
