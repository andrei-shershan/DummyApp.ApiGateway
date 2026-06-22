namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IBlobServiceHttpClient
{
    Task<ImageUploadResult?> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken);
}
