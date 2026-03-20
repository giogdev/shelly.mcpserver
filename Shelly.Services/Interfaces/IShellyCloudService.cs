using Asg.MCP.Models.Shelly;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;

namespace Asg.MCP.Services
{
    public interface IShellyCloudService
    {
        #region Get

        Task<IEnumerable<GenericDeviceStatusModel>> GetDeviceStateAsync(IEnumerable<DeviceNameMappingStoreItem> devicesRequest);

        Task<GenericDeviceStatusModel?> GetSingleDeviceStateAsync(DeviceNameMappingStoreItem deviceRequest);

        /// <summary>
        /// Retrieves historical statistics for a Shelly weather station device over a custom date range.
        /// </summary>
        Task<WeatherStationStatisticsResponse?> GetWeatherStationStatisticsAsync(WeatherStationStatisticsRequest request);

        /// <summary>
        /// Retrieves historical power consumption statistics for a Shelly device over a custom date range.
        /// </summary>
        Task<PowerConsumptionStatisticsResponse?> GetPowerConsumptionStatisticsAsync(PowerConsumptionStatisticsRequest request);

        #endregion


        #region Actions

        Task<string> ControlSwitchDevice(CloudDeviceSwitchRequest switchRequest);
        DeviceNameMappingStoreItem? GetDeviceByFriendlyName(string name);
        IEnumerable<DeviceNameMappingStoreItem> GeKnownDevices();

        #endregion
    }
}