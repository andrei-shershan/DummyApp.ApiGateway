using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class NewTagRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Tag name must be 100 characters or fewer.")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(None|Series)$", ErrorMessage = "Tag type must be either 'None' or 'Series'.")]
    public string Type { get; init; } = string.Empty;
}
