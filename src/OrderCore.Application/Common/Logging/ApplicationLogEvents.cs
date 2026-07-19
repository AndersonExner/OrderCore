using Microsoft.Extensions.Logging;

namespace OrderCore.Application.Common.Logging
{
    public static class ApplicationLogEvents
    {
        public static readonly EventId CustomerCreateStarted = new(1001, nameof(CustomerCreateStarted));
        public static readonly EventId CustomerCreateRejected = new(1002, nameof(CustomerCreateRejected));
        public static readonly EventId CustomerCreated = new(1003, nameof(CustomerCreated));

        public static readonly EventId ProductCreateStarted = new(2001, nameof(ProductCreateStarted));
        public static readonly EventId ProductCreated = new(2002, nameof(ProductCreated));

        public static readonly EventId OrderCreateStarted = new(3001, nameof(OrderCreateStarted));
        public static readonly EventId OrderCreateRejected = new(3002, nameof(OrderCreateRejected));
        public static readonly EventId OrderCreated = new(3003, nameof(OrderCreated));

        public static readonly EventId OrderPayStarted = new(3101, nameof(OrderPayStarted));
        public static readonly EventId OrderPayRejected = new(3102, nameof(OrderPayRejected));
        public static readonly EventId OrderPaid = new(3103, nameof(OrderPaid));

        public static readonly EventId OrderCancelStarted = new(3201, nameof(OrderCancelStarted));
        public static readonly EventId OrderCancelRejected = new(3202, nameof(OrderCancelRejected));
        public static readonly EventId OrderCancelled = new(3203, nameof(OrderCancelled));

        public static readonly EventId OutboxMessagePublishFailed = new(4001, nameof(OutboxMessagePublishFailed));
    }
}
