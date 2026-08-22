using System.Collections.Generic;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface ITagFilterService
{
    IEnumerable<TagDto> FilterTags(IEnumerable<TagDto> tags);
}
