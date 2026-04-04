using Giogdev.Shelly.Integrations.Models.Shelly;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Response;

namespace Shelly.Services.Mapper
{
    /// <summary>
    /// Maps Shelly Cloud API responses to domain models.
    /// <para>
    /// To add a new device type: register a new <c>IDeviceMapper</c> implementation in DI.
    /// To change mapping behaviour in tests: inject a substitute of this interface.
    /// </para>
    /// </summary>
    public interface IShellyCloudMapper
    {
        /// <summary>Dispatches to the appropriate mapping method based on <paramref name="deviceCode"/>.</summary>
        GenericDeviceStatusModel Map(CloudDeviceResponseModel device, int channel, string deviceCode);

        /// <summary>Maps a Gen2 switch-type device status.</summary>
        GenericDeviceStatusModel MapSwitchDevice(CloudDeviceResponseModel device, int channel = 0);

        /// <summary>Maps a Gen1 relay device status.</summary>
        GenericDeviceStatusModel MapRelayDevice(CloudDeviceResponseModel device);

        /// <summary>Maps a Gen1 door/window sensor status.</summary>
        GenericDeviceStatusModel MapDoorWindowGen1Device(CloudDeviceResponseModel device);

        /// <summary>Maps a cloud device list response to the local device store items.</summary>
        List<DeviceNameMappingStoreItem> MapDevicesToStoreItems(GetDevicesResponseModel devices);
    }
}
