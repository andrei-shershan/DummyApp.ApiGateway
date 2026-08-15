using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services;

public sealed class CompletedOrderEventsBackgroundServiceTests
{
    [Fact]
    public void BuildCompletedOrderEmailRequest_ReturnsNull_WhenBodyIsEmpty()
    {
        var result = CompletedOrderEventsBackgroundService.BuildCompletedOrderEmailRequest(string.Empty);

        Assert.Null(result);
    }

    [Fact]
    public void BuildCompletedOrderEmailRequest_ReturnsNull_WhenBodyIsInvalidJson()
    {
        var result = CompletedOrderEventsBackgroundService.BuildCompletedOrderEmailRequest("not-json");

        Assert.Null(result);
    }

    [Fact]
    public void BuildCompletedOrderEmailRequest_ReturnsNull_WhenAddressEmailIsMissing()
    {
        var body = JsonSerializer.Serialize(new
        {
            Items = new[]
            {
                new
                {
                    OrderId = Guid.NewGuid(),
                    ArtworkId = Guid.NewGuid(),
                    Quantity = 2,
                    Name = "test",
                    Description = "test",
                    ImgUrl = "https://example.com/img.png",
                    ThumbnailUrl = "https://example.com/thumb.png",
                    PrintSizeId = 2,
                    PrintSizeName = "A2",
                    PriceId = 2,
                    PriceValue = 80.00m
                }
            },
            Status = "Completed",
            Address = new
            {
                FirstName = "Andrei",
                LastName = "Smith",
                Phone = "101202303",
                Email = "",
                Country = "Poland",
                City = "Warszawa",
                Street = "Dobra",
                HouseNumber = "1",
                PostalCode = "123465"
            }
        });

        var result = CompletedOrderEventsBackgroundService.BuildCompletedOrderEmailRequest(body);

        Assert.Null(result);
    }

    [Fact]
    public void BuildCompletedOrderEmailRequest_ReturnsExpectedSendEmailRequest_WhenBodyIsValid()
    {
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();
        var body = JsonSerializer.Serialize(new
        {
            Items = new[]
            {
                new
                {
                    OrderId = orderId,
                    ArtworkId = artworkId,
                    Quantity = 2,
                    Name = "test",
                    Description = "test",
                    ImgUrl = "https://example.com/img.png",
                    ThumbnailUrl = "https://example.com/thumb.png",
                    PrintSizeId = 2,
                    PrintSizeName = "A2",
                    PriceId = 2,
                    PriceValue = 80.00m
                }
            },
            Status = "Completed",
            Address = new
            {
                FirstName = "Andrei",
                LastName = "Smith",
                Phone = "101202303",
                Email = "mail.shershan@gmail.com",
                Country = "Poland",
                City = "Warszawa",
                Street = "Dobra",
                HouseNumber = "1",
                PostalCode = "123465"
            }
        });

        var result = CompletedOrderEventsBackgroundService.BuildCompletedOrderEmailRequest(body);

        Assert.NotNull(result);
        Assert.Equal("Order Completed", result.Subject);
        Assert.Collection(result.Recipients, recipient => Assert.Equal("mail.shershan@gmail.com", recipient));
        Assert.Equal("CompletedOrder", result.Template);
        Assert.True(result.Parameters.HasValue);
        Assert.Equal("Completed", result.Parameters.Value.GetProperty("Status").GetString());
        Assert.Equal(orderId.ToString(), result.Parameters.Value.GetProperty("Items")[0].GetProperty("OrderId").GetGuid().ToString());
        Assert.Equal(artworkId.ToString(), result.Parameters.Value.GetProperty("Items")[0].GetProperty("ArtworkId").GetGuid().ToString());
    }
}
