namespace NovaDrive.Application.Services;
public interface IUserProvisioningService
{
    Task<Passenger> EnsurePassengerExists(string auth0UserId, string email, CancellationToken ct = default);
}

public sealed class UserProvisioningService : IUserProvisioningService
{
    private readonly IUserRepository _userRepo;
    private readonly IPassengerRepository _passengerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UserProvisioningService(
        IUserRepository userRepo,
        IPassengerRepository passengerRepo,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _passengerRepo = passengerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Passenger> EnsurePassengerExists(
        string auth0UserId, string email, CancellationToken ct = default)
    {
        var existingUser = await _userRepo.GetByAuth0UserId(auth0UserId, ct);
        if (existingUser is not null)
            return existingUser.PassengerProfile!;

        var user = User.Create(email, auth0UserId, UserRole.Passenger);
        await _userRepo.Add(user, ct);

        var passenger = Passenger.Create(user.Id, email, string.Empty);
        await _passengerRepo.Add(passenger, ct);

        await _unitOfWork.SaveChanges(ct);
        return passenger;
    }
}