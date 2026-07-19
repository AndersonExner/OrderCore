using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OrderCore.Infrastructure.Messaging
{
    public sealed class RabbitMqConnection : IDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly object _syncRoot = new();
        private IConnection? _connection;

        public RabbitMqConnection(
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqConnection> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public IConnection GetOpenConnection()
        {
            if (_connection?.IsOpen == true)
            {
                return _connection;
            }

            lock (_syncRoot)
            {
                if (_connection?.IsOpen == true)
                {
                    return _connection;
                }

                _connection?.Dispose();

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true
                };

                _connection = factory.CreateConnection("OrderCore.Api");

                _logger.LogInformation(
                    "RabbitMQ connection opened. Host: {RabbitMqHost}, Port: {RabbitMqPort}, VirtualHost: {RabbitMqVirtualHost}",
                    _options.HostName,
                    _options.Port,
                    _options.VirtualHost);

                return _connection;
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                _connection?.Dispose();
                _connection = null;
            }
        }
    }
}
