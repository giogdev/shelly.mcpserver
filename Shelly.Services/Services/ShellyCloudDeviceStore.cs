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
        private List<DeviceNameMappingStoreItem> _store;
        public IEnumerable<DeviceNameMappingStoreItem> Store => _store;

        public ShellyCloudDeviceStore(IConfiguration configuration)
        {
            // When DeviceMappingFileRequired is false (or not set), skip file loading entirely.
            var mappingRequired = configuration.GetValue<bool>("DeviceMappingFileRequired");
            if (!mappingRequired)
            {
                _store = new List<DeviceNameMappingStoreItem>();
                return;
            }

            var mappingFile = configuration["DeviceMappingFile"];
            if (string.IsNullOrEmpty(mappingFile) || !File.Exists(mappingFile))
            {
                throw new Exception("DeviceMappingFile is empty");
            }
            var json = File.ReadAllText(mappingFile);
            _store = JsonSerializer.Deserialize<List<DeviceNameMappingStoreItem>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) ?? new List<DeviceNameMappingStoreItem>();
        }

        public void UpdateStore(IEnumerable<DeviceNameMappingStoreItem> devices)
        {
            _store = new List<DeviceNameMappingStoreItem>(devices);
        }
    }
}

