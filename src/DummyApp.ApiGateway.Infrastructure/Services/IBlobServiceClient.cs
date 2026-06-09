using System.Threading;
using System.Threading.Tasks;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public interface IBlobServiceClient
{
    Task<string> UploadImageAsync(string base64Image, string fileName, CancellationToken cancellationToken);
}
