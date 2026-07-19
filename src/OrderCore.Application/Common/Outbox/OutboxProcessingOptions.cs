namespace OrderCore.Application.Common.Outbox
{
    public class OutboxProcessingOptions
    {
        public const string SectionName = "Outbox";

        public bool Enabled { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 10;
        public int BatchSize { get; set; } = 20;
        public int MaxRetryCount { get; set; } = 5;
    }
}
