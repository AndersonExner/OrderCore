using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderCore.Application.Notifications.Commands;
using OrderCore.Infrastructure.DependencyInjection;
using OrderCore.Infrastructure.Messaging;
using OrderCore.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    OutboxPublisherTypes.Logging);

builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddScoped<CreateOrderPaidNotificationService>();
builder.Services.AddHostedService<OrderPaidNotificationConsumerService>();

var host = builder.Build();

await host.RunAsync();
