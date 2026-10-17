namespace MiniShop.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("Tags");
        b.HasKey(t => t.Id);
        b.Property(t => t.Name).IsRequired().HasMaxLength(40);
        b.HasIndex(t => t.Name).IsUnique().HasDatabaseName("UX_Tags_Name");

        // cist N:M: skip navigacija, EF pravi tabelu ProductTags (ProductsId, TagsId)
        b.HasMany(t => t.Products).WithMany(p => p.Tags)
            .UsingEntity("ProductTags");

        b.HasData(
            new Tag { Id = 1, Name = "bezicno" },
            new Tag { Id = 2, Name = "gaming" },
            new Tag { Id = 3, Name = "usb-c" });
    }
}