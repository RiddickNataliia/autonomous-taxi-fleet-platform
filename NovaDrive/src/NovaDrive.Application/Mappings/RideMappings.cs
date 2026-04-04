namespace NovaDrive.Application.Mappings;

public static class RideMappings
{
    
    public static RideResponse ToResponse(this Ride r) => new(
        RideId:                  r.Id,
        PassengerId:             r.PassengerId,
        VehicleId:               r.VehicleId,
        Departure:               r.Departure,
        Destination:             r.Destination,
        Status:                  r.Status.ToString(),
        RequestTime:             r.RequestTime,
        CompletedTime:           r.CompletedTime,
        DistanceKm:              r.DistanceKm,
        DurationMinutes:         r.DurationMinutes,
        NetAmount:               r.NetAmount,
        VatAmount:               r.VatAmount,
        FinalPrice:              r.FinalPrice,
        LoyaltyDiscountApplied:  r.LoyaltyDiscountApplied,
        CodeDiscountApplied:     r.CodeDiscountApplied,
        LoyaltyPointsUsed:       r.LoyaltyPointsUsed,
        IsPaid:                  r.IsPaid);

}