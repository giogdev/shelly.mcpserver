using System;

namespace Shelly.Models.Cloud.Request
{
    /// <summary>
    /// Request parameters for retrieving power consumption historical statistics.
    /// </summary>
    public class PowerConsumptionStatisticsRequest
    {
        /// <summary>
        /// The Shelly device identifier.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Start of the date range (inclusive).
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// End of the date range (inclusive).
        /// </summary>
        public DateTime DateTo { get; set; }
    }
}
