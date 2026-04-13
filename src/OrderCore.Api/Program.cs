using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Queries;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Queries;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Queries;
using OrderCore.Infrastructure.DependencyInjection;
using OrderCore.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

builder.Services.AddScoped<CreateCustomerService>();
builder.Services.AddScoped<GetCustomerByIdService>();

builder.Services.AddScoped<CreateProductService>();
builder.Services.AddScoped<GetProductByIdService>();
builder.Services.AddScoped<GetProductsService>();

builder.Services.AddScoped<CreateOrderService>();
builder.Services.AddScoped<GetOrderByIdService>();
builder.Services.AddScoped<GetOrdersService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseGlobalExceptionMiddleware();

app.UseAuthorization();
app.MapControllers();

app.Run();