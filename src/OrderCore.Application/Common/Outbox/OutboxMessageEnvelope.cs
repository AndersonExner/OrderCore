namespace OrderCore.Application.Common.Outbox
{
    public sealed record OutboxMessageEnvelope(
        Guid Id,
        string Type,
        string Payload,
        int RetryCount,
        DateTime CreatedAtUtc);
}
