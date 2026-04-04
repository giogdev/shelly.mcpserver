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
        /// <summary>
        /// Reflects the last known connectivity state from the Shelly Cloud API.
        /// Defaults to false — devices start as offline until the first cloud refresh confirms their state.
        /// </summary>
        public bool IsOnline { get; set; } = false;
    }
}
