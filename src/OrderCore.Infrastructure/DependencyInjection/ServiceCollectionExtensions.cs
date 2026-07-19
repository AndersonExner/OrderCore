using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderCore.Application.Abstractions.Messaging;
using OrderCore.Application.Abstractions.Persistence;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Infrastructure.Messaging;
using OrderCore.Infrastructure.Persistence;
using OrderCore.Infrastructure.Repositories;
using System;

namespace OrderCore.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString,
            string outboxPublisher)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            if (string.Equals(outboxPublisher, OutboxPublisherTypes.RabbitMq, StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<RabbitMqConnection>();
                services.AddScoped<IOutboxMessagePublisher, RabbitMqOutboxMessagePublisher>();
            }
            else
            {
                services.AddScoped<IOutboxMessagePublisher, LoggingOutboxMessagePublisher>();
            }

            return services;
        }
    }
}
