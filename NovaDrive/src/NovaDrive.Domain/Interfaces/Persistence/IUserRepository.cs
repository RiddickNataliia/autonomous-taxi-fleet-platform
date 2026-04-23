namespace NovaDrive.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByAuth0UserId(string auth0UserId, CancellationToken cancellationToken = default);
}