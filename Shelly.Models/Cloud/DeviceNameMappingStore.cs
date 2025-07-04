using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Models.Cloud
{
    public class DeviceNameMappingStoreItem
    {
        public string DeviceId { get; set; }
        public IEnumerable<string> FriendlyNames { get; set; }
        public int ChannelId = 0;
        public string DeviceType { get; set; }
    }
}
