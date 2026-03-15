namespace Shelly.Services.Exceptions;

/// <summary>
/// Thrown when the Shelly Cloud API rejects a request at the application level
/// (e.g. device_not_found) regardless of the HTTP status code returned.
/// </summary>
public class ShellyCloudApiException : Exception
{
    /// <summary>
    /// The error key as returned by the Shelly Cloud API (e.g. "device_not_found").
    /// Used by the presentation layer to map to the appropriate HTTP status code.
    /// </summary>
    public string ErrorKey { get; }

    public ShellyCloudApiException(string errorKey, string message)
        : base(message)
    {
        ErrorKey = errorKey;
    }
}
