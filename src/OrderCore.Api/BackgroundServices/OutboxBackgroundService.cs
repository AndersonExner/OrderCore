using Microsoft.Extensions.Options;
using OrderCore.Application.Common.Outbox;

namespace OrderCore.Api.BackgroundServices
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxBackgroundService> _logger;
        private readonly OutboxProcessingOptions _options;

        public OutboxBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OutboxBackgroundService> logger,
            IOptions<OutboxProcessingOptions> options)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Outbox background service is disabled.");
                return;
            }

            var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, _options.PollingIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService>();

                    var processedCount = await processor.ExecuteAsync(
                        Math.Max(1, _options.BatchSize),
                        Math.Max(1, _options.MaxRetryCount),
                        stoppingToken);

                    if (processedCount > 0)
                    {
                        _logger.LogInformation(
                            "Processed {ProcessedCount} outbox messages.",
                            processedCount);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing outbox messages.");
                }

                try
                {
                    await Task.Delay(pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
