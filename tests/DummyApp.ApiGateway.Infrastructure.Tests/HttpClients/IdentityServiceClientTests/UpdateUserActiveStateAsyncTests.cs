using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.IdentityServiceClientTests;

public class UpdateUserActiveStateAsyncTests : IdentityServiceClientTestsBase
{
    [Fact]
    public async Task UpdateUserActiveStateAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = CreateClient(httpClient);

        var result = await client.UpdateUserActiveStateAsync("user-id", false, CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to update active state for user user-id", Times.Once());
    }

    [Fact]
    public async Task UpdateUserActiveStateAsync_ReturnsUser_WhenResponseIsSuccessfulAndContentIsValid()
    {
        HttpRequestMessage? capturedRequest = null;

        var expectedUser = new UserDto
        {
            Id = "user-id",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            IsActive = false,
            Roles = new[] { "User" }
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

        var result = await client.UpdateUserActiveStateAsync("user-id", false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result!.Id);
        Assert.Equal(expectedUser.Email, result.Email);
        Assert.Equal(expectedUser.FirstName, result.FirstName);
        Assert.Equal(expectedUser.LastName, result.LastName);
        Assert.Equal(expectedUser.IsActive, result.IsActive);
        Assert.Equal(expectedUser.Roles, result.Roles);
        Assert.NotNull(capturedRequest);
        Assert.Equal("/api/admin/users/user-id/active", capturedRequest!.RequestUri?.PathAndQuery);
        Assert.Equal(HttpMethod.Put, capturedRequest.Method);

        var requestBody = await capturedRequest.Content!.ReadAsStringAsync(CancellationToken.None);
        var bodyJson = JsonDocument.Parse(requestBody);
        Assert.True(bodyJson.RootElement.TryGetProperty("IsActive", out var isActiveProperty) || bodyJson.RootElement.TryGetProperty("isActive", out isActiveProperty));
        Assert.False(isActiveProperty.GetBoolean());

        VerifyNoLogs();
    }

    [Fact]
    public async Task UpdateUserActiveStateAsync_ReturnsNull_WhenResponseContentIsInvalid()
    {
        using var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", System.Text.Encoding.UTF8, "application/json")
            });

        var client = CreateClient(httpClient);

        var result = await client.UpdateUserActiveStateAsync("user-id", true, CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read update user active state response from Identity service for user user-id.", Times.Once());
    }
}
