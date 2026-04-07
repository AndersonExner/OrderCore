using Microsoft.EntityFrameworkCore;
using OrderCore.Domain.Entities;

namespace OrderCore.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    { 
        public DbSet<Customer> Customers => Set<Customer>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
