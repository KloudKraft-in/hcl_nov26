using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ContosoOrders.Processor;

public sealed class ProcessOrder
{
    private readonly ILogger<ProcessOrder> _logger;

    public ProcessOrder(ILogger<ProcessOrder> logger) => _logger = logger;

    [Function(nameof(ProcessOrder))]
    public void Run(
        [QueueTrigger("orders", Connection = "OrdersQueue")] string message)
    {
        _logger.LogInformation("Order message received from the orders queue.");

        OrderMessage? order;
        try
        {
            order = JsonSerializer.Deserialize<OrderMessage>(
                message,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Order message contains invalid JSON.");
            throw;
        }

        if (order is null ||
            string.IsNullOrWhiteSpace(order.OrderId) ||
            string.IsNullOrWhiteSpace(order.CustomerName) ||
            string.IsNullOrWhiteSpace(order.Product) ||
            order.Quantity < 1)
        {
            _logger.LogError("Order message failed business validation.");
            throw new InvalidOperationException("Order message failed validation.");
        }

        _logger.LogInformation(
            "Order processed. OrderId={OrderId}; Customer={CustomerName}; Product={Product}; Quantity={Quantity}",
            order.OrderId,
            order.CustomerName,
            order.Product,
            order.Quantity);
    }
}

public sealed class OrderMessage
{
    public string? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? Product { get; set; }
    public int Quantity { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
