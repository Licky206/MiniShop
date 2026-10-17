namespace MiniShop.Api.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(o => o.Id);

        b.Property(o => o.UserId).IsRequired().HasMaxLength(450);   // kao AspNetUsers.Id
        b.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.Total).HasPrecision(18, 2);
        // datetime2 nema zonu: pri citanju oznacavamo vrednost kao UTC ("Z" u JSON-u)
        b.Property(o => o.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // lista "moje narudzbenice": filter po korisniku, sort po datumu
        b.HasIndex(o => new { o.UserId, o.CreatedAt });

        // 1:N stavke - brisu se sa narudzbenicom
        b.HasMany(o => o.Items).WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 adresa - FK je na strani ShippingAddress, brise se sa narudzbenicom
        b.HasOne(o => o.Shipping).WithOne(s => s.Order)
            .HasForeignKey<ShippingAddress>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasKey(i => new { i.OrderId, i.ProductId });              // kompozitni kljuc
        b.Property(i => i.UnitPrice).HasPrecision(18, 2);

        // proizvod koji je prodat ne sme da se obrise: istorija kupovine ostaje tacna
        b.HasOne(i => i.Product).WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
{
    public void Configure(EntityTypeBuilder<ShippingAddress> b)
    {
        b.ToTable("ShippingAddresses");
        b.HasKey(s => s.OrderId);                                   // PK = FK
        b.Property(s => s.Street).IsRequired().HasMaxLength(200);
        b.Property(s => s.City).IsRequired().HasMaxLength(100);
        b.Property(s => s.PostalCode).IsRequired().HasMaxLength(20);
        b.Property(s => s.Country).IsRequired().HasMaxLength(2);
    }
}