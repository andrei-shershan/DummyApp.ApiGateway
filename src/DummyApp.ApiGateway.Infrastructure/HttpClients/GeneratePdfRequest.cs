using System.Text.Json;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class GeneratePdfRequest
{
    public string Template { get; init; } = string.Empty;
    public JsonElement? Parameters { get; init; }
}
