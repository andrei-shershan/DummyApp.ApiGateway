using System.Threading;
using System.Threading.Tasks;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IClientCredentialsTokenCache
{
    Task<string?> GetTokenAsync(string scope, string cacheKey, CancellationToken ct = default);
    void Invalidate(string cacheKey);
}
