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
    private readonly ILogger<CompletedOrderEventsBackgroundService> _logger;
    private readonly string? _orderQrCodeText;
    private ServiceBusProcessor? _processor;

    public CompletedOrderEventsBackgroundService(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IEmailServiceHttpClient emailServiceClient,
        IFileServiceHttpClient fileServiceClient,
        IOptions<OrderQRCodeOptions> orderQrCodeOptions,
        ILogger<CompletedOrderEventsBackgroundService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _serviceBusOptions = serviceBusOptions?.Value ?? throw new ArgumentNullException(nameof(serviceBusOptions));
        _emailServiceClient = emailServiceClient ?? throw new ArgumentNullException(nameof(emailServiceClient));
        _fileServiceClient = fileServiceClient ?? throw new ArgumentNullException(nameof(fileServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderQrCodeText = orderQrCodeOptions?.Value?.OrderQRCode;
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

        var emailRequest = new SendEmailRequest
        {
            Subject = $"Order {orderSummary.Status}",
            Recipients = new[] { orderSummary.Address.Email },
            Template = "CompletedOrder",
            Parameters = parameters
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

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error on queue {QueueName}.", _serviceBusOptions.CompletedOrderEventsQueueName);
        return Task.CompletedTask;
    }
}
