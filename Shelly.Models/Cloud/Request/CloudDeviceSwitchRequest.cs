using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shelly.Models.Cloud.Request
{
    /// <summary>
    /// Action request for switch device status
    /// </summary>
    public class CloudDeviceSwitchRequest
    {
        /// <summary>
        /// The Shelly device id (required)
        /// </summary>
        [JsonPropertyName("id")]
        [JsonRequired]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Channel number (defaults to 0)
        /// </summary>
        [JsonPropertyName("channel")]
        public int Channel { get; set; } = 0;

        /// <summary>
        /// Output state (required)
        /// </summary>
        [JsonPropertyName("on")]
        [JsonRequired]
        public bool On { get; set; }

        /// <summary>
        /// After how many seconds, the state should be set to opposite the value of "on"
        /// </summary>
        [JsonPropertyName("toggle_after")]
        public int? ToggleAfter { get; set; }
    }
}
