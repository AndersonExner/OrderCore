using OrderCore.Api.BackgroundServices;
using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Queries;
using OrderCore.Application.Common.Outbox;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Queries;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Queries;
using OrderCore.Infrastructure.DependencyInjection;
using OrderCore.Api.Extensions;
using OrderCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<OutboxProcessingOptions>(
    builder.Configuration.GetSection(OutboxProcessingOptions.SectionName));

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

builder.Services.AddScoped<CreateCustomerService>();
builder.Services.AddScoped<GetCustomerByIdService>();
builder.Services.AddScoped<GetCustomerByIdentifierService>();
builder.Services.AddScoped<GetCustomersService>();

builder.Services.AddScoped<CreateProductService>();
builder.Services.AddScoped<GetProductByIdService>();
builder.Services.AddScoped<GetProductsService>();

builder.Services.AddScoped<CreateOrderService>();
builder.Services.AddScoped<PayOrderService>();
builder.Services.AddScoped<CancelOrderService>();
builder.Services.AddScoped<GetOrderByIdService>();
builder.Services.AddScoped<GetOrdersService>();
builder.Services.AddScoped<OutboxMessageProcessorService>();
builder.Services.AddHostedService<OutboxBackgroundService>();


var app = builder.Build();
app.Lifetime.ApplicationStopped.Register(NLog.LogManager.Shutdown);

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrations");
var outboxEnabled = app.Configuration.GetValue<bool>($"{OutboxProcessingOptions.SectionName}:Enabled");

startupLogger.LogInformation(
    "Starting OrderCore API. Environment: {EnvironmentName}, ApplyMigrations: {ApplyMigrations}, OutboxEnabled: {OutboxEnabled}",
    app.Environment.EnvironmentName,
    applyMigrations,
    outboxEnabled);

if (applyMigrations)
{
    startupLogger.LogInformation("Applying database migrations.");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    startupLogger.LogInformation("Database migrations applied.");
}

if (app.Environment.IsDevelopment())
{
    startupLogger.LogInformation("Swagger is enabled.");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseGlobalExceptionMiddleware();

app.UseAuthorization();
app.MapControllers();

startupLogger.LogInformation("OrderCore API configured and ready to receive requests.");

app.Run();

public partial class Program
{
}
