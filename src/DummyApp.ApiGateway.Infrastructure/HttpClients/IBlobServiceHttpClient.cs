using System.Threading;
using DummyApp.ApiGateway.Infrastructure.Models;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IBlobServiceHttpClient
{
    Task<ImageUploadResult?> UploadImageAsync(string base64Image, string fileName, ImageType imageType, CancellationToken cancellationToken);
}
