using DummyApp.ApiGateway.Infrastructure.Models;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class StorageUrlService : IStorageUrlService
{
    private readonly string _storageUrl;
    private readonly string _containerName;

    public StorageUrlService(BlobStorageSettings settings)
    {   
        _storageUrl = (settings.StorageUrl ?? string.Empty).Trim().TrimEnd('/');
        _containerName = (settings.ContainerName ?? string.Empty).Trim('/');

        if (string.IsNullOrWhiteSpace(_storageUrl))
        {
            throw new InvalidOperationException("Blob storage URL is not configured. Set BlobStorage__StorageUrl.");
        }

        if (string.IsNullOrWhiteSpace(_containerName))
        {
            throw new InvalidOperationException("Blob storage container name is not configured. Set BlobStorage__ContainerName.");
        }
    }

    public string GetBlobUrl(string blobPath)
    {
        if (string.IsNullOrWhiteSpace(blobPath))
        {
            return string.Empty;
        }

        if (blobPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            blobPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return blobPath;
        }

        var normalizedBlobPath = blobPath.Trim('/');
        if (normalizedBlobPath.StartsWith($"{_containerName}/", StringComparison.OrdinalIgnoreCase))
        {
            return $"{_storageUrl}/{normalizedBlobPath}";
        }

        return $"{_storageUrl}/{_containerName}/{normalizedBlobPath}";
    }
}
