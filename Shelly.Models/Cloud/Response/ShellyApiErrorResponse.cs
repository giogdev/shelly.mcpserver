using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Response
{
    /// <summary>
    /// Represents the error envelope returned by the Shelly Cloud API when a request
    /// is accepted at the HTTP level but rejected at the application level (isok: false).
    /// </summary>
    public class ShellyApiErrorResponse
    {
        /// <summary>
        /// False when the API rejected the request despite a 2xx HTTP status.
        /// </summary>
        [JsonPropertyName("isok")]
        public bool IsOk { get; set; }

        /// <summary>
        /// Map of field names to error messages provided by the API.
        /// </summary>
        [JsonPropertyName("errors")]
        public Dictionary<string, string>? Errors { get; set; }
    }
}
