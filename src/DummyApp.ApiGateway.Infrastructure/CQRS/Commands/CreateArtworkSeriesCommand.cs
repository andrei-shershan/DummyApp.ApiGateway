using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record CreateArtworkSeriesCommand(string Name, string CreatorId) : IRequest<SeriesDto?>;
