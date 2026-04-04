using Giogdev.Shelly.Integrations.Models.Shelly;
using Microsoft.Extensions.Logging;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Response;
using System.Text.Json;

namespace Shelly.Services.Mapper
{
    public class ShellyCloudMapper : IShellyCloudMapper
    {
        private const string UNKNOWN_STATUS = ShellyConstants.DeviceStatus.Unknown;
        private const string TURNED_ON = ShellyConstants.DeviceStatus.On;
        private const string TURNED_OFF = ShellyConstants.DeviceStatus.Off;

        private readonly ILogger<ShellyCloudMapper> _logger;

        // Maps known device codes to their specific mapping method.
        // Devices not listed here fall back to MapSwitchDevice (the common case for Gen2 switches).
        private readonly Dictionary<string, Func<CloudDeviceResponseModel, int, GenericDeviceStatusModel>> _handlers;

        public ShellyCloudMapper(ILogger<ShellyCloudMapper> logger)
        {
            _logger = logger;
            _handlers = new(StringComparer.OrdinalIgnoreCase)
            {
                [ShellyConstants.DeviceCodes.RelayGen1] = (d, _) => MapRelayDevice(d),
                [ShellyConstants.DeviceCodes.DoorWindowGen1] = (d, _) => MapDoorWindowGen1Device(d),
            };
        }

        /// <summary>
        /// Dispatches mapping based on device code.
        /// Adding a new switch-type device requires no code change (fallback handles it).
        /// Adding a new sensor type: add one method + one entry in _handlers.
        /// </summary>
        public GenericDeviceStatusModel Map(CloudDeviceResponseModel device, int channel, string deviceCode)
        {
            if (!string.IsNullOrWhiteSpace(deviceCode) && _handlers.TryGetValue(deviceCode, out var handler))
                return handler(device, channel);

            return MapSwitchDevice(device, channel);
        }

        public GenericDeviceStatusModel MapSwitchDevice(CloudDeviceResponseModel device, int channel = 0)
        {
            GenericDeviceStatusModel model = new GenericDeviceStatusModel() { IsSuccess = true };

            if (channel == 0)
            {
                model.Status = (device?.Status?.Switch0?.Output ?? false) ? TURNED_ON : TURNED_OFF;
                model.Watt = device?.Status?.Switch0?.APower ?? -1;
            }
            else
            {
                model.Status = (device?.Status?.Switch1?.Output ?? false) ? TURNED_ON : TURNED_OFF;
                model.Watt = device?.Status?.Switch1?.APower ?? -1;
            }

            return model;
        }

        public GenericDeviceStatusModel MapRelayDevice(CloudDeviceResponseModel device)
        {
            return new GenericDeviceStatusModel()
            {
                IsSuccess = true,
                Status = device?.Status?.Relays?[0].IsOn ?? false ? TURNED_ON : TURNED_OFF
            };
        }

        public GenericDeviceStatusModel MapDoorWindowGen1Device(CloudDeviceResponseModel device)
        {
            return new GenericDeviceStatusModel()
            {
                IsSuccess = true,
                Status = device?.Status?.Sensor?.State ?? UNKNOWN_STATUS,
                BatteryPercentage = device?.Status?.Battery?.Value ?? -1,
                Temperature = device?.Status?.Temperature?.Value ?? -1,
                Lux = device?.Status?.Lux?.Value ?? -1,
            };
        }

        /// <summary>
        /// Maps a cloud response with the list of all kinds of devices to local store items.
        /// </summary>
        public List<DeviceNameMappingStoreItem> MapDevicesToStoreItems(GetDevicesResponseModel devices)
        {
            var storeItems = new List<DeviceNameMappingStoreItem>();

            foreach (var device in devices)
            {
                if (device.Settings == null)
                {
                    _logger.LogWarning("Device {DeviceId}: settings is null, skipping.", device.Id);
                    continue;
                }

                var settings = device.Settings.Value;

                if (device.Gen == "G1")
                {
                    string name = _tryGetStringProperty(settings, "name") ?? string.Empty;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = _getFallbackDeviceName(settings, device);
                        _logger.LogWarning(
                            "Device {DeviceId} (G1): name not found in settings, using fallback '{FallbackName}'.",
                            device.Id, name);
                    }

                    storeItems.Add(new DeviceNameMappingStoreItem
                    {
                        DeviceId = device.Id,
                        FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                        ChannelId = 0,
                        DeviceType = device.Type ?? device.Code ?? ""
                    });
                }
                else
                {
                    bool foundSwitch = false;
                    for (int channel = 0; channel <= 3; channel++)
                    {
                        string switchKey = $"switch:{channel}";
                        if (settings.TryGetProperty(switchKey, out JsonElement switchElement))
                        {
                            foundSwitch = true;
                            string name = _tryGetStringProperty(switchElement, "name") ?? string.Empty;
                            if (string.IsNullOrEmpty(name))
                            {
                                name = _getFallbackDeviceName(settings, device);
                                _logger.LogWarning(
                                    "Device {DeviceId} ({SwitchKey}): name not found, using fallback '{FallbackName}'.",
                                    device.Id, switchKey, name);
                            }

                            storeItems.Add(new DeviceNameMappingStoreItem
                            {
                                DeviceId = device.Id,
                                FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                                ChannelId = channel,
                                DeviceType = device.Type ?? device.Code ?? ""
                            });
                        }
                    }

                    if (!foundSwitch)
                    {
                        bool foundScript = false;
                        for (int i = 0; i <= 3; i++)
                        {
                            string scriptKey = $"script:{i}";
                            if (settings.TryGetProperty(scriptKey, out JsonElement scriptElement))
                            {
                                foundScript = true;
                                string name = _tryGetStringProperty(scriptElement, "name") ?? string.Empty;
                                if (string.IsNullOrEmpty(name))
                                {
                                    name = _getFallbackDeviceName(settings, device);
                                    _logger.LogWarning(
                                        "Device {DeviceId} ({ScriptKey}): name not found, using fallback '{FallbackName}'.",
                                        device.Id, scriptKey, name);
                                }

                                storeItems.Add(new DeviceNameMappingStoreItem
                                {
                                    DeviceId = device.Id,
                                    FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                                    ChannelId = i,
                                    DeviceType = device.Type ?? device.Code ?? ""
                                });
                            }
                        }

                        if (!foundScript)
                        {
                            string name = _getFallbackDeviceName(settings, device);
                            _logger.LogWarning(
                                "Device {DeviceId}: no switch or script found in settings, using fallback '{FallbackName}'.",
                                device.Id, name);
                            storeItems.Add(new DeviceNameMappingStoreItem
                            {
                                DeviceId = device.Id,
                                FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                                ChannelId = 0,
                                DeviceType = device.Type ?? device.Code ?? ""
                            });
                        }
                    }
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Returns a fallback name for the device when no friendly name is available.
        /// It tries to derive it from the system ID (stripping any MAC prefix), otherwise falls back to the device code.
        /// </summary>
        private static string _getFallbackDeviceName(JsonElement settings, GetDevicesDeviceItem device)
        {
            string id = string.Empty;
            string mac = string.Empty;

            if (settings.TryGetProperty("sys", out JsonElement sysElement)
                && sysElement.TryGetProperty("device", out JsonElement deviceElement))
            {
                id = _tryGetStringProperty(deviceElement, "id") ?? string.Empty;
                mac = _tryGetStringProperty(deviceElement, "mac") ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(mac))
                    id = id.Replace(mac, "").TrimEnd('-');
                return id;
            }

            if (!string.IsNullOrEmpty(device.Code))
                return device.Code;

            return string.Empty;
        }

        /// <summary>
        /// Attempts to retrieve a string property from a <see cref="JsonElement"/>.
        /// Returns null if the property is missing or not a string.
        /// </summary>
        private static string? _tryGetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }
    }
}
