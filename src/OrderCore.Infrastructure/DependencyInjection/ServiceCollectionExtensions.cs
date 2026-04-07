using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderCore.Application.Abstractions.Repositories;
using OrderCore.Infrastructure.Persistence;
using OrderCore.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }
    }
}
