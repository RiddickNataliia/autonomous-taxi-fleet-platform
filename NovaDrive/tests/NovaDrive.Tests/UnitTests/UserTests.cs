namespace NovaDrive.Tests.UnitTests;

public class UserTests
{
    [Fact]
    public void Create_SetsInitialValues()
    {
        var before = DateTimeOffset.UtcNow;
        var user   = User.Create("jane@example.com", "hashedpassword", UserRole.Passenger);

        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal(UserRole.Passenger, user.Role);
        Assert.Null(user.LastLoginAt);
        Assert.True(user.CreatedAt >= before);
    }

    [Fact]
    public void Create_WithEmptyEmail_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            User.Create("", "hashedpassword", UserRole.Passenger));
        Assert.Equal(UserDomainException.InvalidEmail, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyPassword_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            User.Create("jane@example.com", "", UserRole.Passenger));
        Assert.Equal(UserDomainException.InvalidPassword, ex.Message);
    }

    [Fact]
    public void Create_WithUnknownRole_Throws()
    {
        var ex = Assert.Throws<UserDomainException>(() =>
            User.Create("jane@example.com", "hashedpassword", UserRole.Unknown));
        Assert.Equal(UserDomainException.InvalidRole, ex.Message);
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user   = User.Create("jane@example.com", "hashedpassword", UserRole.Passenger);
        var before = DateTimeOffset.UtcNow;

        user.RecordLogin();

        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= before);
    }

    [Fact]
    public void RecordLogin_CalledTwice_UpdatesTimestamp()
    {
        var user = User.Create("jane@example.com", "hashedpassword", UserRole.Passenger);
        user.RecordLogin();
        var firstLogin = user.LastLoginAt;

        Thread.Sleep(10);
        user.RecordLogin();

        Assert.True(user.LastLoginAt > firstLogin);
    }
}