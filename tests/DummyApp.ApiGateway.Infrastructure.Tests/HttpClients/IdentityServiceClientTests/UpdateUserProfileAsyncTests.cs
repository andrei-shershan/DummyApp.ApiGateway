using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.IdentityServiceClientTests;

public class UpdateUserProfileAsyncTests : IdentityServiceClientTestsBase
{
    [Fact]
    public async Task UpdateUserProfileAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = CreateClient(httpClient);

        var result = await client.UpdateUserProfileAsync("user-id", "First", "Last", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to update profile for user user-id in Identity service. Status code:", Times.Once());
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ReturnsUser_WhenResponseIsSuccessfulAndContentIsValid()
    {
        HttpRequestMessage? capturedRequest = null;

        var expectedUser = new UserDto
        {
            Id = "user-id",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            Roles = new List<string> { "Creator" }
        };

        var content = JsonContent.Create(expectedUser, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = content },
            request =>
            {
                capturedRequest = request;
                return Task.CompletedTask;
            });

        var client = CreateClient(httpClient);

        var result = await client.UpdateUserProfileAsync("user-id", "First", "Last", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result!.Id);
        Assert.Equal(expectedUser.Email, result.Email);
        Assert.Equal(expectedUser.FirstName, result.FirstName);
        Assert.Equal(expectedUser.LastName, result.LastName);
        Assert.Equal(expectedUser.Roles, result.Roles);
        Assert.NotNull(capturedRequest);
        Assert.Equal("/api/admin/users/user-id", capturedRequest!.RequestUri?.PathAndQuery);
        Assert.Equal(HttpMethod.Put, capturedRequest.Method);

        var requestBody = await capturedRequest.Content!.ReadAsStringAsync(CancellationToken.None);
        var bodyJson = JsonDocument.Parse(requestBody);

        Assert.True(
            bodyJson.RootElement.TryGetProperty("FirstName", out var firstNameProperty)
            || bodyJson.RootElement.TryGetProperty("firstName", out firstNameProperty));
        Assert.Equal("First", firstNameProperty.GetString());

        Assert.True(
            bodyJson.RootElement.TryGetProperty("LastName", out var lastNameProperty)
            || bodyJson.RootElement.TryGetProperty("lastName", out lastNameProperty));
        Assert.Equal("Last", lastNameProperty.GetString());

        VerifyNoLogs();
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ReturnsNull_WhenResponseContentIsInvalid()
    {
        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", System.Text.Encoding.UTF8, "application/json")
            });

        var client = CreateClient(httpClient);

        var result = await client.UpdateUserProfileAsync("user-id", "First", "Last", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read update user profile response from Identity service for user user-id.", Times.Once());
    }
}
