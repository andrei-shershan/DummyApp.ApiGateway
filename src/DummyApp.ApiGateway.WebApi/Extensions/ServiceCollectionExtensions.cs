using Azure.Identity;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Services;
using DummyApp.ApiGateway.WebApi.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiGatewaySettings>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ApiGatewaySettings>>().Value);

        return services;
    }

    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, ApiGatewaySettings settings)
    {
        ValidateBlobStorageSettings(settings.BlobStorage);

        services.AddSingleton(new BlobStorageSettings(
            settings.BlobStorage.StorageUrl!, 
            settings.BlobStorage.ContainerName!));

        services.AddScoped<IStorageUrlService, StorageUrlService>();
        services.AddSingleton(new ClientCredentialsTokenCacheOptions
        {
            Authority = settings.IdentityServer.Authority,
            ClientId = settings.IdentityServer.OidcClients.ApiGateway.ClientId,
            ClientSecret = settings.IdentityServer.OidcClients.ApiGateway.ClientSecret
        });
        services.AddSingleton<IClientCredentialsTokenCache, ClientCredentialsTokenCache>();

        services.AddApiGatewayHttpClients(settings);

        return services;
    }

    public static IServiceCollection AddApiGatewayHttpClients(this IServiceCollection services, ApiGatewaySettings settings)
    {
        var storageBaseUrl = settings.Services.StorageService.BaseUrl;
        var blobServiceBaseUrl = settings.Services.BlobService.BaseUrl;

        services.AddHttpClient<IStorageServiceHttpClient, StorageServiceClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(storageBaseUrl))
            {
                client.BaseAddress = new Uri(storageBaseUrl);
            }
        })
        .AddHttpMessageHandler(sp => new ClientCredentialsTokenHandler(
            sp.GetRequiredService<IClientCredentialsTokenCache>(),
            scope: "storage.write",
            cacheKey: "storage",
            sp.GetRequiredService<ILogger<ClientCredentialsTokenHandler>>()));

        services.AddHttpClient<IBlobServiceHttpClient, BlobServiceHttpClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(blobServiceBaseUrl))
            {
                client.BaseAddress = new Uri(blobServiceBaseUrl);
            }
        });

        services.AddHttpClient("storage", client =>
        {
            if (!string.IsNullOrWhiteSpace(storageBaseUrl))
            {
                client.BaseAddress = new Uri(storageBaseUrl);
            }
        });

        services.AddHttpClient("blobservice", client =>
        {
            if (!string.IsNullOrWhiteSpace(blobServiceBaseUrl))
            {
                client.BaseAddress = new Uri(blobServiceBaseUrl);
            }
        });

        return services;
    }

    public static IServiceCollection AddApiGatewayAuthentication(this IServiceCollection services, ApiGatewaySettings settings)
    {
        services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = settings.IdentityServer.Authority;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuer = settings.IdentityServer.Authority,
                    ValidateAudience = false // OpenIddict does not set aud by default
                };
            });

        return services;
    }

    public static WebApplicationBuilder AddApiGatewayKeyVault(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            return builder;
        }

        var keyVaultSection = builder.Configuration.GetSection(nameof(ApiGatewaySettings.KeyVault));
        var keyVaultUrl = keyVaultSection[nameof(KeyVaultOptions.Url)];
        if (string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            return builder;
        }

        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var credential = string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(clientId);

        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
        return builder;
    }

    public static IApplicationBuilder UseApiGatewayForwardedHeaders(this IApplicationBuilder app, ApiGatewaySettings settings)
    {
        var forwardedOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        if (settings.ReverseProxy.TrustAllProxies)
        {
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
        }

        app.UseForwardedHeaders(forwardedOptions);
        return app;
    }

    private static void ValidateBlobStorageSettings(BlobStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StorageUrl))
        {
            throw new InvalidOperationException(
                $"Blob storage URL is not configured. Set {nameof(ApiGatewaySettings.BlobStorage)}__{nameof(BlobStorageOptions.StorageUrl)}.");
        }

        if (string.IsNullOrWhiteSpace(options.ContainerName))
        {
            throw new InvalidOperationException(
                $"Blob storage container name is not configured. Set {nameof(ApiGatewaySettings.BlobStorage)}__{nameof(BlobStorageOptions.ContainerName)}.");
        }
    }
}
