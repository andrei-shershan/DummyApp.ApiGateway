using System.Net;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.EmailServiceClientTests;

public class SendEmailAsyncTests : EmailServiceClientTestsBase
{
    [Fact]
    public async Task SendEmailAsync_ReturnsFalse_WhenResponseIsNotSuccessful()
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

        var emailRequest = new SendEmailRequest
        {
            Subject = "Invitation",
            Recipients = new[] { "user@example.com" },
            Template = "Invite",
            Parameters = JsonSerializer.SerializeToElement(new { token = "token" })
        };

        var result = await client.SendEmailAsync(emailRequest, CancellationToken.None);

        Assert.False(result);
        Assert.Equal("/api/email/send?code=test-secret", capturedUri);
        Assert.Equal("application/json; charset=utf-8", capturedContentType);

        var payload = JsonSerializer.Deserialize<SendEmailRequest>(capturedBody!);

        Assert.NotNull(payload);
        Assert.Equal("Invitation", payload!.Subject);
        Assert.Single(payload.Recipients, "user@example.com");
        Assert.Equal("Invite", payload.Template);
        Assert.True(payload.Parameters.HasValue);
        Assert.Equal("token", payload.Parameters.Value.GetProperty("token").GetString());

        VerifyLog(LogLevel.Error, "Failed to send email to", Times.Once());
    }

    [Fact]
    public async Task SendEmailAsync_ReturnsTrue_WhenResponseIsSuccessful()
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

        var emailRequest = new SendEmailRequest
        {
            Subject = "Invitation",
            Recipients = new[] { "user@example.com" },
            Template = "Invite",
            Parameters = JsonSerializer.SerializeToElement(new { token = "token" })
        };

        var result = await client.SendEmailAsync(emailRequest, CancellationToken.None);

        Assert.True(result);
        Assert.NotNull(capturedRequest);
        Assert.Equal("/api/email/send?code=test-secret", capturedRequest.RequestUri?.PathAndQuery);
        VerifyLog(LogLevel.Information, "Successfully sent email to", Times.Once());
    }

    [Fact]
    public async Task SendEmailAsync_UsesPathWithoutCode_WhenSecretKeyIsMissing()
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

        var emailRequest = new SendEmailRequest
        {
            Subject = "Invitation",
            Recipients = new[] { "user@example.com" },
            Template = "Invite",
            Parameters = JsonSerializer.SerializeToElement(new { token = "token" })
        };

        var result = await client.SendEmailAsync(emailRequest, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("/api/email/send", capturedRequest?.RequestUri?.PathAndQuery);
        VerifyLog(LogLevel.Information, "Successfully sent email to", Times.Once());
    }
}
