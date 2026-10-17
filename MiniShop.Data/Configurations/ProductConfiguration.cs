using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Domain.Entities;

namespace MiniShop.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(p => p.Id);

        b.Property(p => p.Name).IsRequired().HasMaxLength(120);
        b.Property(p => p.Sku).IsRequired().HasMaxLength(32);
        b.Property(p => p.Description).HasMaxLength(2000);
        b.Property(p => p.Price).HasColumnType("decimal(18,2)");
        // CreatedAt se postavlja u kodu (= DateTime.UtcNow), ne HasDefaultValueSql

        b.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("UX_Products_Sku");
        b.HasIndex(p => new { p.CategoryId, p.IsActive });

        // kategorija sa proizvodima ne sme da se obrise
        b.HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}