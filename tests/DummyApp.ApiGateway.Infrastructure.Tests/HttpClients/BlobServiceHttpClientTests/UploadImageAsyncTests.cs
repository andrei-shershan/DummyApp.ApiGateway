using System.Net;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.BlobServiceHttpClientTests;

public class UploadImageAsyncTests : BlobServiceHttpClientTestsBase
{
    [Fact]
    public async Task UploadImageAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = new BlobServiceHttpClient(httpClient, LoggerMock.Object);

        var result = await client.UploadImageAsync("base64", "file.png", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "BlobService returned", Times.Once());
    }

    [Fact]
    public async Task UploadImageAsync_ReturnsNull_WhenUrlIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { Url = string.Empty }), Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new BlobServiceHttpClient(httpClient, LoggerMock.Object);

        var result = await client.UploadImageAsync("base64", "file.png", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Unexpected response from BlobService when uploading image.", Times.Once());
    }

    [Fact]
    public async Task UploadImageAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new BlobServiceHttpClient(httpClient, LoggerMock.Object);

        var result = await client.UploadImageAsync("base64", "file.png", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to parse response from BlobService when uploading image.", Times.Once());
    }

    [Fact]
    public async Task UploadImageAsync_ReturnsUrl_WhenResponseIsSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { Url = "https://example.com/blob.png" }), Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new BlobServiceHttpClient(httpClient, LoggerMock.Object);

        var result = await client.UploadImageAsync("base64", "file.png", CancellationToken.None);

        Assert.Equal("https://example.com/blob.png", result);
        VerifyNoLogs();
    }
}
