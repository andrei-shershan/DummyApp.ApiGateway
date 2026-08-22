using System;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record NewTagRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
