using System.ComponentModel.DataAnnotations;

namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class UpdateUserActiveStateRequest
{
    [Required]
    public bool? IsActive { get; init; }
}
