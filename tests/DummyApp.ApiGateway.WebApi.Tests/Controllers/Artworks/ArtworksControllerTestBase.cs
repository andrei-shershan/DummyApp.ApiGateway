using System.Security.Claims;
using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Artworks;

public abstract class ArtworksControllerTestBase
{
    protected static ArtworksController CreateController(
        Mock<IMediator> mediatorMock,
        Mock<ILogger<ArtworksController>> loggerMock,
        ClaimsPrincipal? user = null)
    {
        return new ArtworksController(mediatorMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };
    }

    protected static CreateArtworkBodyRequest CreateValidArtworkRequest() => new()
    {
        Name = "Name",
        Description = "Description",
        FileName = "file.png",
        CreationDate = DateTime.UtcNow,
        IsActive = true,
        UploadedImage = "base64image"
    };

    protected static ClaimsPrincipal CreateUserWithId(string creatorId) =>
        new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, creatorId) }, "TestAuth"));

    protected static ClaimsPrincipal CreateUserWithoutId() =>
        new(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth"));
}
