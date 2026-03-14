using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Response
{
    /// <summary>
    /// Top-level response from the power consumption statistics API endpoint.
    /// </summary>
    public class PowerConsumptionStatisticsResponse
    {
        /// <summary>
        /// Timezone of the returned data (e.g. "Europe/Rome").
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
        public List<PowerConsumptionHistoryEntry> History { get; set; } = new();
    }

    /// <summary>
    /// A single aggregated data point returned by the power consumption statistics API.
    /// Numeric fields are nullable because the device may not report all channels.
    /// Datetime is kept as string because the API returns "yyyy-MM-dd HH:mm:ss" (no T, no Z),
    /// which is not directly parseable by the default System.Text.Json DateTime converter.
    /// </summary>
    public class PowerConsumptionHistoryEntry
    {
        /// <summary>
        /// Start of the aggregation interval in "yyyy-MM-dd HH:mm:ss" format.
        /// </summary>
        [JsonPropertyName("datetime")]
        public string? Datetime { get; set; }

        /// <summary>
        /// Average voltage during the interval (V).
        /// </summary>
        [JsonPropertyName("voltage")]
        public double? Voltage { get; set; }

        /// <summary>
        /// Energy consumed during the interval (Wh).
        /// </summary>
        [JsonPropertyName("consumption")]
        public double? Consumption { get; set; }

        /// <summary>
        /// Energy returned to the grid during the interval (Wh). Relevant for prosumer setups.
        /// </summary>
        [JsonPropertyName("reversed")]
        public double? Reversed { get; set; }

        /// <summary>
        /// Cost of energy consumed during the interval, in the configured currency.
        /// </summary>
        [JsonPropertyName("cost")]
        public double? Cost { get; set; }

        /// <summary>
        /// Declared purpose of the device (e.g. "other", "heating", "lighting").
        /// </summary>
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Tariff identifier applied during the interval.
        /// </summary>
        [JsonPropertyName("tariff_id")]
        public string? TariffId { get; set; }

        /// <summary>
        /// True when the device reported no data for this interval (gap in history).
        /// </summary>
        [JsonPropertyName("missing")]
        public bool? Missing { get; set; }
    }
}
