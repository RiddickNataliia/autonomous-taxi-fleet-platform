namespace NovaDrive.Tests.UnitTests;

public class RideTests
{
    //helpers:
    private static Ride CreateRide() => new()
    {
        PassengerId = Guid.NewGuid(),
        VehicleId   = Guid.NewGuid(),
        Departure   = "Brussels",
        Destination = "Ghent"
    };

    private static PricingResult CreatePricingResult(
        decimal net   = 19.50m,
        decimal vat   =  4.10m,
        decimal gross = 23.60m) => new(
            NetAmount:              net,
            VatAmount:              vat,
            TotalGross:             gross,
            LoyaltyDiscountApplied: 0m,
            CodeDiscountApplied:    0m,
            PointsUsed:             0);

    private static Ride CreateEnRouteRide()
    {
        var ride = CreateRide();
        ride.RequestRide();
        ride.StartRide();
        return ride;
    }

    private static Ride CreateCompletedRide()
    {
        var ride = CreateEnRouteRide();
        ride.CompleteRide(CreatePricingResult(), distanceKm: 10.0, durationMinutes: 20);
        return ride;
    }






    [Fact]
    public void NewRide_HasUnknownStatus()
    {
        var ride = CreateRide();
        Assert.Equal(RideStatus.Unknown, ride.Status);
    }

    [Fact]
    public void NewRide_HasZeroPricing()
    {
        var ride = CreateRide();
        Assert.Equal(0m,  ride.NetAmount);
        Assert.Equal(0m,  ride.VatAmount);
        Assert.Equal(0m,  ride.FinalPrice);
        Assert.Equal(0m,  ride.LoyaltyDiscountApplied);
        Assert.Equal(0m,  ride.CodeDiscountApplied);
        Assert.Equal(0,   ride.LoyaltyPointsUsed);
        Assert.Equal(0.0, ride.DistanceKm);
        Assert.Equal(0,   ride.DurationMinutes);
    }

    [Fact]
    public void NewRide_IsNotPaid()
    {
        var ride = CreateRide();
        Assert.False(ride.IsPaid);
    }

    [Fact]
    public void NewRide_CompletedTimeIsNull()
    {
        var ride = CreateRide();
        Assert.Null(ride.CompletedTime);
    }


    [Fact]
    public void RequestRide_FromUnknown_SetsStatusToRequested()
    {
        var ride = CreateRide();
        ride.RequestRide();
        Assert.Equal(RideStatus.Requested, ride.Status);
    }

    [Fact]
    public void RequestRide_WhenAlreadyRequested_Throws()
    {
        var ride = CreateRide();

        //arrange
        ride.RequestRide();
        //act + assert
        var ex = Assert.Throws<RideDomainException>(ride.RequestRide);
        Assert.Equal(RideDomainException.AlreadyInitialized, ex.Message);
    }

    [Fact]
    public void RequestRide_WhenEnRoute_Throws()
    {
        //arrange
        var ride = CreateEnRouteRide();
        //act + assert
        var ex = Assert.Throws<RideDomainException>(ride.RequestRide);
        Assert.Equal(RideDomainException.AlreadyInitialized, ex.Message);
    }

    [Fact]
    public void RequestRide_WhenCompleted_Throws()
    {
        var ride = CreateCompletedRide();

        var ex = Assert.Throws<RideDomainException>(ride.RequestRide);
        Assert.Equal(RideDomainException.AlreadyInitialized, ex.Message);
    }

    [Fact]
    public void RequestRide_WhenCanceled_Throws()
    {
        var ride = CreateRide();
        ride.RequestRide();
        ride.CancelRide();

        var ex = Assert.Throws<RideDomainException>(ride.RequestRide);
        Assert.Equal(RideDomainException.AlreadyInitialized, ex.Message);
    }


    [Fact]
    public void StartRide_FromRequested_SetsStatusToEnRoute()
    {
        //arrange
        var ride = CreateRide();
        ride.RequestRide();
        //act
        ride.StartRide();
        //assert
        Assert.Equal(RideStatus.EnRoute, ride.Status);
    }

    [Fact]
    public void StartRide_FromUnknown_Throws()
    {
        var ride = CreateRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.StartRide());
        Assert.Equal(RideDomainException.NotRequested, ex.Message);
    }

    [Fact]
    public void StartRide_WhenAlreadyEnRoute_Throws()
    {
        var ride = CreateEnRouteRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.StartRide());
        Assert.Equal(RideDomainException.NotRequested, ex.Message);
    }

    [Fact]
    public void StartRide_WhenCompleted_Throws()
    {
        var ride = CreateCompletedRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.StartRide());
        Assert.Equal(RideDomainException.NotRequested, ex.Message);
    }


    [Fact]
    public void CompleteRide_FromEnRoute_SetsStatusToCompleted()
    {
        var ride = CreateEnRouteRide();

        ride.CompleteRide(CreatePricingResult(), distanceKm: 10.0, durationMinutes: 20);

        Assert.Equal(RideStatus.Completed, ride.Status);
    }

    [Fact]
    public void CompleteRide_StoresPricingBreakdown()
    {
        var ride   = CreateEnRouteRide();
        var result = new PricingResult(
            NetAmount:              16.00m,
            VatAmount:               3.36m,
            TotalGross:             19.36m,
            LoyaltyDiscountApplied:  2.00m,
            CodeDiscountApplied:     1.00m,
            PointsUsed:               200);

        ride.CompleteRide(result, distanceKm: 12.5, durationMinutes: 25);

        Assert.Equal(16.00m, ride.NetAmount);
        Assert.Equal(3.36m,  ride.VatAmount);
        Assert.Equal(19.36m, ride.FinalPrice);
        Assert.Equal(2.00m,  ride.LoyaltyDiscountApplied);
        Assert.Equal(1.00m,  ride.CodeDiscountApplied);
        Assert.Equal(200,    ride.LoyaltyPointsUsed);
        Assert.Equal(12.5,   ride.DistanceKm);
        Assert.Equal(25,     ride.DurationMinutes);
    }

    [Fact]
    public void CompleteRide_SetsCompletedTime()
    {
        var ride   = CreateEnRouteRide();
        var before = DateTimeOffset.UtcNow;

        ride.CompleteRide(CreatePricingResult(), distanceKm: 10.0, durationMinutes: 20);

        Assert.NotNull(ride.CompletedTime);
        Assert.True(ride.CompletedTime >= before);
        Assert.True(ride.CompletedTime <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CompleteRide_FromRequested_Throws()
    {
        var ride = CreateRide();
        ride.RequestRide();

        var ex = Assert.Throws<RideDomainException>(
            () => ride.CompleteRide(CreatePricingResult(), 10.0, 20));
        Assert.Equal(RideDomainException.NotEnRoute, ex.Message);
    }

    [Fact]
    public void CompleteRide_FromUnknown_Throws()
    {
        var ride = CreateRide();

        var ex = Assert.Throws<RideDomainException>(
            () => ride.CompleteRide(CreatePricingResult(), 10.0, 20));
        Assert.Equal(RideDomainException.NotEnRoute, ex.Message);
    }

    [Fact]
    public void CompleteRide_WhenAlreadyCompleted_Throws()
    {
        var ride = CreateCompletedRide();

        var ex = Assert.Throws<RideDomainException>(
            () => ride.CompleteRide(CreatePricingResult(), 10.0, 20));
        Assert.Equal(RideDomainException.NotEnRoute, ex.Message);
    }


    [Fact]
    public void CancelRide_FromRequested_SetsStatusToCanceled()
    {
        var ride = CreateRide();
        ride.RequestRide();

        ride.CancelRide();

        Assert.Equal(RideStatus.Canceled, ride.Status);
    }

    [Fact]
    public void CancelRide_FromEnRoute_ThrowsDomainException()
    {
        var ride = CreateEnRouteRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.CancelRide());
        Assert.Equal(RideDomainException.CannotCancelEnRoute, ex.Message);
    }

    [Fact]
    public void CancelRide_WhenCompleted_Throws()
    {
        var ride = CreateCompletedRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.CancelRide());
        Assert.Equal(RideDomainException.AlreadyCompleted, ex.Message);
    }

    [Fact]
    public void CancelRide_WhenEnRoute_Throws()
    {
        var ride = CreateEnRouteRide();
        
        var ex = Assert.Throws<RideDomainException>(() => ride.CancelRide());
        Assert.Equal(RideDomainException.CannotCancelEnRoute, ex.Message);
    }

    [Fact]
    public void CancelRide_WhenAlreadyCanceled_IsIdempotent()
    {
        var ride = CreateRide();
        ride.RequestRide();
        ride.CancelRide();

        ride.CancelRide();

        Assert.Equal(RideStatus.Canceled, ride.Status);
    }


    [Fact]
    public void MarkAsPaid_WhenCompleted_SetsIsPaidTrue()
    {
        var ride = CreateCompletedRide();

        ride.MarkAsPaid();

        Assert.True(ride.IsPaid);
    }

    [Fact]
    public void MarkAsPaid_WhenEnRoute_Throws()
    {
        var ride = CreateEnRouteRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.MarkAsPaid());
        Assert.Equal(RideDomainException.NotCompleted, ex.Message);
    }

    [Fact]
    public void MarkAsPaid_WhenRequested_Throws()
    {
        var ride = CreateRide();
        ride.RequestRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.MarkAsPaid());
        Assert.Equal(RideDomainException.NotCompleted, ex.Message);
    }

    [Fact]
    public void MarkAsPaid_WhenCanceled_Throws()
    {
        var ride = CreateRide();
        ride.RequestRide();
        ride.CancelRide();

        var ex = Assert.Throws<RideDomainException>(() => ride.MarkAsPaid());
        Assert.Equal(RideDomainException.NotCompleted, ex.Message);
    }


    [Fact]
    public void FullLifecycle_Unknown_Requested_EnRoute_Completed_Paid()
    {
        var ride   = CreateRide();
        var result = CreatePricingResult();

        Assert.Equal(RideStatus.Unknown,   ride.Status);
        ride.RequestRide();
        Assert.Equal(RideStatus.Requested, ride.Status);
        ride.StartRide();
        Assert.Equal(RideStatus.EnRoute,   ride.Status);
        ride.CompleteRide(result, distanceKm: 10.0, durationMinutes: 20);
        Assert.Equal(RideStatus.Completed, ride.Status);
        Assert.False(ride.IsPaid);
        ride.MarkAsPaid();
        Assert.True(ride.IsPaid);
    }
}