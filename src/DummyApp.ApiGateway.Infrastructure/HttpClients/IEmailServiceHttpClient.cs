namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IEmailServiceHttpClient
{
    Task<bool> SendInviteAsync(string email, string token, CancellationToken cancellationToken);
}
