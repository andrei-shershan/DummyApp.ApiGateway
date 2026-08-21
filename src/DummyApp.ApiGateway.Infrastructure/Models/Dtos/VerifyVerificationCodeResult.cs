using System;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record VerifyVerificationCodeResult
{
    public bool Success { get; init; }
    public Guid Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsServerError { get; init; }
    public string? ErrorMessage { get; init; }
}
