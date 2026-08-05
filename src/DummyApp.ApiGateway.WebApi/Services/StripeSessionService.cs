using Stripe;
using Stripe.Checkout;

namespace DummyApp.ApiGateway.WebApi.Services;

public interface IStripeSessionService
{
    Task<Session> CreateAsync(SessionCreateOptions options, CancellationToken cancellationToken = default);
}

public sealed class StripeSessionService : IStripeSessionService
{
    private readonly SessionService _sessionService;

    public StripeSessionService()
    {
        _sessionService = new SessionService();
    }

    public Task<Session> CreateAsync(SessionCreateOptions options, CancellationToken cancellationToken = default)
    {
        return _sessionService.CreateAsync(options, null, cancellationToken);
    }
}
