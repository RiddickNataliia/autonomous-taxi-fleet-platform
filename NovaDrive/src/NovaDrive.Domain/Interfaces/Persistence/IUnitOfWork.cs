namespace NovaDrive.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}