namespace NovaDrive.Application.Services;

public interface IPassengerService
{
    /// <summary>
    /// Returns the passenger profile for the currently authenticated user.
    /// </summary>
    Task<PassengerResponse> GetProfile(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates the passenger's full name, home address, and preferred payment method.
    /// </summary>
    Task<PassengerResponse> UpdateProfile(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
}

public sealed class PassengerService : IPassengerService
{
    private readonly IPassengerRepository _passengerRepo;
    private readonly IUnitOfWork          _unitOfWork;

    public PassengerService(
        IPassengerRepository passengerRepo,
        IUnitOfWork          unitOfWork)
    {
        _passengerRepo = passengerRepo;
        _unitOfWork    = unitOfWork;
    }

    public async Task<PassengerResponse> GetProfile(
        Guid userId, CancellationToken ct = default)
    {
        var passenger = await _passengerRepo.GetByUserId(userId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        return passenger.ToResponse();
    }

    public async Task<PassengerResponse> UpdateProfile(
        Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var passenger = await _passengerRepo.GetByUserId(userId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        if (!Enum.TryParse<PaymentMethod>(request.PreferredPaymentMethod, ignoreCase: true, out var method)
            || method == PaymentMethod.Unknown)
            throw new UserDomainException(UserDomainException.InvalidPaymentMethod);

        passenger.UpdateProfile(request.FullName, request.HomeAddress, method);
        await _unitOfWork.SaveChanges(ct);

        return passenger.ToResponse();
    }
}