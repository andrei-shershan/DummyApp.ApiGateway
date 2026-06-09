using System;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed record CreateArtworkBodyRequest(
    string Name,
    string Description,
    DateTime CreationDate,
    string ImgUrl,
    string SmallImgUrl,
    bool IsActive,
    string? UploadedImage);
