using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Request;

public record GetAllDevicesRequest
{
    [JsonPropertyName("select")]
    public string[] Select { get; init; } = [];

    [JsonPropertyName("show")]
    public string[] Show { get; init; } = [];
}
