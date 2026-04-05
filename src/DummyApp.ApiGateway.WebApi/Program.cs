var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var storageConfig = builder.Configuration.GetSection("StorageService");
var storageBaseUrl = storageConfig["BaseUrl"];
var ignoreSslErrors = storageConfig.GetValue<bool>("IgnoreSslErrors");

builder.Services.AddControllers();

// JWT validation: verify tokens issued by the Identity server.
// ApiGateway itself only validates the token (issued by Identity and forwarded by BFF).
// It does NOT participate in the OIDC login flow.
var identityJwtSection = builder.Configuration.GetSection("IdentityJwt");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityJwtSection["Authority"];
        options.MetadataAddress = identityJwtSection["MetadataAddress"];
        options.RequireHttpsMetadata = false; // HTTP in Docker dev
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = identityJwtSection["Authority"],
            ValidateAudience = false // OpenIddict does not set aud by default
        };
    });

builder.Services.AddHttpClient("storage", client =>
{
    if (!string.IsNullOrEmpty(storageBaseUrl))
    {
        client.BaseAddress = new Uri(storageBaseUrl);
    }
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    if (ignoreSslErrors)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    return new HttpClientHandler();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
