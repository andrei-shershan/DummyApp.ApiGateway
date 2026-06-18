using Microsoft.Extensions.Logging;
using Moq;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers;

internal static class LoggerMockExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string expectedMessage, Times times)
    {
        logger.Verify(x => x.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
            times);
    }
}
