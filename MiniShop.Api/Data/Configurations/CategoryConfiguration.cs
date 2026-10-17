using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniShop.Api.Models;

namespace MiniShop.Api.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(c => c.Id);

        b.Property(c => c.Name).IsRequired().HasMaxLength(80);
        b.Property(c => c.Slug).IsRequired().HasMaxLength(80);
        b.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("UX_Categories_Slug");

        // pocetni podaci: postaju INSERT naredbe u migraciji
        b.HasData(
            new Category { Id = 1, Name = "Periferija", Slug = "periferija" },
            new Category { Id = 2, Name = "Monitori", Slug = "monitori" },
            new Category { Id = 3, Name = "Laptopovi", Slug = "laptopovi" });
    }
}