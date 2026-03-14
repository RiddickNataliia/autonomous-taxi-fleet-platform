namespace NovaDrive.Application.Services;

public class RideService : IRideService
{
    private readonly IRideRepository _rideRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDiscountRepository _discountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RideService(
        IRideRepository rideRepo, 
        IUserRepository userRepo, 
        IDiscountRepository discountRepo,
        IUnitOfWork unitOfWork)
    {
        _rideRepo = rideRepo;
        _userRepo = userRepo;
        _discountRepo = discountRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task CompleteRideAsync(Guid rideId, decimal calculatedAmount, string? discountCodeStr)
    {
        // 1. Fetch data from repositories
        var ride = await _rideRepo.GetByIdAsync(rideId) 
            ?? throw new Exception("Ride not found");
            
        var passenger = await _userRepo.GetByIdAsync(ride.PassengerId) 
            ?? throw new Exception("Passenger not found");
        
        // 2. Fetch Discount Code if one was provided
        DiscountCode? discountCode = null;
        if (!string.IsNullOrEmpty(discountCodeStr))
        {
            discountCode = await _discountRepo.GetByCodeAsync(discountCodeStr);
        }

        // 3. Run the Domain Service (The Engine)
        var engine = new PricingEngine();
        var result = engine.CalculateFinalPrice(calculatedAmount, passenger.LoyaltyPoints, discountCode);

        // 4. Update the Ride Entity (Business Rule check happens here)
        ride.CompleteRide(result);

        // 5. Update the Passenger (Loyalty Points deduction)
        passenger.DeductPoints(result.PointsUsed);

        // 6. Persist everything to the database in one transaction
        await _unitOfWork.SaveChangesAsync();
    }
}