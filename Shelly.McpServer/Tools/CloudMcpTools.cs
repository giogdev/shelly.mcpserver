using Giogdev.Shelly.Integrations.Models.Shelly;
using Giogdev.Shelly.Integrations.Services;
using ModelContextProtocol.Server;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using Shelly.Services.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.McpServer.Tools
{
    [McpServerToolType, Description("Tools to get informations about Shelly in Cloud")]
    public class CloudMcpTools
    {
        private readonly IShellyCloudService _shellyCloudService;
        public CloudMcpTools(IShellyCloudService shellyCloudService)
        {
            _shellyCloudService = shellyCloudService;
        }

        [McpServerTool, Description("Get status of Shelly Iot Devices from Shelly Cloud")]
        public async Task<GenericDeviceStatusModel> GetCloudDeviceStatusById(
            [Description("Name of your shelly device")] string deviceName
            )
        {
            try
            {
                //Get id from name
                DeviceNameMappingStoreItem? storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null) return new GenericDeviceStatusModel() { IsSuccess = false, Error = "There isn't any device with this name, try another name" };

                //get status of device
                GenericDeviceStatusModel? deviceState = await _shellyCloudService.GetSingleDeviceStateAsync(storeDevice);

                if (deviceState != null)
                {
                    return deviceState;
                }
                else
                {
                    //If status is null return a response for LLM
                    return new GenericDeviceStatusModel()
                    {
                        Status = "UNKNOWN",
                        IsSuccess = false,
                        Error = "UNKNOWN DEVICE STATUS"
                    };
                }

            }
            catch (Exception ex)
            {
                return new GenericDeviceStatusModel
                {
                    Status = "UNKNOWN",
                    Error = $"{ex.Message} {ex.StackTrace}"
                };
            }
        }

        [McpServerTool, Description("Get info about Shelly Iot Devices from Shelly Cloud (Battery, Watt, temperature, Lux, status)")]
        public async Task<GenericDeviceStatusModel> GetCloudDeviceInfoById(
            [Description("Name of your shelly device")] string deviceName
            )
        {
            return await GetCloudDeviceStatusById(deviceName);
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
            [Description("Optional parameter, after how many seconds, the state should be set to oposite the value of \"on\"")] int delay = -1
            )
        {
            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null) return new GenericDeviceStatusModel() { IsSuccess = false, Error = "There isn't any device with this name, try another name" };

                string result = await _shellyCloudService.ControlSwitchDevice(new Shelly.Models.Cloud.Request.CloudDeviceSwitchRequest
                {
                    Channel = storeDevice.ChannelId,
                    Id = storeDevice.DeviceId,
                    On = status == "ON",
                    ToggleAfter = delay == 0 ? -1 : delay
                });

                return new DefaultResponse(IsSuccess: true);
            }
            catch (Exception ex)
            {
                return new DefaultResponse
                {
                    IsSuccess = false,
                    Error = $"{ex.Message} {ex.StackTrace}"
                };
            }
        }
    
        [McpServerTool, Description("Get statistics of your shelly weather station")]
        public async Task<WeatherStationStatisticsResponse?> GetWeatherStationStatistics(
            [Description("Name of your shelly weather station device")] string deviceName,
            [Description("Start date for statistics (UTC)")] DateTime dateFrom,
            [Description("End date for statistics (UTC)")] DateTime dateTo)
        {
            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null) return null;

                var request = new WeatherStationStatisticsRequest
                {
                    DeviceId = storeDevice.DeviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                return await _shellyCloudService.GetWeatherStationStatisticsAsync(request);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return null;
            }
    
    }

        [McpServerTool, Description("Get statistics of your shelly device power consumption")]
        public async Task<PowerConsumptionStatisticsResponse?> GetPowerConsumptionStatistics(
            [Description("Name of your shelly device")] string deviceName,
            [Description("Start date for statistics (UTC)")] DateTime dateFrom,
            [Description("End date for statistics (UTC)")] DateTime dateTo)
        {
            try
            {
                var storeDevice = _shellyCloudService.GetDeviceByFriendlyName(deviceName);
                if (storeDevice == null) return null;

                var request = new PowerConsumptionStatisticsRequest
                {
                    DeviceId = storeDevice.DeviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                return await _shellyCloudService.GetPowerConsumptionStatisticsAsync(request);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return null;
            }
        }
    }

}
