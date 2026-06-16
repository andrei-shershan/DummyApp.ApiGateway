namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IBlobServiceHttpClient
{
    Task<string> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken);
}
