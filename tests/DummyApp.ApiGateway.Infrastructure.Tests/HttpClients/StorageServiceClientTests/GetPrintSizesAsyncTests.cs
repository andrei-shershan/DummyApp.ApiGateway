using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public sealed class GetPrintSizesAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetPrintSizesAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetPrintSizesAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetPrintSizesAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetPrintSizesAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when getting print sizes.", Times.Once());
    }

    [Fact]
    public async Task GetPrintSizesAsync_ReturnsPrintSizes_WhenResponseIsSuccessful()
    {
        var expected = new[]
        {
            new PrintSizeDto
            {
                Id = 1,
                Name = "A1",
                Prices = new[]
                {
                    new PriceDto { Id = 1, PrintSizeId = 1, Value = 100m, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
                }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<PrintSizeDto>>(expected)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetPrintSizesAsync(CancellationToken.None);

        Assert.Equal(expected, result);
        VerifyNoLogs();
    }
}
