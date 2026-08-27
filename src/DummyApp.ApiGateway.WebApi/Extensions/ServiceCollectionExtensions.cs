using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Services;
using DummyApp.ApiGateway.WebApi.Configuration;
using DummyApp.ApiGateway.WebApi.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using InfrastructureApplicationOptions = DummyApp.ApiGateway.Infrastructure.Models.ApplicationOptions;
using BlobStorageOptionsModel = DummyApp.ApiGateway.Infrastructure.Models.BlobStorageOptions;
using ServiceBusConfigurationOptions = DummyApp.ApiGateway.Infrastructure.Models.ServiceBusOptions;

namespace DummyApp.ApiGateway.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiGatewaySettings>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        services.AddOptions<BlobStorageOptionsModel>()
            .Bind(configuration.GetSection(nameof(ApiGatewaySettings.BlobStorage)));

        services.AddOptions<EmailServiceOptions>()
            .Bind(configuration.GetSection(nameof(ApiGatewaySettings.EmailService)));

        services.AddOptions<FileServiceOptions>()
            .Bind(configuration.GetSection("FileService"));

        services.AddOptions<AnalyticsServiceOptions>()
            .Bind(configuration.GetSection(nameof(ApiGatewaySettings.AnalyticsService)));

        services.AddOptions<InfrastructureApplicationOptions>()
            .Bind(configuration.GetSection(InfrastructureApplicationOptions.SectionName));

        services.AddOptions<OrderQRCodeOptions>()
            .Bind(configuration);

        services.AddOptions<ServiceBusConfigurationOptions>()
            .Bind(configuration.GetSection(nameof(ApiGatewaySettings.ServiceBus)));

        services.AddOptions<InviteOptions>();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ApiGatewaySettings>>().Value);

        return services;
    }

    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services, ApiGatewaySettings settings)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IArtworkQueryFilterService, ArtworkQueryFilterService>();
        services.AddScoped<ITagFilterService, TagFilterService>();
        services.AddScoped<IStripeSessionService, StripeSessionService>();
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
            scope: "storage.scope",
            cacheKey: "storage",
            sp.GetRequiredService<ILogger<ClientCredentialsTokenHandler>>()));

        services.AddHttpClient<IBlobServiceHttpClient, BlobServiceHttpClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(blobServiceBaseUrl))
            {
                client.BaseAddress = new Uri(blobServiceBaseUrl);
            }
        });

        var identityBaseUrl = settings.Services.IdentityService.BaseUrl;
        services.AddHttpClient<IIdentityServiceHttpClient, IdentityServiceClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(identityBaseUrl))
            {
                client.BaseAddress = new Uri(identityBaseUrl);
            }
        })
        .AddHttpMessageHandler(sp => new ClientCredentialsTokenHandler(
            sp.GetRequiredService<IClientCredentialsTokenCache>(),
            scope: "identity.admin",
            cacheKey: "identity",
            sp.GetRequiredService<ILogger<ClientCredentialsTokenHandler>>()));

        var emailServiceBaseUrl = settings.Services.EmailService.BaseUrl;
        services.AddHttpClient<IEmailServiceHttpClient, EmailServiceClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(emailServiceBaseUrl))
            {
                client.BaseAddress = new Uri(emailServiceBaseUrl);
            }
        });

        var fileServiceBaseUrl = settings.Services.FileService.BaseUrl;
        services.AddHttpClient<IFileServiceHttpClient, FileServiceClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(fileServiceBaseUrl))
            {
                client.BaseAddress = new Uri(fileServiceBaseUrl);
            }
        });

        var analyticsServiceBaseUrl = settings.Services.AnalyticsService.BaseUrl;
        services.AddHttpClient<IAnalyticsServiceHttpClient, AnalyticsServiceClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(analyticsServiceBaseUrl))
            {
                client.BaseAddress = new Uri(analyticsServiceBaseUrl);
            }
        });

        if (!string.IsNullOrWhiteSpace(settings.ServiceBus.ConnectionString))
        {
            services.AddSingleton(new ServiceBusClient(settings.ServiceBus.ConnectionString));
            services.AddHostedService<CompletedOrderEventsBackgroundService>();
        }

        services.Configure<InviteOptions>(options =>
        {
            options.IdentityServiceBaseUrl = settings.Services.IdentityService.BaseUrl;
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
            forwardedOptions.KnownIPNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
        }

        app.UseForwardedHeaders(forwardedOptions);
        return app;
    }

}
