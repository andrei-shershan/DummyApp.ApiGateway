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
}
