namespace NovaDrive.Application.Services;


public interface IRideService
{
    Task<RideResponse> RequestRide(RequestRideRequest request, CancellationToken ct = default);
    Task<RideResponse> StartRide(Guid rideId, CancellationToken ct = default);
    Task<RideResponse> CompleteRide(CompleteRideRequest request, CancellationToken ct = default);
    Task<RideResponse> CancelRide(Guid rideId, Guid passengerId, CancellationToken ct = default);
    Task<IEnumerable<RideResponse>> GetRidesByPassenger(Guid passengerId, CancellationToken ct = default);
    Task<IEnumerable<RideResponse>> GetAllRides(CancellationToken ct = default);
    Task<RideResponse?> GetActiveRide(Guid passengerId, CancellationToken ct = default);
    Task<RideResponse?> GetPendingRideForVehicle(Guid vehicleId, CancellationToken ct = default);
}
public sealed class RideService : IRideService
{
    private readonly IRideRepository          _rideRepo;
    private readonly IPassengerRepository     _passengerRepo;
    private readonly IVehicleRepository       _vehicleRepo;
    private readonly IDiscountCodeRepository  _discountRepo;
    private readonly IUnitOfWork              _unitOfWork;
    private readonly IPricingEngine           _pricingEngine;
    private readonly IRideMatchingService     _matchingService;

    public RideService(
        IRideRepository         rideRepo,
        IPassengerRepository    passengerRepo,
        IVehicleRepository      vehicleRepo,
        IDiscountCodeRepository discountRepo,
        IUnitOfWork             unitOfWork,
        IPricingEngine          pricingEngine,
        IRideMatchingService    matchingService)
    {
        _rideRepo      = rideRepo;
        _passengerRepo = passengerRepo;
        _vehicleRepo   = vehicleRepo;
        _discountRepo  = discountRepo;
        _unitOfWork    = unitOfWork;
        _pricingEngine = pricingEngine;
        _matchingService = matchingService;
    }

    public async Task<RideResponse> RequestRide(
        RequestRideRequest request, CancellationToken ct = default)
    {
        // verify passenger exists before assigning a vehicle
        _ = await _passengerRepo.GetById(request.PassengerId, ct)
            ?? throw new KeyNotFoundException(UserDomainException.PassengerNotFound);
        // ensure passenger doesn't already have an active ride before trying to find a match
        var existingActive = await _rideRepo.GetActiveByPassengerId(request.PassengerId, ct);
            if (existingActive is not null)
                throw new InvalidOperationException("You already have an active ride.");

        var activeVehicles    = await _vehicleRepo.GetAllActive(ct);
        var passengerLocation = new GpsLocation(request.PassengerLatitude, request.PassengerLongitude);
        var vehicle = _matchingService.FindBestMatch(passengerLocation, activeVehicles, request.EstimatedDistanceKm, request.PreferredVehicleType)
                                ?? throw new InvalidOperationException(
                                    request.PreferredVehicleType is null
                                        ? "No available vehicle found within range. Please try again shortly."
                                        : $"No available {request.PreferredVehicleType} vehicle found within range. Try a different type or wait.");
        var ride = new Ride
        {
            PassengerId = request.PassengerId,
            VehicleId   = vehicle.Id,
            Departure   = request.Departure,
            Destination = request.Destination
        };
        ride.RequestRide();
        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            ride.SetDiscountCode(request.DiscountCode);

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

        var codeToUse = !string.IsNullOrWhiteSpace(request.DiscountCode)
            ? request.DiscountCode
            : ride.DiscountCodeUsed;

        DiscountCode? discountCode = null;
        if (!string.IsNullOrWhiteSpace(codeToUse))
            discountCode = await _discountRepo.GetByCode(codeToUse, ct);

        var result = _pricingEngine.CalculateFinalPrice(
            distanceKm:      request.ActualDistanceKm,
            durationMinutes: request.ActualDurationMinutes,
            vehicleType:     vehicle.Type,
            requestTime:     ride.RequestTime.UtcDateTime,
            availablePoints: passenger.LoyaltyPoints,
            code:            discountCode);

        ride.CompleteRide(result, request.ActualDistanceKm, request.ActualDurationMinutes);
        passenger.DeductPoints(result.PointsUsed);
        passenger.EarnPoints(_pricingEngine.CalculateEarnedPoints(result.TotalGross));

        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    /// <summary>
    /// Cancels a ride if it's not already en route. Only the passenger who requested the ride can cancel it.
    /// </summary>
    /// <param name="rideId"></param>
    /// <param name="passengerId"></param>
    /// <param name="ct"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<RideResponse> CancelRide(Guid rideId, Guid passengerId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(rideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);

        if (ride.PassengerId != passengerId)
            throw new UnauthorizedAccessException(RideDomainException.NotOwner);

        ride.CancelRide();
        await _unitOfWork.SaveChanges(ct);
        return ride.ToResponse();
    }

    /// <summary>
    /// Returns all rides for a given passenger.
    /// </summary>
    /// <param name="passengerId"></param>
    /// <param name="ct"></param>
    public async Task<IEnumerable<RideResponse>> GetRidesByPassenger(
        Guid passengerId, CancellationToken ct = default)
        => (await _rideRepo.GetByPassengerId(passengerId, ct)).Select(ride => ride.ToResponse());

    /// <summary>
    /// Returns all rides in the system. 
    /// </summary>
    /// <param name="ct"></param>
    public async Task<IEnumerable<RideResponse>> GetAllRides(CancellationToken ct = default)
        => (await _rideRepo.GetAll(ct)).Select(r => r.ToResponse());

    /// <summary>
    /// Returns the active ride for a given passenger, if one exists. 
    /// </summary>
    /// <param name="passengerId"></param>
    /// <param name="ct"></param>
    public async Task<RideResponse?> GetActiveRide(Guid passengerId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetActiveByPassengerId(passengerId, ct);
        return ride?.ToResponse();
    }

    /// <summary>
    /// Returns the pending ride for a given vehicle, if one exists. 
    /// This is used by the vehicle to check if it has an assigned ride when it comes online or finishes a ride.
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <param name="ct"></param>
    public async Task<RideResponse?> GetPendingRideForVehicle(Guid vehicleId, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetPendingByVehicleId(vehicleId, ct);
        return ride?.ToResponse();
    }

}
