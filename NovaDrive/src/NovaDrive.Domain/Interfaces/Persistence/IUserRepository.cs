namespace NovaDrive.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
}