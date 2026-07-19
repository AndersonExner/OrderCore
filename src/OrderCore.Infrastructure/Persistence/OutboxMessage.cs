namespace OrderCore.Infrastructure.Persistence
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Type { get; private set; }
        public string Payload { get; private set; }
        public string Status { get; private set; }
        public int RetryCount { get; private set; }
        public string? LastError { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime? ProcessedAtUtc { get; private set; }

        private OutboxMessage()
        {
            Type = string.Empty;
            Payload = string.Empty;
            Status = OutboxMessageStatus.Pending;
        }

        public OutboxMessage(string type, string payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Outbox message type cannot be empty.");

            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Outbox message payload cannot be empty.");

            Type = type.Trim();
            Payload = payload;
            Status = OutboxMessageStatus.Pending;
        }

        public void MarkAsProcessed()
        {
            Status = OutboxMessageStatus.Processed;
            ProcessedAtUtc = DateTime.UtcNow;
            LastError = null;
        }

        public void MarkAsFailed(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Outbox message error cannot be empty.");

            Status = OutboxMessageStatus.Failed;
            RetryCount++;
            LastError = error.Trim();
        }
    }
}
