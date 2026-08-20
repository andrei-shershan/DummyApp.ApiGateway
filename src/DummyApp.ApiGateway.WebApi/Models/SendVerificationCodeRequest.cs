namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class SendVerificationCodeRequest
{
    public string Email { get; init; } = string.Empty;
}
