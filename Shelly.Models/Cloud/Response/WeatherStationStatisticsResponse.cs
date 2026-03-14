using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Response
{
    /// <summary>
    /// Top-level response from the weather station statistics API endpoint.
    /// </summary>
    public class WeatherStationStatisticsResponse
    {
        /// <summary>
        /// Timezone of the returned data (e.g. "UTC").
        /// </summary>
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        /// <summary>
        /// Aggregation interval used (e.g. "hour", "day").
        /// </summary>
        [JsonPropertyName("interval")]
        public string Interval { get; set; } = string.Empty;

        /// <summary>
        /// Ordered list of historical data points for the requested range.
        /// </summary>
        [JsonPropertyName("history")]
        public List<WeatherStationHistoryEntry> History { get; set; } = new();
    }

    /// <summary>
    /// A single aggregated data point returned by the weather station statistics API.
    /// Numeric fields are nullable because the device may not report all sensors.
    /// </summary>
    public class WeatherStationHistoryEntry
    {
        /// <summary>
        /// Start of the aggregation interval.
        /// </summary>
        [JsonPropertyName("datetime")]
        public DateTime Datetime { get; set; }

        /// <summary>
        /// Minimum temperature recorded during the interval (°C).
        /// </summary>
        [JsonPropertyName("min_temperature")]
        public double? MinTemperature { get; set; }

        /// <summary>
        /// Maximum temperature recorded during the interval (°C).
        /// </summary>
        [JsonPropertyName("max_temperature")]
        public double? MaxTemperature { get; set; }

        /// <summary>
        /// Average relative humidity during the interval (%).
        /// </summary>
        [JsonPropertyName("humidity")]
        public double? Humidity { get; set; }

        /// <summary>
        /// Total precipitation during the interval (mm).
        /// </summary>
        [JsonPropertyName("precipitation")]
        public double? Precipitation { get; set; }

        /// <summary>
        /// UV index during the interval.
        /// </summary>
        [JsonPropertyName("uv")]
        public double? Uv { get; set; }

        /// <summary>
        /// Minimum atmospheric pressure during the interval (hPa).
        /// </summary>
        [JsonPropertyName("min_pressure")]
        public double? MinPressure { get; set; }

        /// <summary>
        /// Maximum atmospheric pressure during the interval (hPa).
        /// </summary>
        [JsonPropertyName("max_pressure")]
        public double? MaxPressure { get; set; }

        /// <summary>
        /// Average illuminance during the interval (lux).
        /// </summary>
        [JsonPropertyName("illuminance")]
        public double? Illuminance { get; set; }

        /// <summary>
        /// Average wind speed during the interval (m/s). Null when not reported.
        /// </summary>
        [JsonPropertyName("wind_speed")]
        public double? WindSpeed { get; set; }

        /// <summary>
        /// True when the device reported no data for this interval (gap in history).
        /// </summary>
        [JsonPropertyName("missing")]
        public bool? Missing { get; set; }
    }
}
