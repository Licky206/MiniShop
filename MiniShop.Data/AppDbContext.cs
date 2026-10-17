using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;

namespace MiniShop.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Tag> Tags => Set<Tag>();
    // OrderItem i ShippingAddress nemaju DbSet: pristupa im se kroz Order

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ucitava sve IEntityTypeConfiguration klase iz asemblija
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(b);
    }
}