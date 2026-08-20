namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class VerifyVerificationCodeRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
