namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string? SiteId { get; init; }
}
