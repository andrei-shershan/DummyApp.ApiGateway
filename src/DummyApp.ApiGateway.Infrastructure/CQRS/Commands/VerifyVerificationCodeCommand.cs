using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record VerifyVerificationCodeCommand(string Email, string Code) : IRequest<VerifyVerificationCodeResult>;
