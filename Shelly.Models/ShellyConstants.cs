namespace Shelly.Models;

/// <summary>
/// Centralised constants for Shelly device codes, statuses and sentinel values.
/// </summary>
public static class ShellyConstants
{
    /// <summary>Known Shelly device model codes.</summary>
    public static class DeviceCodes
    {
        public const string DoorWindowGen1 = "SHDW-2";
        public const string RelayGen1      = "SHSW-1";
        public const string Plus2PmGen2    = "SNSW-102P16EU";
    }

    /// <summary>Status strings returned by the mapping layer.</summary>
    public static class DeviceStatus
    {
        public const string On      = "TURNED_ON";
        public const string Off     = "TURNED_OFF";
        public const string Unknown = "unknown";
    }
}
