using OrderCore.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")
);

var app = builder.Build();