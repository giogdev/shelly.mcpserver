using Asg.MCP.Models.Shelly;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;

namespace Asg.MCP.Services
{
    public interface IShellyCloudService
    {
        #region Get

        Task<IEnumerable<GenericDeviceStatusModel>> GetDeviceStateAsync(IEnumerable<DeviceNameMappingStoreItem> devicesRequest);

        Task<GenericDeviceStatusModel?> GetSingleDeviceStateAsync(DeviceNameMappingStoreItem deviceRequest);

        #endregion


        #region Actions

        Task<string> ControlSwitchDevice(CloudDeviceSwitchRequest switchRequest);
        DeviceNameMappingStoreItem? GetDeviceByFriendlyName(string name);

        #endregion
    }
}