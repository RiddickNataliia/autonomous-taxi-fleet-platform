namespace NovaDrive.Infrastructure.Repositories;

internal sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Looks up a user by their unique e-mail address.
    /// Used during login and duplicate-registration checks.
    /// Comparison is case-insensitive.
    /// </summary>
    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
        => await DbSet
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == email.ToLower(),
                cancellationToken);


    public async Task<User?> GetByAuth0UserId(string auth0UserId, CancellationToken cancellationToken = default)
        => await DbSet
        .Include(u => u.PassengerProfile)
        .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId, cancellationToken);
}


