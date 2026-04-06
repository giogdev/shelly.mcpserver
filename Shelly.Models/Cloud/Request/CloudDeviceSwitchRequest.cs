using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Request;

/// <summary>
/// Action request for switch device status.
/// </summary>
public record CloudDeviceSwitchRequest
{
    /// <summary>The Shelly device id (required).</summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public string Id { get; init; } = string.Empty;

    /// <summary>Channel number (defaults to 0).</summary>
    [JsonPropertyName("channel")]
    public int Channel { get; init; } = 0;

    /// <summary>Output state (required).</summary>
    [JsonPropertyName("on")]
    [JsonRequired]
    public bool On { get; init; }

    /// <summary>After how many seconds the state should be set to the opposite of "on". Null = no auto-revert.</summary>
    [JsonPropertyName("toggle_after")]
    public int? ToggleAfter { get; init; }
}
