using Giogdev.Shelly.Integrations.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using System.ComponentModel;

namespace Shelly.McpServer.Tools
{
    [McpServerToolType, Description("Tools to get informations about Shelly in Cloud")]
    public class CloudMcpTools
    {
        private readonly IShellyCloudService _shellyCloudService;
        private readonly ILogger<CloudMcpTools> _logger;

        public CloudMcpTools(IShellyCloudService shellyCloudService, ILogger<CloudMcpTools> logger)
        {
            _shellyCloudService = shellyCloudService;
            _logger = logger;
        }

        [McpServerTool, Description("Get status of Shelly Devices by id")]
        public async Task<GenericDeviceStatusModel> GetCloudDeviceStatusById(
            [Description("Name of your shelly device")] string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return new GenericDeviceStatusModel { IsSuccess = false, Error = "The device name cannot be empty." };

            try
            {
                DeviceNameMappingStoreItem? storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null)
                    return new GenericDeviceStatusModel { IsSuccess = false, Error = "There isn't any device with this name, try another name" };

                if (!storeDevice.IsOnline)
                    return new GenericDeviceStatusModel { IsSuccess = false, Error = $"Device '{deviceName}' is currently offline." };

                GenericDeviceStatusModel? deviceState = await _shellyCloudService.GetSingleDeviceStateAsync(storeDevice);

                return deviceState ?? new GenericDeviceStatusModel
                {
                    Status = ShellyConstants.DeviceStatus.Unknown,
                    IsSuccess = false,
                    Error = "UNKNOWN DEVICE STATUS"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCloudDeviceStatusById for device '{DeviceName}'.", deviceName);
                return new GenericDeviceStatusModel
                {
                    Status = ShellyConstants.DeviceStatus.Unknown,
                    IsSuccess = false,
                    Error = "An error occurred during the operation. Please try again."
                };
            }
        }

        [McpServerTool, Description("Get list of your shelly devices")]
        public IEnumerable<DeviceNameMappingStoreItem> GetDevices()
        {
            return _shellyCloudService.GetKnownDevices();
        }

        [McpServerTool, Description("Set ON or OFF on Shelly device (ex. light or switch device)")]
        public async Task<DefaultResponse> TurnOnOrOffDevice(
            [Description("Name of your shelly device")] string deviceName,
            [Description("ON or OFF")] string status,
            [Description("Optional: after how many seconds the state should revert. Omit or pass null to disable.")] int? delay = null)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return new DefaultResponse { IsSuccess = false, Error = "The device name cannot be empty." };

            if (string.IsNullOrWhiteSpace(status))
                return new DefaultResponse { IsSuccess = false, Error = "The 'status' parameter cannot be empty (use ON or OFF)." };

            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null)
                    return new DefaultResponse { IsSuccess = false, Error = "There isn't any device with this name, try another name" };

                if (!storeDevice.IsOnline)
                    return new DefaultResponse { IsSuccess = false, Error = $"Device '{deviceName}' is currently offline." };

                await _shellyCloudService.ControlSwitchDevice(new CloudDeviceSwitchRequest
                {
                    Channel = storeDevice.ChannelId,
                    Id = storeDevice.DeviceId,
                    On = status.Equals("ON", StringComparison.OrdinalIgnoreCase),
                    ToggleAfter = delay > 0 ? delay : null
                });

                return new DefaultResponse(IsSuccess: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TurnOnOrOffDevice for device '{DeviceName}'.", deviceName);
                return new DefaultResponse
                {
                    IsSuccess = false,
                    Error = "An error occurred during the operation. Please try again."
                };
            }
        }

        [McpServerTool, Description("Get statistics of your shelly weather station")]
        public async Task<WeatherStationStatisticsResponse?> GetWeatherStationStatistics(
            [Description("Name of your shelly weather station device")] string deviceName,
            [Description("Start date for statistics (UTC)")] DateTime dateFrom,
            [Description("End date for statistics (UTC)")] DateTime dateTo)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return null;

            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null)
                    return null;

                if (!storeDevice.IsOnline)
                {
                    _logger.LogWarning("GetWeatherStationStatistics: device '{DeviceName}' is currently offline.", deviceName);
                    return null;
                }

                return await _shellyCloudService.GetWeatherStationStatisticsAsync(new WeatherStationStatisticsRequest
                {
                    DeviceId = storeDevice.DeviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWeatherStationStatistics for device '{DeviceName}'.", deviceName);
                return null;
            }
        }

        [McpServerTool, Description("Get statistics of your shelly device power consumption")]
        public async Task<PowerConsumptionStatisticsResponse?> GetPowerConsumptionStatistics(
            [Description("Name of your shelly device")] string deviceName,
            [Description("Start date for statistics (UTC)")] DateTime dateFrom,
            [Description("End date for statistics (UTC)")] DateTime dateTo)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return null;

            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null)
                    return null;

                if (!storeDevice.IsOnline)
                {
                    _logger.LogWarning("GetPowerConsumptionStatistics: device '{DeviceName}' is currently offline.", deviceName);
                    return null;
                }

                return await _shellyCloudService.GetPowerConsumptionStatisticsAsync(new PowerConsumptionStatisticsRequest
                {
                    DeviceId = storeDevice.DeviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPowerConsumptionStatistics for device '{DeviceName}'.", deviceName);
                return null;
            }
        }
    }
}
