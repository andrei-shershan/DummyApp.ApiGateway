using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class CreateArtworkBodyRequest
{
    [Required]
    [StringLength(200, ErrorMessage = "Name must be 200 characters or fewer.")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, ErrorMessage = "Description must be 2000 characters or fewer.")]
    public string Description { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    [Required]
    public DateTime CreationDate { get; init; }

    [Required]
    public string UploadedImage { get; init; } = string.Empty;
}
