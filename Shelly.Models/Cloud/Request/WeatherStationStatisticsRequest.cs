namespace Shelly.Models.Cloud.Request;

/// <summary>
/// Request parameters for retrieving weather station historical statistics.
/// </summary>
public record WeatherStationStatisticsRequest
{
    /// <summary>The Shelly device identifier.</summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>Start of the date range (inclusive).</summary>
    public DateTime DateFrom { get; init; }

    /// <summary>End of the date range (inclusive).</summary>
    public DateTime DateTo { get; init; }
}
