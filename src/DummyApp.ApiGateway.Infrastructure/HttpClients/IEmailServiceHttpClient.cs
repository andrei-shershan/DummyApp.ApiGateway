namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IEmailServiceHttpClient
{
    Task<bool> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken);
}
