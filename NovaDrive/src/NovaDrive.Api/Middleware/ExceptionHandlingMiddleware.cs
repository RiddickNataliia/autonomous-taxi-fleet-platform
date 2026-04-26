namespace NovaDrive.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions and converts them to RFC 9457 ProblemDetails
/// responses. This keeps error handling out of every endpoint handler and ensures
/// a consistent JSON error contract for API consumers.
///
/// Mapping strategy:
///   DomainException subtypes  → 422 Unprocessable Entity  (business rule violated)
///   KeyNotFoundException       → 404 Not Found
///   UnauthorizedAccessException→ 403 Forbidden
///   Everything else            → 500 Internal Server Error (detail hidden in Production)
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            System.Collections.Generic.KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,           "Access denied"),
            _                              => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        // Log 5xx as errors, 4xx as warnings to keeps the noise down
        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning(ex, "Handled exception ({Status}): {Message}", statusCode, ex.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            // Only expose detail in Development to avoid leaking internals in Production
            Detail = _env.IsDevelopment() || statusCode < 500 ? ex.Message : null,
            Instance = context.Request.Path
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = statusCode;
        // Serialize the ProblemDetails object to JSON and write it to the response body
        await context.Response.WriteAsJsonAsync(problem);
    }
}

/// <summary>Extension method for cleaner registration in Program.cs.</summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
