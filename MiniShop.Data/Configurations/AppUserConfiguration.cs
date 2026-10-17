namespace MiniShop.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        // tabela AspNetUsers ostaje; dodajemo samo ogranicenje nase kolone
        b.Property(u => u.FullName).HasMaxLength(100);
    }
}