namespace NovaDrive.Application.Services;

public class RideService : IRideService
{
    private readonly IRideRepository _rideRepo;
    private readonly IPassengerRepository _passengerRepo;
    private readonly IDiscountRepository _discountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RideService(
        IRideRepository rideRepo, 
        IPassengerRepository passengerRepo, 
        IDiscountRepository discountRepo,
        IUnitOfWork unitOfWork)
    {
        _rideRepo = rideRepo;
        _passengerRepo = passengerRepo;
        _discountRepo = discountRepo;
        _unitOfWork = unitOfWork;
    }

    // Application/Services/RideService.cs
    public async Task CompleteRideAsync(
        Guid rideId,
        double distanceKm,
        int durationMinutes,
        string? discountCodeStr,
        CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(rideId, ct)
            ?? throw new RideDomainException(RideDomainException.NotFound);

        var passenger = await _passengerRepo.GetById(ride.PassengerId, ct)
            ?? throw new UserDomainException(UserDomainException.NotFound);

        var vehicle = await _vehicleRepo.GetById(ride.VehicleId, ct)
            ?? throw new VehicleDomainException(VehicleDomainException.NotFound);

        DiscountCode? discountCode = null;
        if (!string.IsNullOrEmpty(discountCodeStr))
            discountCode = await _discountRepo.GetByCode(discountCodeStr, ct);

        var engine = new PricingEngine();
        var result = engine.CalculateFinalPrice(
            distanceKm,
            durationMinutes,
            vehicle.Type,           // ← was missing
            ride.RequestTime.UtcDateTime,
            passenger.LoyaltyPoints,
            discountCode);

        ride.CompleteRide(result, distanceKm, durationMinutes);
        passenger.DeductPoints(result.PointsUsed);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}