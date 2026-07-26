namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class AddArtworkToBasketRequest
{
    public Guid ArtworkId { get; set; }
    public int? Quantity { get; set; }
}
