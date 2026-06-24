using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Moq;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public abstract class AdminControllerTestBase
{
    protected static AdminController CreateController(Mock<IMediator> mediatorMock)
        => new AdminController(mediatorMock.Object);
}
