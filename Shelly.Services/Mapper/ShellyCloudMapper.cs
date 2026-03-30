using Giogdev.Shelly.Integrations.Models.Shelly;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public static List<DeviceNameMappingStoreItem> MapDevicesToStoreItems(GetDevicesResponseModel devices)
        {
            var storeItems = new List<DeviceNameMappingStoreItem>();

            foreach (var device in devices)
            {
                if (device.Settings == null)
                {
                    Console.WriteLine($"[WARNING] Device {device.Id}: settings is null, skipping.");
                    continue;
                }

                var settings = device.Settings.Value;

                if (device.Gen == "G1")
                {
                    string name = _tryGetStringProperty(settings, "name") ?? string.Empty;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = _getFallbackDeviceName(settings, device);
                        Console.WriteLine($"[WARNING] Device {device.Id} (G1): name not found in settings, using fallback '{name}'.");
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
                                Console.WriteLine($"[WARNING] Device {device.Id} ({switchKey}): name not found, using fallback '{name}'.");
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
                                    Console.WriteLine($"[WARNING] Device {device.Id} ({scriptKey}): name not found, using fallback '{name}'.");
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
                            Console.WriteLine($"[WARNING] Device {device.Id}: no switch or script found in settings, using fallback '{name}'.");
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

        private static string? _tryGetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }
    }
}
