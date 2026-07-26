using OrderCore.Domain.Common;

namespace OrderCore.Domain.Entities
{
    public class Notification : BaseEntity
    {
        private Notification()
        {
            Type = string.Empty;
            Title = string.Empty;
            Message = string.Empty;
        }

        public Notification(
            string type,
            string title,
            string message,
            Guid sourceMessageId,
            Guid? orderId = null,
            Guid? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Notification type cannot be empty.");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Notification title cannot be empty.");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message cannot be empty.");

            if (sourceMessageId == Guid.Empty)
                throw new ArgumentException("Notification source message id cannot be empty.");

            Type = type;
            Title = title;
            Message = message;
            SourceMessageId = sourceMessageId;
            OrderId = orderId;
            CustomerId = customerId;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public string Type { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public Guid SourceMessageId { get; private set; }
        public Guid? OrderId { get; private set; }
        public Guid? CustomerId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ReadAtUtc { get; private set; }

        public bool IsRead => ReadAtUtc.HasValue;

        public void MarkAsRead()
        {
            ReadAtUtc ??= DateTime.UtcNow;
        }
    }
}
