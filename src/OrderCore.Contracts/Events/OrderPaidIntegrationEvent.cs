namespace OrderCore.Contracts.Events
{
    public sealed record OrderPaidIntegrationEvent(
        Guid OrderId,
        Guid CustomerId,
        decimal TotalAmount,
        DateTime PaidAtUtc);
}
