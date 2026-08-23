namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record PaginatedResult<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount);