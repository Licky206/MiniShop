namespace MiniShop.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options), IUnitOfWork
{
    // Users, Roles, UserRoles i ostale Identity tabele dolaze iz IdentityDbContext
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // PRVO Identity: kljucevi, indeksi i relacije AspNet* tabela
        base.OnModelCreating(b);

        // zatim nase IEntityTypeConfiguration klase (ukljucujuci AppUserConfiguration)
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // unit of work za Application sloj (nedelja 6): EF izuzetak ne izlazi iz Data sloja
    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            return await SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Red je u medjuvremenu izmenjen.", ex);
        }
    }
}