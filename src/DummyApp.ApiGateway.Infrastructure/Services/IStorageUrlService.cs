namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IStorageUrlService
{
    string GetBlobUrl(string blobPath);
}
