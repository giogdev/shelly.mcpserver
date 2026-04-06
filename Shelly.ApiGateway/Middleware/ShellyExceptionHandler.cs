using Microsoft.AspNetCore.Diagnostics;
using Shelly.Models;
using Shelly.Models.Exceptions;

namespace Shelly.ApiGateway.Middleware;

/// <summary>
/// Maps domain exceptions to HTTP status codes for all API endpoints.
/// Registered via app.UseExceptionHandler() in Program.cs.
/// </summary>
public sealed class ShellyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ShellyCloudApiException { ErrorKey: "device_not_found" } ex
                => (StatusCodes.Status404NotFound, ex.Message),

            ShellyCloudApiException ex
                => (StatusCodes.Status400BadRequest, ex.Message),

            HttpRequestException
                => (StatusCodes.Status502BadGateway, "Upstream service unavailable."),

            _
                => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ApiErrorResponse(message), cancellationToken);
        return true;
    }
}
