namespace NovaDrive.Infrastructure.Repositories;

internal abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet   = context.Set<T>();
    }

    public async Task<T?> GetById(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public async Task Add(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    // for detached entities (e.g. from a unit test that builds an entity outside a DbContext scope).
    public Task Update(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetById(id, cancellationToken);
        if (entity is not null)
            DbSet.Remove(entity);
    }
}
