using Microsoft.EntityFrameworkCore;

namespace PaymentService.Core.Persistence;

public class PaymentDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        // Configure your entity mappings here
    }
}