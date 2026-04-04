namespace NovaDrive.Application.Mappings;

public static class DiscountCodeMappings
{
    public static DiscountCodeResponse ToResponse(this DiscountCode d) => new(
        Id:               d.Id,
        Code:             d.Code,
        Type:             d.Type.ToString(),
        Value:            d.Value,
        MinimumRideValue: d.MinimumRideValue,
        ExpirationDate:   d.ExpirationDate,
        IsActive:         d.IsActive);
}