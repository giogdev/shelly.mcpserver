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
    public class ShellyCloudDeviceStore(IConfiguration configuration)
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private List<DeviceNameMappingStoreItem> _store = LoadFromConfiguration(configuration);
        public IEnumerable<DeviceNameMappingStoreItem> Store => _store;

        public void UpdateStore(IEnumerable<DeviceNameMappingStoreItem> devices)
        {
            foreach (var device in devices)
            {
                var existing = _store.FirstOrDefault(s => s.DeviceId == device.DeviceId);
                if (existing != null)
                {
                    existing.FriendlyNames = [.. (existing.FriendlyNames ?? []).Union(device.FriendlyNames ?? [])];
                    existing.ChannelId = device.ChannelId;
                    existing.DeviceType = device.DeviceType;
                }
                else
                {
                    _store.Add(device);
                }
            }
        }

        /// <summary>
        /// Reset and re-initialize devices store
        /// </summary>
        public void ReinitializeDeviceStore()
        {
            _store = LoadFromConfiguration(configuration);
        }

        private static List<DeviceNameMappingStoreItem> LoadFromConfiguration(IConfiguration configuration)
        {
            var mappingFile = configuration["DeviceMappingFile"];

            if (string.IsNullOrEmpty(mappingFile) || !File.Exists(mappingFile))
            {
                return [];
            }

            var json = File.ReadAllText(mappingFile);
            return JsonSerializer.Deserialize<List<DeviceNameMappingStoreItem>>(json, JsonOptions) ?? [];
        }
    }
}

