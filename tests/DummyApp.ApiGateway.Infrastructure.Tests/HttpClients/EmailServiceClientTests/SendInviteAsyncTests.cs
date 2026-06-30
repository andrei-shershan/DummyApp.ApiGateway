using System.Net;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.EmailServiceClientTests;

public class SendInviteAsyncTests : EmailServiceClientTestsBase
{
    [Fact]
    public async Task SendInviteAsync_ReturnsFalse_WhenResponseIsNotSuccessful()
    {
        string? capturedUri = null;
        string? capturedBody = null;
        string? capturedContentType = null;

        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad request", Encoding.UTF8, "text/plain")
            },
            async request =>
            {
                capturedUri = request.RequestUri?.PathAndQuery;
                capturedContentType = request.Content?.Headers.ContentType?.ToString();
                capturedBody = await request.Content?.ReadAsStringAsync(CancellationToken.None);
            });

        var client = CreateClient(httpClient, "test-secret");

        var result = await client.SendInviteAsync("user@example.com", "token", CancellationToken.None);

        Assert.False(result);
        Assert.Equal("/api/email/invite?code=test-secret", capturedUri);
        Assert.Equal("application/json; charset=utf-8", capturedContentType);

        var payload = JsonSerializer.Deserialize<EmailServiceClient.InviteEmailRequest>(capturedBody!);

        Assert.NotNull(payload);
        Assert.Equal("user@example.com", payload!.Email);
        Assert.Equal("token", payload.Token);

        VerifyLog(LogLevel.Error, "Failed to send invite email to", Times.Once());
    }

    [Fact]
    public async Task SendInviteAsync_ReturnsTrue_WhenResponseIsSuccessful()
    {
        HttpRequestMessage? capturedRequest = null;

        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK),
            request =>
            {
                capturedRequest = request;
                return Task.CompletedTask;
            });

        var client = CreateClient(httpClient, "test-secret");

        var result = await client.SendInviteAsync("user@example.com", "token", CancellationToken.None);

        Assert.True(result);
        Assert.NotNull(capturedRequest);
        Assert.Equal("/api/email/invite?code=test-secret", capturedRequest.RequestUri?.PathAndQuery);
        VerifyNoLogs();
    }

    [Fact]
    public async Task SendInviteAsync_UsesPathWithoutCode_WhenSecretKeyIsMissing()
    {
        HttpRequestMessage? capturedRequest = null;

        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK),
            request =>
            {
                capturedRequest = request;
                return Task.CompletedTask;
            });

        var client = CreateClient(httpClient, null);

        var result = await client.SendInviteAsync("user@example.com", "token", CancellationToken.None);

        Assert.True(result);
        Assert.Equal("/api/email/invite", capturedRequest?.RequestUri?.PathAndQuery);
        VerifyNoLogs();
    }
}
