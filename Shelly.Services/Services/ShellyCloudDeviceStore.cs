using Shelly.Models.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Services.Services
{
    public class ShellyCloudDeviceStore
    {
        public readonly IEnumerable<DeviceNameMappingStoreItem> Store = 
            new List<DeviceNameMappingStoreItem>()
            {
                new DeviceNameMappingStoreItem()
                {
                    DeviceId = "your-device-id",
                    ChannelId = 0,
                    FriendlyNames = ["kitchen", "kitchen light", "luce cucina", "cucina"],
                    DeviceType = "switch"
                }
            };
    }
}

