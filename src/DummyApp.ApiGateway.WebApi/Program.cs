using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.WebApi.Configuration;
using DummyApp.ApiGateway.WebApi.Extensions;
using MediatR;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiGatewayKeyVault();

builder.Services.AddApiGatewayConfiguration(builder.Configuration);
var apiGatewaySettings = builder.Configuration.Get<ApiGatewaySettings>() ?? new ApiGatewaySettings();
if (!string.IsNullOrWhiteSpace(apiGatewaySettings.Stripe.SecretKey))
{
    StripeConfiguration.ApiKey = apiGatewaySettings.Stripe.SecretKey;
}

builder.Services.AddApiGatewayServices(apiGatewaySettings);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddMediatR(typeof(CreateArtworkCommand).Assembly);

builder.Services.AddApiGatewayAuthentication(apiGatewaySettings);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiGatewayForwardedHeaders(apiGatewaySettings);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
