namespace DummyApp.ApiGateway.WebApi.Models;

public sealed class UpdateBasketItemRequest
{
    public int Quantity { get; set; }
    public int? PrintSizeId { get; set; }
    public int? PriceId { get; set; }
}
