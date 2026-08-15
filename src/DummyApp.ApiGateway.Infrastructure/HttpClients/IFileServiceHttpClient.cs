namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IFileServiceHttpClient
{
    Task<string> GenerateQrCodeBase64Async(string text, int pixelsPerModule, CancellationToken cancellationToken);
    Task<byte[]> GeneratePdfAsync(GeneratePdfRequest request, CancellationToken cancellationToken);
}
