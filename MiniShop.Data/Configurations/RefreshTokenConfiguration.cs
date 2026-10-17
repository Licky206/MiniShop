namespace MiniShop.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(t => t.Id);

        b.Property(t => t.UserId).IsRequired().HasMaxLength(450);   // kao AspNetUsers.Id
        b.Property(t => t.TokenHash).IsRequired().HasMaxLength(64); // Base64 SHA-256 = 44 znaka

        // pretraga pri svakoj obnovi ide po hesu: jedinstven indeks
        b.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("UX_RefreshTokens_TokenHash");
        b.HasIndex(t => t.UserId);                                  // opoziv porodice

        b.Ignore(t => t.IsActive);                                  // racuna se u kodu

        // brisanje korisnika brise i njegove refresh tokene
        b.HasOne<AppUser>().WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}