using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace ContosoOrders.Web.Services;

public sealed class QueueOrderService
{
    private readonly QueueClient _queueClient;

    public QueueOrderService(IConfiguration configuration)
    {
        var serviceUri = configuration["QueueStorage:ServiceUri"]
            ?? throw new InvalidOperationException("QueueStorage:ServiceUri is not configured.");
        var queueName = configuration["QueueStorage:QueueName"]
            ?? throw new InvalidOperationException("QueueStorage:QueueName is not configured.");

        var options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queueClient = new QueueClient(
            new Uri($"{serviceUri.TrimEnd('/')}/{queueName}"),
            new DefaultAzureCredential(),
            options);
    }

    public async Task SendOrderAsync(OrderMessage order, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(order);
        await _queueClient.SendMessageAsync(payload, cancellationToken);
    }
}
