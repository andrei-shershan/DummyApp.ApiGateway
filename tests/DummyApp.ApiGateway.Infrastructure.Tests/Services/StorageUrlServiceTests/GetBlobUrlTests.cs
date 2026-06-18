using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.StorageUrlServiceTests;

public class GetBlobUrlTests
{
    private static StorageUrlService CreateService(string storageUrl, string containerName)
        => new StorageUrlService(Options.Create(new BlobStorageOptions
        {
            StorageUrl = storageUrl,
            ContainerName = containerName
        }));

    [Fact]
    public void GetBlobUrl_ReturnsEmptyString_WhenBlobPathIsNull()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl(null!);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetBlobUrl_ReturnsEmptyString_WhenBlobPathIsWhitespace()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl("   ");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetBlobUrl_ReturnsOriginalUrl_WhenBlobPathIsAbsoluteHttp()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl("http://cdn.example.com/blob.txt");

        Assert.Equal("http://cdn.example.com/blob.txt", result);
    }

    [Fact]
    public void GetBlobUrl_ReturnsOriginalUrl_WhenBlobPathIsAbsoluteHttps()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl("https://cdn.example.com/blob.txt");

        Assert.Equal("https://cdn.example.com/blob.txt", result);
    }

    [Fact]
    public void GetBlobUrl_ReturnsUrlWithContainer_WhenBlobPathDoesNotStartWithContainerName()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl("path/to/blob.txt");

        Assert.Equal("https://example.com/container/path/to/blob.txt", result);
    }

    [Fact]
    public void GetBlobUrl_ReturnsUrlWithoutDuplicateContainer_WhenBlobPathAlreadyStartsWithContainerName()
    {
        var service = CreateService("https://example.com", "container");

        var result = service.GetBlobUrl("container/path/to/blob.txt");

        Assert.Equal("https://example.com/container/path/to/blob.txt", result);
    }
}
