using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Request
{
    public class GetAllDevicesRequest
    {
        [JsonPropertyName("select")]
        public string[] Select { get; set; } = Array.Empty<string>();

        [JsonPropertyName("show")]
        public string[] Show { get; set; } = Array.Empty<string>();
    }
}
