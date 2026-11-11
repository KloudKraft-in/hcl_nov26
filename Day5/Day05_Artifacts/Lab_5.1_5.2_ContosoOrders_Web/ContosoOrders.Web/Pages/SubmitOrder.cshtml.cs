using System.ComponentModel.DataAnnotations;
using ContosoOrders.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoOrders.Web.Pages;

public sealed class SubmitOrderModel : PageModel
{
    private readonly QueueOrderService _queueService;
    private readonly ILogger<SubmitOrderModel> _logger;
    private readonly IConfiguration _configuration;

    public SubmitOrderModel(
        QueueOrderService queueService,
        ILogger<SubmitOrderModel> logger,
        IConfiguration configuration)
    {
        _queueService = queueService;
        _logger = logger;
        _configuration = configuration;
    }

    [BindProperty, Required, StringLength(80)]
    [Display(Name = "Customer name")]
    public string CustomerName { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(80)]
    public string Product { get; set; } = string.Empty;

    [BindProperty, Range(1, 1000)]
    public int Quantity { get; set; } = 1;

    public string? StatusMessage { get; private set; }
    public string EnvironmentName => _configuration["APP_ENVIRONMENT"] ?? "UNSPECIFIED";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var order = new OrderMessage
        {
            OrderId = $"ORD-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            CustomerName = CustomerName.Trim(),
            Product = Product.Trim(),
            Quantity = Quantity,
            SubmittedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _queueService.SendOrderAsync(order, cancellationToken);
            _logger.LogInformation("Order {OrderId} submitted to queue.", order.OrderId);
            StatusMessage = $"Order {order.OrderId} was submitted successfully.";
            ModelState.Clear();
            CustomerName = string.Empty;
            Product = string.Empty;
            Quantity = 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit order {OrderId}.", order.OrderId);
            StatusMessage = "Order submission failed. Check the application logs and identity permissions.";
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }

        return Page();
    }
}
