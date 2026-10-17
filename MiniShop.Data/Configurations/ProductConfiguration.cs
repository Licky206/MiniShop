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
        b.Property(p => p.ImageUrl).HasMaxLength(300);
        b.Property(p => p.Price).HasColumnType("decimal(18,2)");
        // CreatedAt se postavlja u kodu (= DateTime.UtcNow), ne HasDefaultValueSql

        // isto sto i [Timestamp]: rowversion kolona i token konkurentnosti
        b.Property(p => p.RowVersion).IsRowVersion();

        b.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("UX_Products_Sku");
        b.HasIndex(p => new { p.CategoryId, p.IsActive });

        // kategorija sa proizvodima ne sme da se obrise
        b.HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        // N:M ka Tag je u TagConfiguration (nedelja 5)
    }
}