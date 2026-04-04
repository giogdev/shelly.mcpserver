using Microsoft.Extensions.Configuration;
using Shelly.Models.Cloud;
using System.Text.Json;

namespace Shelly.Services.Services
{
    public class ShellyCloudDeviceStore(IConfiguration configuration)
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        // ReaderWriterLockSlim: many concurrent reads, rare writes (only at startup / refresh).
        private readonly ReaderWriterLockSlim _lock = new();
        private List<DeviceNameMappingStoreItem> _store = LoadFromConfiguration(configuration);

        public IEnumerable<DeviceNameMappingStoreItem> Store
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return [.. _store];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void UpdateStore(IEnumerable<DeviceNameMappingStoreItem> devices)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var device in devices)
                {
                    // Match on both DeviceId AND ChannelId: multi-channel devices (e.g. Plus 2PM)
                    // produce one store item per channel and must not be merged together.
                    var existing = _store.FirstOrDefault(
                        s => s.DeviceId == device.DeviceId && s.ChannelId == device.ChannelId);
                    if (existing != null)
                    {
                        existing.FriendlyNames = [.. (existing.FriendlyNames ?? []).Union(device.FriendlyNames ?? [])];
                        existing.DeviceType    = device.DeviceType;
                    }
                    else
                    {
                        _store.Add(device);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>Reset and re-initialize the device store from configuration.</summary>
        public void ReinitializeDeviceStore()
        {
            var fresh = LoadFromConfiguration(configuration);
            _lock.EnterWriteLock();
            try
            {
                _store = fresh;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private static List<DeviceNameMappingStoreItem> LoadFromConfiguration(IConfiguration configuration)
        {
            var mappingFile = configuration["DeviceMappingFile"];

            if (string.IsNullOrEmpty(mappingFile) || !File.Exists(mappingFile))
                return [];

            var json = File.ReadAllText(mappingFile);
            return JsonSerializer.Deserialize<List<DeviceNameMappingStoreItem>>(json, JsonOptions) ?? [];
        }
    }
}
