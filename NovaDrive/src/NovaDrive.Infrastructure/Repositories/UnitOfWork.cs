namespace NovaDrive.Infrastructure.Repositories;

/// <summary>
/// Wraps a single <see cref="ApplicationDbContext"/> instance so that
/// multiple repositories that share it can be committed in one atomic call.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
        => _context = context;

    /// <inheritdoc/>
    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
