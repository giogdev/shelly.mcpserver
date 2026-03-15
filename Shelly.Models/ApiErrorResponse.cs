namespace Shelly.Models;

/// <summary>
/// Standard error response body returned by all API endpoints.
/// </summary>
/// <param name="Message">Human-readable description of the error.</param>
public record ApiErrorResponse(string Message);
