namespace NovaDrive.Application.Mappings;

public static class PassengerMappings
{
    public static PassengerResponse ToResponse(this Passenger p) => new(
        PassengerId:            p.Id,
        UserId:                 p.UserId,
        FullName:               p.FullName,
        HomeAddress:            p.HomeAddress,
        PreferredPaymentMethod: p.PreferredPaymentMethod.ToString(),
        LoyaltyPoints:          p.LoyaltyPoints);
}