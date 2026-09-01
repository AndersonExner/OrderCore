using OrderCore.Api.BackgroundServices;
using OrderCore.Api.Security;
using OrderCore.Application.Abstractions.Security;
using OrderCore.Application.Auth.Commands;
using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Queries;
using OrderCore.Application.Common.Outbox;
using OrderCore.Application.Notifications.Commands;
using OrderCore.Application.Notifications.Queries;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Queries;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Queries;
using OrderCore.Infrastructure.DependencyInjection;
using OrderCore.Infrastructure.Messaging;
using OrderCore.Api.Extensions;
using OrderCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog.Web;
using System.Text;

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
    });
});
builder.Services.Configure<OutboxProcessingOptions>(
    builder.Configuration.GetSection(OutboxProcessingOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? new JwtOptions();

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(AuthPolicies.AddOrderCorePolicies);

var outboxPublisher = builder.Configuration.GetValue<string>("Messaging:OutboxPublisher")
    ?? OutboxPublisherTypes.Logging;

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    outboxPublisher
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

builder.Services.AddScoped<CreateOrderPaidNotificationService>();
builder.Services.AddScoped<GetNotificationsService>();
builder.Services.AddScoped<MarkNotificationAsReadService>();
builder.Services.AddScoped<RegisterUserService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddHostedService<OutboxBackgroundService>();


var app = builder.Build();
app.Lifetime.ApplicationStopped.Register(NLog.LogManager.Shutdown);

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrations");
var outboxEnabled = app.Configuration.GetValue<bool>($"{OutboxProcessingOptions.SectionName}:Enabled");

startupLogger.LogInformation(
    "Starting OrderCore API. Environment: {EnvironmentName}, ApplyMigrations: {ApplyMigrations}, OutboxEnabled: {OutboxEnabled}, OutboxPublisher: {OutboxPublisher}",
    app.Environment.EnvironmentName,
    applyMigrations,
    outboxEnabled,
    outboxPublisher);

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

startupLogger.LogInformation("OrderCore API configured and ready to receive requests.");

app.Run();

public partial class Program
{
}
