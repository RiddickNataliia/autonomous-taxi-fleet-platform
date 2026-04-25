namespace NovaDrive.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? Auth0UserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? Email(this ClaimsPrincipal user)
    => user.FindFirst(ClaimTypes.Email)?.Value;

    public static string? Role(this ClaimsPrincipal user)
        => user.FindFirst("https://novadrive/role")?.Value;
}