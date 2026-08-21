using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public sealed class GetCompletedOrdersByTokenAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetCompletedOrdersByTokenAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        var token = Guid.NewGuid();
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.GetCompletedOrdersByTokenAsync(token, CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetCompletedOrdersByTokenAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var token = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        var client = CreateClient(response);

        var result = await client.GetCompletedOrdersByTokenAsync(token, CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when getting completed orders.", Times.Once());
    }

    [Fact]
    public async Task GetCompletedOrdersByTokenAsync_ReturnsCompletedOrders_WhenResponseIsSuccessful()
    {
        var token = Guid.NewGuid();
        var expectedOrders = new[]
        {
            new OrderSummaryDto { OrderId = Guid.NewGuid(), Status = "Completed", Email = "user1@example.com" },
            new OrderSummaryDto { OrderId = Guid.NewGuid(), Status = "Completed", Email = "user2@example.com" }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedOrders)
        };

        var client = CreateClient(response);

        var result = await client.GetCompletedOrdersByTokenAsync(token, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Collection(result!,
            order =>
            {
                Assert.Equal(expectedOrders[0].OrderId, order.OrderId);
                Assert.Equal(expectedOrders[0].Status, order.Status);
                Assert.Equal(expectedOrders[0].Email, order.Email);
            },
            order =>
            {
                Assert.Equal(expectedOrders[1].OrderId, order.OrderId);
                Assert.Equal(expectedOrders[1].Status, order.Status);
                Assert.Equal(expectedOrders[1].Email, order.Email);
            });
        VerifyNoLogs();
    }
}
