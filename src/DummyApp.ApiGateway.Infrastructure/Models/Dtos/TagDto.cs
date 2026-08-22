using System;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
