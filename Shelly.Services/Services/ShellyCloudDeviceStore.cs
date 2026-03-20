using Microsoft.Extensions.Configuration;
using Shelly.Models.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shelly.Services.Services
{
    public class ShellyCloudDeviceStore
    {
        public IEnumerable<DeviceNameMappingStoreItem> Store { get; }

        public ShellyCloudDeviceStore(IConfiguration configuration)
        {
            // When DeviceMappingFileRequired is false (or not set), skip file loading entirely.
            var mappingRequired = configuration.GetValue<bool>("DeviceMappingFileRequired");
            if (!mappingRequired)
            {
                Store = [];
                return;
            }

            var mappingFile = configuration["DeviceMappingFile"];
            if (string.IsNullOrEmpty(mappingFile) || !File.Exists(mappingFile))
            {
                throw new Exception("DeviceMappingFile is empty");
            }
            var json = File.ReadAllText(mappingFile);
            Store = JsonSerializer.Deserialize<List<DeviceNameMappingStoreItem>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
    }
}

