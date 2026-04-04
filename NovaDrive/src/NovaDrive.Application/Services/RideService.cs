namespace NovaDrive.Application.Services;


public interface IRideService
{
    Task<RideResponse> RequestRide(RequestRideRequest request, CancellationToken ct = default);
    Task<RideResponse> StartRide(Guid rideId, CancellationToken ct = default);
    Task<RideResponse> CompleteRide(CompleteRideRequest request, CancellationToken ct = default);
    Task<RideResponse> CancelRide(Guid rideId, CancellationToken ct = default);
    Task<IEnumerable<RideResponse>> GetRidesByPassenger(Guid passengerId, CancellationToken ct = default);
}
public sealed class RideService : IRideService
{
    private readonly IRideRepository          _rideRepo;
    private readonly IPassengerRepository     _passengerRepo;
    private readonly IVehicleRepository       _vehicleRepo;
    private readonly IDiscountCodeRepository  _discountRepo;
    private readonly IUnitOfWork              _unitOfWork;

    public RideService(
        IRideRepository         rideRepo,
        IPassengerRepository    passengerRepo,
        IVehicleRepository      vehicleRepo,
        IDiscountCodeRepository discountRepo,
        IUnitOfWork             unitOfWork)
    {
        _rideRepo      = rideRepo;
        _passengerRepo = passengerRepo;
        _vehicleRepo   = vehicleRepo;
        _discountRepo  = discountRepo;
        _unitOfWork    = unitOfWork;
    }

    public async Task<RideResponse> RequestRide(
        RequestRideRequest request, CancellationToken ct = default)
    {
        await _passengerRepo.GetById(request.PassengerId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        var activeVehicles    = await _vehicleRepo.GetAllActive(ct);
        var passengerLocation = new GpsLocation(request.PassengerLatitude, request.PassengerLongitude);
        var vehicle           = new RideMatchingService()
                                    .FindBestMatch(passengerLocation, activeVehicles, request.EstimatedDistanceKm)
                                ?? throw new InvalidOperationException(
                                    "No available vehicle found within range. Please try again shortly.");

        var ride = new Ride
        {
            PassengerId = request.PassengerId,
            VehicleId   = vehicle.Id,
            Departure   = request.Departure,
            Destination = request.Destination
        };
        ride.RequestRide();

        await _rideRepo.Add(ride, ct);
        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    public async Task<RideResponse> StartRide(Guid rideId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(rideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);

        ride.StartRide();
        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    /// <summary>
    /// Completes a ride: runs PricingEngine, deducts loyalty points spent,
    /// awards points earned, and persists everything in one SaveChanges call.
    /// </summary>
    public async Task<RideResponse> CompleteRide(CompleteRideRequest request, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(request.RideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);

        var passenger = await _passengerRepo.GetById(ride.PassengerId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);

        var vehicle = await _vehicleRepo.GetById(ride.VehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        DiscountCode? discountCode = null;
        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            discountCode = await _discountRepo.GetByCode(request.DiscountCode, ct);

        var engine = new PricingEngine();
        var result = engine.CalculateFinalPrice(
            distanceKm:      request.ActualDistanceKm,
            durationMinutes: request.ActualDurationMinutes,
            vehicleType:     vehicle.Type,
            requestTime:     ride.RequestTime.UtcDateTime,
            availablePoints: passenger.LoyaltyPoints,
            code:            discountCode);

        ride.CompleteRide(result, request.ActualDistanceKm, request.ActualDurationMinutes);
        passenger.DeductPoints(result.PointsUsed);
        passenger.EarnPoints(engine.CalculateEarnedPoints(result.TotalGross));

        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    public async Task<RideResponse> CancelRide(Guid rideId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(rideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);

        ride.CancelRide();
        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    public async Task<IEnumerable<RideResponse>> GetRidesByPassenger(
        Guid passengerId, CancellationToken ct = default)
        => (await _rideRepo.GetByPassengerId(passengerId, ct)).Select(ride => ride.ToResponse());

}
