namespace NovaDrive.Domain.Entities;

public class Ride
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid PassengerId { get; init; }
    public required Guid VehicleId { get; set; }
    public string Departure { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; private set; }
    public int DurationMinutes { get; private set; }
    public RideStatus Status { get; private set; } = RideStatus.Unknown;
    public DateTimeOffset RequestTime { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedTime { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal VatAmount { get; private set; }
    public decimal FinalPrice { get; private set; } 
    public bool IsPaid { get; private set; }
    public decimal LoyaltyDiscountApplied { get; private set; }
    public int LoyaltyPointsUsed { get; private set; }
    public decimal CodeDiscountApplied { get; private set; }

    /// <summary>
    /// Initializes the ride by setting its status to Requested.
    /// </summary>
    /// <exception cref="RideDomainException.AlreadyInitialized">Thrown if the ride is already initialized or has a status other than Unknown.</exception>
    public void RequestRide()
    {
        if (Status != RideStatus.Unknown) 
            throw new RideDomainException(RideDomainException.AlreadyInitialized);
            
        Status = RideStatus.Requested;
    }

    /// <summary>
    /// Transitions the ride to the EnRoute status.
    /// </summary>
    /// <exception cref="RideDomainException.NotRequested">Thrown if the ride is not currently in the Requested state.</exception>
    public void StartRide()
    {
        if (Status != RideStatus.Requested)
            throw new RideDomainException(RideDomainException.NotRequested);

        Status = RideStatus.EnRoute;
    }

    /// <summary>
    /// Completes the ride, applies the pricing breakdown, and sets the status to Completed.
    /// </summary>
    /// <param name="result">The calculated pricing breakdown from the PricingEngine.</param>
    /// <exception cref="RideDomainException">Thrown if the ride is not currently EnRoute.</exception>
    public void CompleteRide(PricingResult result, double distanceKm, int durationMinutes)
    {
        if (Status != RideStatus.EnRoute)
            throw new RideDomainException(RideDomainException.NotEnRoute);

        DistanceKm = distanceKm;
        DurationMinutes = durationMinutes;
        CompletedTime = DateTimeOffset.UtcNow;

        NetAmount = result.NetAmount;
        VatAmount = result.VatAmount;
        FinalPrice = result.TotalGross;
        LoyaltyDiscountApplied = result.LoyaltyDiscountApplied;
        LoyaltyPointsUsed = result.PointsUsed;
        CodeDiscountApplied = result.CodeDiscountApplied;

        Status = RideStatus.Completed;
    }
    

    /// <summary>
    /// Cancels the ride if it hasn't been finished yet.
    /// </summary>
    /// <exception cref="RideDomainException.AlreadyCompleted">Thrown if the ride is already marked as Completed.</exception>
    public void CancelRide()
    {
        if (Status == RideStatus.Completed)
            throw new RideDomainException(RideDomainException.AlreadyCompleted);

        if (Status == RideStatus.Canceled) return;

        Status = RideStatus.Canceled;
    }

    public void MarkAsPaid()
    {
        if (Status != RideStatus.Completed)
            throw new RideDomainException(RideDomainException.NotCompleted);

        IsPaid = true;
    }

}

