using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DummyApp.ApiGateway.WebApi.Services;

public interface IClientCredentialsTokenCache
{
    Task<string?> GetTokenAsync(string scope, CancellationToken ct = default);
    void Invalidate(string scope);
}

public sealed class ClientCredentialsTokenCache : IClientCredentialsTokenCache
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClientCredentialsTokenCache> _logger;

    // Buffer: refresh the token this many seconds before it actually expires
    // to avoid sending an already-expired token to StorageService.
    private const int ExpiryBufferSeconds = 60;

    public ClientCredentialsTokenCache(
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ClientCredentialsTokenCache> logger)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync(string scope, CancellationToken ct = default)
    {
        var cacheKey = $"cc_token:{scope}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
        {
            return cached;
        }

        var (token, expiresIn) = await FetchTokenAsync(scope, ct);
        if (token is null)
        {
            return null;
        }

        var ttl = TimeSpan.FromSeconds(Math.Max(expiresIn - ExpiryBufferSeconds, 30));
        _cache.Set(cacheKey, token, ttl);

        _logger.LogInformation(
            "Acquired new client-credentials token for scope '{Scope}', cached for {Ttl}",
            scope, ttl);

        return token;
    }

    public void Invalidate(string scope)
    {
        _cache.Remove($"cc_token:{scope}");
    }

    private async Task<(string? Token, int ExpiresIn)> FetchTokenAsync(string scope, CancellationToken ct)
    {
        var identity = _configuration.GetSection("Identity");
        var authority = identity["Authority"]?.TrimEnd('/');
        var tokenEndpoint = $"{authority}/connect/token";

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = identity["ClientId"] ?? string.Empty,
                ["client_secret"] = identity["ClientSecret"] ?? string.Empty,
                ["scope"] = scope
            })
        };

        var client = _httpClientFactory.CreateClient();
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach identity token endpoint {Endpoint}", tokenEndpoint);
            return (null, 0);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Identity returned {Status} for client-credentials request", response.StatusCode);
            return (null, 0);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        var token = root.TryGetProperty("access_token", out var t) ? t.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

        return (token, expiresIn);
    }
}
