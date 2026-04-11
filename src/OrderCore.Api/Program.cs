using OrderCore.Application.Customers.Commands;
using OrderCore.Application.Customers.Queries;
using OrderCore.Application.Products.Commands;
using OrderCore.Application.Products.Queries;
using OrderCore.Application.Orders.Commands;
using OrderCore.Application.Orders.Queries;
using OrderCore.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthorization();
app.MapControllers();

app.Run();