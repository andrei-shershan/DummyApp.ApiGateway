using Azure.Identity;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.Services;
using DummyApp.ApiGateway.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Key Vault: add in stg/prod only; local dev uses appsettings.Development.json
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var credential = string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(clientId);

        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
    }
}

// Add services to the container.

var storageBaseUrl = builder.Configuration["Services:StorageService:BaseUrl"];
var blobServiceBaseUrl = builder.Configuration["Services:BlobService:BaseUrl"];

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IClientCredentialsTokenCache, ClientCredentialsTokenCache>();
builder.Services.AddMediatR(typeof(CreateArtworkCommand).Assembly);

builder.Services.AddHttpClient<IStorageServiceClient, StorageServiceClient>(client =>
{
    if (!string.IsNullOrEmpty(storageBaseUrl))
    {
        client.BaseAddress = new Uri(storageBaseUrl);
    }
})
.AddHttpMessageHandler(sp => new ClientCredentialsTokenHandler(
    sp.GetRequiredService<IClientCredentialsTokenCache>(),
    scope: "storage.write",
    cacheKey: "storage",
    sp.GetRequiredService<ILogger<ClientCredentialsTokenHandler>>()));

builder.Services.AddHttpClient<IBlobServiceClient, BlobServiceHttpClient>(client =>
{
    if (!string.IsNullOrEmpty(blobServiceBaseUrl))
    {
        client.BaseAddress = new Uri(blobServiceBaseUrl);
    }
});

// JWT validation: verify tokens issued by the Identity server.
// ApiGateway itself only validates the token (issued by Identity and forwarded by BFF).
// It does NOT participate in the OIDC login flow.
var identityServerSection = builder.Configuration.GetSection("IdentityServer");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityServerSection["Authority"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = identityServerSection["Authority"],
            ValidateAudience = false // OpenIddict does not set aud by default
        };
    });

builder.Services.AddHttpClient("storage", client =>
{
    if (!string.IsNullOrEmpty(storageBaseUrl))
    {
        client.BaseAddress = new Uri(storageBaseUrl);
    }
});

builder.Services.AddHttpClient("blobservice", client =>
{
    if (!string.IsNullOrEmpty(blobServiceBaseUrl))
    {
        client.BaseAddress = new Uri(blobServiceBaseUrl);
    }
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
if (builder.Configuration.GetValue<bool>("ReverseProxy:TrustAllProxies"))
{
    // Dev only: trust all proxies inside the Docker network (Traefik).
    // Do NOT enable in production.
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
}
app.UseForwardedHeaders(forwardedOptions);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
