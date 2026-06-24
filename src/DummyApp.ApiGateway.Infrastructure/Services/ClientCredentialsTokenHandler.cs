using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class ClientCredentialsTokenHandler : DelegatingHandler
{
    private readonly IClientCredentialsTokenCache _tokenCache;
    private readonly string _scope;
    private readonly string _cacheKey;
    private readonly ILogger<ClientCredentialsTokenHandler> _logger;

    public ClientCredentialsTokenHandler(
        IClientCredentialsTokenCache tokenCache,
        string scope,
        string cacheKey,
        ILogger<ClientCredentialsTokenHandler> logger)
    {
        _tokenCache = tokenCache;
        _scope = scope;
        _cacheKey = cacheKey;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenCache.GetTokenAsync(_scope, _cacheKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Unable to acquire client credentials token.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("Token expired or invalid for cache '{CacheKey}' and scope '{Scope}'; refreshing and retrying.", _cacheKey, _scope);
            _tokenCache.Invalidate(_cacheKey, _scope);
            accessToken = await _tokenCache.GetTokenAsync(_scope, _cacheKey, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Unable to reacquire client credentials token.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
