using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class CreateArtworkBodyRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string Description { get; init; } = string.Empty;

    [StringLength(100, ErrorMessage = "Series name must be 100 characters or fewer.")]
    public string? SeriesName { get; init; }

    public string FileName { get; init; } = string.Empty;

    [Required]
    public DateTime CreationDate { get; init; }

    [Required]
    public string UploadedImage { get; init; } = string.Empty;
}
