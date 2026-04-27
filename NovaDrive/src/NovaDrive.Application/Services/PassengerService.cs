namespace NovaDrive.Application.Services;

public interface IPassengerService
{

    Task<PassengerResponse> GetProfile(string auth0UserId, CancellationToken ct = default);


    Task<PassengerResponse> UpdateProfile(string auth0UserId, UpdateProfileRequest request, CancellationToken ct = default);

    Task<IEnumerable<PassengerResponse>> GetAllPassengers(CancellationToken ct = default);

    Task<PassengerResponse> GetById(Guid passengerId, CancellationToken ct = default);
}

public sealed class PassengerService : IPassengerService
{
    private readonly IUserRepository _userRepo;
    private readonly IPassengerRepository _passengerRepo;
    private readonly IUnitOfWork          _unitOfWork;

    public PassengerService(
        IUserRepository userRepo,
        IPassengerRepository passengerRepo,
        IUnitOfWork          unitOfWork)
    {
        _userRepo = userRepo;
        _passengerRepo = passengerRepo;
        _unitOfWork    = unitOfWork;
    }

    public async Task<PassengerResponse> GetProfile(
        string auth0UserId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByAuth0UserId(auth0UserId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        return user.PassengerProfile!.ToResponse();
    }

    public async Task<PassengerResponse> UpdateProfile(
        string auth0UserId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByAuth0UserId(auth0UserId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        if (!Enum.TryParse<PaymentMethod>(request.PreferredPaymentMethod, ignoreCase: true, out var method)
            || method == PaymentMethod.Unknown)
            throw new UserDomainException(UserDomainException.InvalidPaymentMethod);

        user.PassengerProfile!.UpdateProfile(request.FullName, request.HomeAddress, method);
        await _unitOfWork.SaveChanges(ct);

        return user.PassengerProfile.ToResponse();
    }

    public async Task<IEnumerable<PassengerResponse>> GetAllPassengers(CancellationToken ct = default)
    => (await _passengerRepo.GetAll(ct)).Select(p => p.ToResponse());

    public async Task<PassengerResponse> GetById(Guid passengerId, CancellationToken ct = default)
    {
        var passenger = await _passengerRepo.GetById(passengerId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        return passenger.ToResponse();
}
}