using Asg.MCP.Models.Shelly;
using Asg.MCP.Services;
using ModelContextProtocol.Server;
using Shelly.Models;
using Shelly.Models.Cloud;
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
    }
}
