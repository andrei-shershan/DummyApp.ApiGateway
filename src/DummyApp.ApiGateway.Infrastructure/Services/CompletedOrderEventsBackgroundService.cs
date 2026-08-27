using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Messaging.ServiceBus;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class CompletedOrderEventsBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusOptions _serviceBusOptions;
    private readonly IEmailServiceHttpClient _emailServiceClient;
    private readonly IFileServiceHttpClient _fileServiceClient;
    private readonly IAnalyticsServiceHttpClient _analyticsServiceClient;
    private readonly ILogger<CompletedOrderEventsBackgroundService> _logger;
    private readonly string? _orderQrCodeText;
    private readonly string _siteId;
    private ServiceBusProcessor? _processor;

    public CompletedOrderEventsBackgroundService(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IEmailServiceHttpClient emailServiceClient,
        IFileServiceHttpClient fileServiceClient,
        IAnalyticsServiceHttpClient analyticsServiceClient,
        IOptions<OrderQRCodeOptions> orderQrCodeOptions,
        IOptions<ApplicationOptions> applicationOptions,
        ILogger<CompletedOrderEventsBackgroundService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _serviceBusOptions = serviceBusOptions?.Value ?? throw new ArgumentNullException(nameof(serviceBusOptions));
        _emailServiceClient = emailServiceClient ?? throw new ArgumentNullException(nameof(emailServiceClient));
        _fileServiceClient = fileServiceClient ?? throw new ArgumentNullException(nameof(fileServiceClient));
        _analyticsServiceClient = analyticsServiceClient ?? throw new ArgumentNullException(nameof(analyticsServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderQrCodeText = orderQrCodeOptions?.Value?.OrderQRCode;
        _siteId = applicationOptions?.Value?.SiteId ?? string.Empty;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_serviceBusOptions.CompletedOrderEventsQueueName))
        {
            _logger.LogWarning("ServiceBus:CompletedOrderEventsQueueName is not configured. Completed order event subscription will not start.");
            return;
        }

        _processor = _serviceBusClient.CreateProcessor(_serviceBusOptions.CompletedOrderEventsQueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("Starting Service Bus processor for queue {QueueName}.", _serviceBusOptions.CompletedOrderEventsQueueName);
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = GetMessageBody(message);
        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Received empty completed order event message {MessageId}.", message.MessageId);
            await args.AbandonMessageAsync(message);
            return;
        }

        OrderSummaryDto? orderSummary;
        try
        {
            orderSummary = JsonSerializer.Deserialize<OrderSummaryDto>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize completed order event message {MessageId}. Body: {Body}", message.MessageId, body);
            await args.AbandonMessageAsync(message);
            return;
        }

        if (orderSummary is null || orderSummary.Address is null || string.IsNullOrWhiteSpace(orderSummary.Address.Email))
        {
            _logger.LogWarning("Completed order event message {MessageId} does not contain a valid order summary or recipient email.", message.MessageId);
            await args.AbandonMessageAsync(message);
            return;
        }

        var parameters = JsonDocument.Parse(body).RootElement.Clone();

        if (!string.IsNullOrWhiteSpace(_orderQrCodeText))
        {
            try
            {
                var qrCodeBase64 = await _fileServiceClient.GenerateQrCodeBase64Async(_orderQrCodeText, 20, args.CancellationToken);
                if (parameters.TryGetProperty("QrCodeBase64", out _))
                {
                    parameters = JsonDocument.Parse(parameters.GetRawText()).RootElement.Clone();
                }
                var jsonObject = JsonNode.Parse(parameters.GetRawText())?.AsObject() ?? new JsonObject();
                jsonObject["QrCodeBase64"] = qrCodeBase64;
                parameters = JsonDocument.Parse(jsonObject.ToJsonString()).RootElement.Clone();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR code for order email. Message {MessageId} will be abandoned.", message.MessageId);
                await args.AbandonMessageAsync(message);
                return;
            }
        }

        try
        {
            var analyticsEvent = BuildCompletedOrderAnalyticsEvent(body);
            if (analyticsEvent is not null)
            {
                await _analyticsServiceClient.PublishEventAsync(analyticsEvent, args.CancellationToken);
                _logger.LogInformation("Analytics event published for message {MessageId}.", message.MessageId);
            }
            else
            {
                _logger.LogWarning("Completed order event message {MessageId} could not be converted into an analytics event.", message.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish analytics event for message {MessageId}.", message.MessageId);
        }

        SendEmailAttachment? pdfAttachment = null;
        try
        {
            var pdfBytes = await _fileServiceClient.GeneratePdfAsync(new GeneratePdfRequest
            {
                Template = "OrderSummary",
                Parameters = parameters
            }, args.CancellationToken);

            pdfAttachment = new SendEmailAttachment
            {
                Name = "order-summary.pdf",
                ContentType = "application/pdf",
                Base64Content = Convert.ToBase64String(pdfBytes)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate order summary PDF for message {MessageId}. Message will be abandoned.", message.MessageId);
            await args.AbandonMessageAsync(message);
            return;
        }

        var emailRequest = new SendEmailRequest
        {
            Subject = $"Order {orderSummary.Status}",
            Recipients = new[] { orderSummary.Address.Email },
            Template = "CompletedOrder",
            Parameters = parameters,
            Attachments = pdfAttachment is null ? Array.Empty<SendEmailAttachment>() : new[] { pdfAttachment }
        };

        var sendSuccess = false;
        try
        {
            sendSuccess = await _emailServiceClient.SendEmailAsync(emailRequest, args.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completed order email for message {MessageId}.", message.MessageId);
        }

        if (sendSuccess)
        {
            await args.CompleteMessageAsync(message);
            _logger.LogInformation("Completed order event message {MessageId} completed successfully.", message.MessageId);
        }
        else
        {
            await args.AbandonMessageAsync(message);
            _logger.LogWarning("Completed order event message {MessageId} was abandoned because email sending failed.", message.MessageId);
        }
    }

    private static string? GetMessageBody(ServiceBusReceivedMessage message)
    {
        var bytes = message.Body?.ToArray();
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(bytes);
    }

    public static SendEmailRequest? BuildCompletedOrderEmailRequest(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        OrderSummaryDto? orderSummary;
        try
        {
            orderSummary = JsonSerializer.Deserialize<OrderSummaryDto>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (orderSummary is null || orderSummary.Address is null || string.IsNullOrWhiteSpace(orderSummary.Address.Email))
        {
            return null;
        }

        var parameters = JsonDocument.Parse(body).RootElement.Clone();

        return new SendEmailRequest
        {
            Subject = $"Order {orderSummary.Status}",
            Recipients = new[] { orderSummary.Address.Email },
            Template = "CompletedOrder",
            Parameters = parameters
        };
    }

    private AnalyticsEventRequest? BuildCompletedOrderAnalyticsEvent(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        OrderSummaryDto? orderSummary;
        try
        {
            orderSummary = JsonSerializer.Deserialize<OrderSummaryDto>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (orderSummary is null)
        {
            return null;
        }

        var tags = Array.Empty<string>();
        try
        {
            using var jsonDocument = JsonDocument.Parse(body);
            if (jsonDocument.RootElement.TryGetProperty("Tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
            {
                tags = tagsElement.EnumerateArray()
                    .Select(tagElement => tagElement.ValueKind == JsonValueKind.String
                        ? tagElement.GetString()
                        : tagElement.ValueKind == JsonValueKind.Object && tagElement.TryGetProperty("Name", out var tagName)
                            ? tagName.GetString()
                            : null)
                    .Where(tagText => !string.IsNullOrWhiteSpace(tagText))
                    .Cast<string>()
                    .ToArray();
            }
        }
        catch
        {
            tags = Array.Empty<string>();
        }

        return new AnalyticsEventRequest
        {
            OrderId = orderSummary.OrderId,
            Status = orderSummary.Status,
            Email = orderSummary.Email,
            SiteId = _siteId,
            Address = orderSummary.Address is null
                ? null
                : new AnalyticsOrderAddress
                {
                    FirstName = orderSummary.Address.FirstName,
                    LastName = orderSummary.Address.LastName,
                    Phone = orderSummary.Address.Phone,
                    Email = orderSummary.Address.Email,
                    Country = orderSummary.Address.Country,
                    City = orderSummary.Address.City,
                    Street = orderSummary.Address.Street,
                    HouseNumber = orderSummary.Address.HouseNumber,
                    PostalCode = orderSummary.Address.PostalCode
                },
            Items = orderSummary.Items.Select(item => new AnalyticsOrderItem
            {
                OrderId = item.OrderId,
                ArtworkId = item.ArtworkId,
                Quantity = item.Quantity,
                Name = item.Name,
                Description = item.Description,
                ImgUrl = item.ImgUrl,
                ThumbnailUrl = item.ThumbnailUrl,
                PrintSizeId = item.PrintSizeId,
                PrintSizeName = item.PrintSizeName,
                PriceId = item.PriceId,
                PriceValue = item.PriceValue
            }),
            Tags = tags,
            EventTimestamp = DateTimeOffset.UtcNow
        };
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error on queue {QueueName}.", _serviceBusOptions.CompletedOrderEventsQueueName);
        return Task.CompletedTask;
    }
}
