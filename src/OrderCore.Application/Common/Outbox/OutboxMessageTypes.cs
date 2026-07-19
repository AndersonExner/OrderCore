namespace OrderCore.Application.Common.Outbox
{
    public static class OutboxMessageTypes
    {
        public const string OrderCreated = "OrderCreated";
        public const string OrderPaid = "OrderPaid";
        public const string OrderCancelled = "OrderCancelled";
    }
}
