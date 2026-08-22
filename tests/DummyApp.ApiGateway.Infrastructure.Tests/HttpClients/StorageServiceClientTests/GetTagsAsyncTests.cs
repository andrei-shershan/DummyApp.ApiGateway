using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class GetTagsAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetTagsAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetTagsAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetTagsAsync_ReturnsNull_WhenResponseContentIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetTagsAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when getting tags.", Times.Once());
    }

    [Fact]
    public async Task GetTagsAsync_ReturnsTags_WhenResponseIsSuccessful()
    {
        var expectedTags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "Artist", Type = "None" },
            new TagDto { Id = Guid.NewGuid(), Name = "Series", Type = "Series" }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedTags)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetTagsAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedTags, result);
        VerifyNoLogs();
    }
}
