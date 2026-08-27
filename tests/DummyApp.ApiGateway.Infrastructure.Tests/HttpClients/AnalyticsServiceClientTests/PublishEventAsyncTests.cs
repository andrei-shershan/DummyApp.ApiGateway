using System.Net;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.AnalyticsServiceClientTests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.AnalyticsServiceClientTests;

public sealed class PublishEventAsyncTests : AnalyticsServiceClientTestsBase
{
    [Fact]
    public async Task PublishEventAsync_SendsRequestToCorrectUri_WhenSecretKeyIsNull()
    {
        // Arrange
        string? capturedBody = null;
        string? capturedContentType = null;
        string? capturedPathAndQuery = null;
        var expectedRequest = new AnalyticsEventRequest
        {
            OrderId = Guid.NewGuid(),
            Status = "Completed",
            Email = "test@example.com",
            SiteId = "site-1",
            EventTimestamp = DateTimeOffset.UtcNow
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK), secretKey: null, onRequest: async request =>
        {
            capturedPathAndQuery = request.RequestUri?.PathAndQuery;
            capturedContentType = request.Content?.Headers.ContentType?.ToString();
            capturedBody = await request.Content!.ReadAsStringAsync();
        });

        // Act
        await client.PublishEventAsync(expectedRequest, CancellationToken.None);

        // Assert
        Assert.Equal("/api/analytics/event", capturedPathAndQuery);
        Assert.Equal("application/json; charset=utf-8", capturedContentType);
        Assert.NotNull(capturedBody);

        var actual = JsonSerializer.Deserialize<AnalyticsEventRequest>(capturedBody);
        Assert.NotNull(actual);
        Assert.Equal(expectedRequest.OrderId, actual!.OrderId);
        Assert.Equal(expectedRequest.Status, actual.Status);
        Assert.Equal(expectedRequest.Email, actual.Email);
    }

    [Fact]
    public async Task PublishEventAsync_SendsRequestToCorrectUri_WhenSecretKeyIsProvided()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var expectedKey = "abc-123";
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK), secretKey: expectedKey, onRequest: request =>
        {
            capturedRequest = request;
            return Task.CompletedTask;
        });

        var requestBody = new AnalyticsEventRequest
        {
            OrderId = Guid.NewGuid(),
            Status = "Completed",
            Email = "test@example.com",
            SiteId = "site-1",
            EventTimestamp = DateTimeOffset.UtcNow
        };

        // Act
        await client.PublishEventAsync(requestBody, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal($"/api/analytics/event?code={Uri.EscapeDataString(expectedKey)}", capturedRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task PublishEventAsync_ThrowsInvalidOperationException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var request = new AnalyticsEventRequest
        {
            OrderId = Guid.NewGuid(),
            Status = "Completed",
            Email = "test@example.com",
            SiteId = "site-1",
            EventTimestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.PublishEventAsync(request, CancellationToken.None));

        Assert.Contains("Analytics service returned status", exception.Message);
        VerifyLog(LogLevel.Error, "Failed to publish analytics event. StatusCode:", Times.Once());
    }
}
