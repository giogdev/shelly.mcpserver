using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Models
{
    public class GenericDeviceStatusModel : DefaultResponse
    {
        [Description("Indicates the status of device")]
        public string Status{ get; set; }

        [Description("Indicates percentage of battery")]
        public int? BatteryPercentage { get; set; }
        public double? Temperature { get; set; }

        [Description("Environment lux")]
        public int? Lux { get; set; }

        [Description("")]
        public double? Watt { get; set; }

    }
}
