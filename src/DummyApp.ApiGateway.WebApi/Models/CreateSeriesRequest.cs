using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class CreateSeriesRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Series name must be 100 characters or fewer.")]
    public string Name { get; init; } = string.Empty;
}
