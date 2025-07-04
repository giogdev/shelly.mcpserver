using Asg.MCP.Models.Shelly;
using Shelly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Services.Mapper
{
    public class ShellyCloudMapper
    {
        private const string UNKNOWN_STATUS = "unknown";
        private const string TURNED_ON = "TURNED_ON";
        private const string TURNED_OFF = "TURNED_OFF";

        public static GenericDeviceStatusModel MapSwitchDevice(CloudDeviceResponseModel device, int channel = 0)
        {
            GenericDeviceStatusModel model = new GenericDeviceStatusModel() { IsSuccess = true };

            if(channel == 0)
            {
                model.Status = (device?.Status?.Switch0?.Output ?? false) ? TURNED_ON : TURNED_OFF;
                model.Watt = device?.Status?.Switch0?.APower ?? -1;

            }
            else
            {
                model.Status = (device?.Status?.Switch1?.Output ?? false) ? TURNED_ON : TURNED_OFF;
                model.Watt = device?.Status?.Switch0?.APower ?? -1;
            }
            

            return model;
        }

        public static GenericDeviceStatusModel MapRelayDevice(CloudDeviceResponseModel device)
        {
            return new GenericDeviceStatusModel()
            {
                IsSuccess = true,
                Status = device?.Status?.Relays?[0].IsOn ?? false ? TURNED_ON : TURNED_OFF
            };
        }

        public static GenericDeviceStatusModel MapDoorWindowGen1Device(CloudDeviceResponseModel device)
        {
            return new GenericDeviceStatusModel()
            {
                IsSuccess = true,
                Status = device?.Status?.Sensor?.State ?? UNKNOWN_STATUS,
                BatteryPercentage = device?.Status?.Battery?.Value ?? -1,
                Temperature = device?.Status?.Temperature?.Value ?? - 1,
                Lux = device?.Status?.Lux?.Value ?? -1,
            };
        }
    }
}
