using MediatR;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetPrintSizesQuery() : IRequest<IEnumerable<PrintSizeDto>>;
