namespace MiniShop.Application.Abstractions;

// jedna poslovna operacija = jedan SaveChanges = jedna transakcija; AppDbContext ga implementira
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}