namespace DummyApp.ApiGateway.WebApi.Configuration;

public sealed record ApiGatewaySettings
{
    public string? TestMessage { get; init; }
    public KeyVaultOptions KeyVault { get; init; } = new();
    public IdentityServerOptions IdentityServer { get; init; } = new();
    public ServicesOptions Services { get; init; } = new();
    public BlobStorageOptions BlobStorage { get; init; } = new();
    public ReverseProxyOptions ReverseProxy { get; init; } = new();
}

public sealed record KeyVaultOptions
{
    public string? Url { get; init; }
}

public sealed record IdentityServerOptions
{
    public string? Authority { get; init; }
    public OidcClientsOptions OidcClients { get; init; } = new();
}

public sealed record OidcClientsOptions
{
    public OidcClientOptions ApiGateway { get; init; } = new();
}

public sealed record OidcClientOptions
{
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}

public sealed record ServicesOptions
{
    public ServiceEndpointOptions StorageService { get; init; } = new();
    public ServiceEndpointOptions BlobService { get; init; } = new();
}

public sealed record ServiceEndpointOptions
{
    public string? BaseUrl { get; init; }
}

public sealed record BlobStorageOptions
{
    public string? StorageUrl { get; init; }
    public string? ContainerName { get; init; }
}

public sealed record ReverseProxyOptions
{
    public bool TrustAllProxies { get; init; }
}
