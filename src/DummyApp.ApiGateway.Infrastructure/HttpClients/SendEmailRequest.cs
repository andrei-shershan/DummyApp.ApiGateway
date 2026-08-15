using System.Text.Json;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed class SendEmailRequest
{
    public string Subject { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Recipients { get; init; } = Array.Empty<string>();
    public string Template { get; init; } = string.Empty;
    public JsonElement? Parameters { get; init; }
    public IReadOnlyCollection<SendEmailAttachment> Attachments { get; init; } = Array.Empty<SendEmailAttachment>();
}
