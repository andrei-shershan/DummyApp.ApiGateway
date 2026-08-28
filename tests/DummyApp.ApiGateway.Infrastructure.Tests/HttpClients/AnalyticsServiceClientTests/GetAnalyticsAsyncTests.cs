using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.AnalyticsServiceClientTests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.AnalyticsServiceClientTests;

public sealed class GetAnalyticsAsyncTests : AnalyticsServiceClientTestsBase
{
    [Fact]
    public async Task GetAnalyticsAsync_ReturnsData_WhenResponseIsSuccessful()
    {
        var expected = new[]
        {
            new AnalyticsEventDto(
                Id: "analytics-1",
                OrderId: Guid.NewGuid(),
                Status: "Completed",
                Email: "test@example.com",
                SiteId: "site-1",
                Address: null,
                Items: Array.Empty<AnalyticsOrderItem>(),
                Tags: Array.Empty<string>(),
                EventTimestamp: DateTimeOffset.UtcNow)
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expected, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        };

        var client = CreateClient(response, secretKey: null);

        var result = await client.GetAnalyticsAsync(30, CancellationToken.None);

        var actual = Assert.Single(result);
        Assert.Equal(expected[0].Id, actual.Id);
        Assert.Equal(expected[0].OrderId, actual.OrderId);
        Assert.Equal(expected[0].Status, actual.Status);
        Assert.Equal(expected[0].Email, actual.Email);
        Assert.Equal(expected[0].SiteId, actual.SiteId);
        Assert.Equal(expected[0].Address, actual.Address);
        Assert.Equal(expected[0].EventTimestamp, actual.EventTimestamp);
        Assert.Equal(expected[0].Tags, actual.Tags);
        Assert.Equal(expected[0].Items, actual.Items);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ThrowsInvalidOperationException_WhenResponseIsNotSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { Message = "Bad request" })
        };

        var client = CreateClient(response, secretKey: null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAnalyticsAsync(30, CancellationToken.None));

        Assert.Contains("Analytics service returned status", exception.Message);
        VerifyLog(LogLevel.Error, "Failed to get analytics data. StatusCode:", Times.Once());
    }

    [Fact]
    public async Task GetAnalyticsAsync_ReturnsEmptyEnumerable_WhenResponseBodyIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create((IEnumerable<AnalyticsEventDto>?)null)
        };

        var client = CreateClient(response, secretKey: null);

        var result = await client.GetAnalyticsAsync(30, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
