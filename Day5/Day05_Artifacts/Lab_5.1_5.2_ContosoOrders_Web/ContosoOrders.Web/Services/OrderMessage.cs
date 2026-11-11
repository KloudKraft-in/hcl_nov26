namespace ContosoOrders.Web.Services;

public sealed class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
