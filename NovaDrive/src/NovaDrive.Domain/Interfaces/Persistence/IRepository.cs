namespace NovaDrive.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task Add(T entity, CancellationToken cancellationToken = default);
    Task Update(T entity, CancellationToken cancellationToken = default);
    Task Delete(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default);
}