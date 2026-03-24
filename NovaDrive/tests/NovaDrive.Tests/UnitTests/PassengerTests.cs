namespace NovaDrive.Tests.UnitTests;

public class PassengerTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Create_SetsInitialValues()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        Assert.Equal(ValidUserId,      passenger.UserId);
        Assert.Equal("Jane Doe",       passenger.FullName);
        Assert.Equal("123 Main St",    passenger.HomeAddress);
        Assert.Equal(0,                passenger.LoyaltyPoints);
        Assert.Equal(PaymentMethod.Unknown, passenger.PreferredPaymentMethod);
    }

    [Fact]
    public void Create_WithEmptyGuid_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            Passenger.Create(Guid.Empty, "Jane Doe", "123 Main St"));
        Assert.Equal(UserDomainException.InvalidUserId, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyFullName_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            Passenger.Create(ValidUserId, "", "123 Main St"));
        Assert.Equal(UserDomainException.InvalidFullName, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyHomeAddress_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            Passenger.Create(ValidUserId, "Jane Doe", ""));
        Assert.Equal(UserDomainException.InvalidHomeAddress, ex.Message);
    }

    [Fact]
    public void UpdateProfile_UpdatesAllFields()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        passenger.UpdateProfile("John Smith", "456 New Ave", PaymentMethod.CreditCard);

        Assert.Equal("John Smith", passenger.FullName);
        Assert.Equal("456 New Ave", passenger.HomeAddress);
        Assert.Equal(PaymentMethod.CreditCard, passenger.PreferredPaymentMethod);
    }

    [Fact]
    public void UpdateProfile_WithEmptyFullName_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        var ex = Assert.Throws<UserDomainException>(() =>
            passenger.UpdateProfile("", "456 New Ave", PaymentMethod.CreditCard));
        Assert.Equal(UserDomainException.InvalidFullName, ex.Message);
    }

    [Fact]
    public void UpdateProfile_WithEmptyHomeAddress_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        var ex = Assert.Throws<UserDomainException>(() =>
            passenger.UpdateProfile("Jane Doe", "", PaymentMethod.CreditCard));
        Assert.Equal(UserDomainException.InvalidHomeAddress, ex.Message);
    }

    [Fact]
    public void UpdateProfile_WithUnknownPaymentMethod_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        var ex = Assert.Throws<UserDomainException>(() =>
            passenger.UpdateProfile("Jane Doe", "123 Main St", PaymentMethod.Unknown));
        Assert.Equal(UserDomainException.InvalidPaymentMethod, ex.Message);
    }

    [Fact]
    public void EarnPoints_AddsPoints()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        passenger.EarnPoints(100);

        Assert.Equal(100, passenger.LoyaltyPoints);
    }

    [Fact]
    public void EarnPoints_AccumulatesAcrossMultipleCalls()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        passenger.EarnPoints(100);
        passenger.EarnPoints(50);

        Assert.Equal(150, passenger.LoyaltyPoints);
    }

    [Fact]
    public void EarnPoints_WithNegativeAmount_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        var ex = Assert.Throws<UserDomainException>(() => passenger.EarnPoints(-10));
        Assert.Equal(UserDomainException.NegativeEarn, ex.Message);
    }

    [Fact]
    public void EarnPoints_WithZero_IsIdempotent()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        passenger.EarnPoints(0);

        Assert.Equal(0, passenger.LoyaltyPoints);
    }

    [Fact]
    public void DeductPoints_SubtractsPoints()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");
        passenger.EarnPoints(200);

        passenger.DeductPoints(50);

        Assert.Equal(150, passenger.LoyaltyPoints);
    }

    [Fact]
    public void DeductPoints_WithNegativeAmount_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");

        var ex = Assert.Throws<UserDomainException>(() => passenger.DeductPoints(-10));
        Assert.Equal(UserDomainException.NegativeDeduct, ex.Message);
    }

    [Fact]
    public void DeductPoints_MoreThanAvailable_Throws()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");
        passenger.EarnPoints(100);

        var ex = Assert.Throws<UserDomainException>(() => passenger.DeductPoints(101));
        Assert.Equal(UserDomainException.InsufficientPoints, ex.Message);
    }

    [Fact]
    public void DeductPoints_ExactBalance_LeavesZero()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");
        passenger.EarnPoints(100);

        passenger.DeductPoints(100);

        Assert.Equal(0, passenger.LoyaltyPoints);
    }

    [Fact]
    public void DeductPoints_WithZero_IsIdempotent()
    {
        var passenger = Passenger.Create(ValidUserId, "Jane Doe", "123 Main St");
        passenger.EarnPoints(100);

        passenger.DeductPoints(0);

        Assert.Equal(100, passenger.LoyaltyPoints);
    }
}