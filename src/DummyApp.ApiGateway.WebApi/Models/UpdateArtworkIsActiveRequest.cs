using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class UpdateArtworkIsActiveRequest
{
    [Required]
    public bool? IsActive { get; init; }
}
