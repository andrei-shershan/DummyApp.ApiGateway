namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class SendEmailAttachment
{
    public string Name { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string Base64Content { get; init; } = string.Empty;
}
