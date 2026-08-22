using System;
using System.Collections.Generic;
using System.Linq;
using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetArtworkPrerequisiteQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ITagFilterService> _tagFilterServiceMock = new();

    private GetArtworkPrerequisiteQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _tagFilterServiceMock.Object);

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenStorageServiceReturnsNull()
    {
        _storageServiceClientMock
            .Setup(x => x.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<TagDto>?)null);

        _tagFilterServiceMock
            .Setup(x => x.FilterTags(It.IsAny<IEnumerable<TagDto>>()))
            .Returns<IEnumerable<TagDto>>(tags => tags ?? Enumerable.Empty<TagDto>());

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworkPrerequisiteQuery(), CancellationToken.None);

        Assert.Empty(result);
        _tagFilterServiceMock.Verify(x => x.FilterTags(It.IsAny<IEnumerable<TagDto>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GroupsAndOrdersTags_ByTypeAndName()
    {
        var tags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "Zebra", Type = "Series" },
            new TagDto { Id = Guid.NewGuid(), Name = "Alpha", Type = "Series" },
            new TagDto { Id = Guid.NewGuid(), Name = "Beta", Type = "None" },
            new TagDto { Id = Guid.NewGuid(), Name = "Gamma", Type = "Other" }
        };

        _storageServiceClientMock
            .Setup(x => x.GetTagsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        _tagFilterServiceMock
            .Setup(x => x.FilterTags(tags))
            .Returns(tags);

        var handler = CreateHandler();
        var result = (await handler.Handle(new GetArtworkPrerequisiteQuery(), CancellationToken.None)).ToArray();

        Assert.Equal(3, result.Length);
        Assert.Equal(new[] { "None", "Other", "Series" }, result.Select(g => g.TagType));
        Assert.Equal(new[] { "Beta" }, result[0].Tags.Select(t => t.Name));
        Assert.Equal(new[] { "Gamma" }, result[1].Tags.Select(t => t.Name));
        Assert.Equal(new[] { "Alpha", "Zebra" }, result[2].Tags.Select(t => t.Name));
    }
}
