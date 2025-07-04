using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Request
{
    /// <summary>
    /// Model to request info about device
    /// </summary>
    public class CloudDeviceRequest
    {
        /// <summary>
        /// List of device ids (max 10 items).
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(10)]
        [JsonPropertyName("ids")]
        public string[] Ids { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List containing any of: "status", "settings". Additional data to fetch.
        /// </summary>
        [JsonPropertyName("select")]
        public string[]? Select { get; set; }

        /// <summary>
        /// Fetch only specified properties of the additional data. Fetch all if not provided.
        /// </summary>
        [JsonPropertyName("pick")]
        public CloudPickOptions? Pick { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public static class CloudSelectRequestOption
    {
        [JsonPropertyName("status")]
        public static string Status = "status";

        [JsonPropertyName("settings")]
        public static string Settings = "settings";
    }

    /// <summary>
    /// Define witch kin of options select from cloud
    /// </summary>
    public class CloudPickOptions
    {
        /// <summary>
        /// Restrict device status to specific top-level properties. Fetch all if not provided.
        /// </summary>
        [JsonPropertyName("status")]
        public string[]? Status { get; set; }

        /// <summary>
        /// Restrict device settings to specific top-level properties. Fetch all if not provided.
        /// </summary>
        [JsonPropertyName("settings")]
        public string[]? Settings { get; set; }
    }
}
