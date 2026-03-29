using Giogdev.Shelly.Integrations.Models.Shelly;
using System.Text.Json.Serialization;
using System.Text.Json;
using Shelly.Models.Cloud.Request;
using Microsoft.Extensions.Configuration;
using Polly;
using Shelly.Services.Utils;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Response;
using Shelly.Models;
using Shelly.Services.Mapper;
using Shelly.Services.Services;

namespace Giogdev.Shelly.Integrations.Services
{
    /// <summary>
    /// Integration service with Shelly cloud
    /// </summary>
    public partial class ShellyCloudService : IShellyCloudService
    {
        private readonly string _host;
        private readonly string _authKey;
        private readonly HttpClient _httpClient;
        private readonly ShellyCloudDeviceStore _deviceStore;

        public ShellyCloudService(IConfiguration _config, IHttpClientFactory _clientFactory, ShellyCloudDeviceStore _store)
        {
            _httpClient = _clientFactory.CreateClient("ShellyCloudClient");
            _host = _config["SHELLY_API_ENDPOINT"] ?? throw new Exception("SHELLY_API_ENDPOINT key not provided");
            _authKey = _config["SHELLY_API_KEY"] ?? throw new Exception("SHELLY_API_KEY key not provided");
            _deviceStore = _store;
        }

        /// <summary>
        /// Get list of devices
        /// </summary>
        public IEnumerable<DeviceNameMappingStoreItem> GeKnownDevices()
        {
            return _deviceStore.Store;
        }

        /// <summary>
        /// Get device by name given in shelly app
        /// </summary>
        public DeviceNameMappingStoreItem? GetDeviceByFriendlyName(string name) 
        {
            return _deviceStore.Store.Where(x=> x.FriendlyNames.Contains(name)).FirstOrDefault();
        }

        /// <summary>
        /// Get the state of all requested devices
        /// </summary>
        public async Task<IEnumerable<GenericDeviceStatusModel>> GetDeviceStateAsync(IEnumerable<DeviceNameMappingStoreItem> devicesRequest)
        {
            //Prepare request payload
            var request = new CloudDeviceRequest
            {
                Ids = devicesRequest.Select(x=> $"{x.DeviceId}_{x.ChannelId}" ).ToArray(),
                Select = [CloudSelectRequestOption.Status]
            };

            var content = _serializeAndPreparePayloadForHttpRequest(request);
            var url = $"https://{_host}/v2/devices/api/get?auth_key={_authKey}";

            //Get response from endpoint
            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(async (context) =>
            await _httpClient.PostAsync(url, content), new Context { }); 

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }

            var apiResult = await _deserializeApiResponseAsync<CloudDeviceResponseModel[]>(response);

            if(apiResult!= null)
            {
                List<GenericDeviceStatusModel> mappedDevices = new List<GenericDeviceStatusModel>();
                foreach (DeviceNameMappingStoreItem requestedDevice in devicesRequest)
                {
                    CloudDeviceResponseModel? cloudDevice = apiResult.FirstOrDefault(x => x.Id == requestedDevice.DeviceId);
                    
                    //Map response (only if I found device in cloud response)
                    if (cloudDevice != null)
                    {
                        switch (cloudDevice.Code)
                        {
                            case "SHDW-2": //Shelly door window (Gen2)
                                mappedDevices.Add(ShellyCloudMapper.MapDoorWindowGen1Device(cloudDevice));
                                break;
                            case "SHSW-1": //Shelly 1 (Gen1)
                                mappedDevices.Add(ShellyCloudMapper.MapRelayDevice(cloudDevice));
                                break;
                            case "SNSW-102P16EU": //Shelly plus 2PM (Gen2)
                            default:
                                mappedDevices.Add(ShellyCloudMapper.MapSwitchDevice(cloudDevice, requestedDevice.ChannelId));
                                break;

                        }
                    }
                    
                }

                return mappedDevices;
            }

            return [];
        }

        /// <summary>
        /// Get state of single device
        /// </summary>
        /// <param name="deviceRequest"></param>
        /// <returns></returns>
        public async Task<GenericDeviceStatusModel?> GetSingleDeviceStateAsync(DeviceNameMappingStoreItem deviceRequest)
        {
            var devices = await GetDeviceStateAsync([deviceRequest]);
            return devices?.FirstOrDefault();
        }

        /// <summary>
        /// Fetch all devices from Shelly Cloud and populate the local device store.
        /// </summary>
        public async Task FetchAndPopulateDevicesAsync()
        {
            var request = new GetAllDevicesRequest
            {
                Select = ["status", "settings"],
                Show = ["offline", "shared"]
            };

            var content = _serializeAndPreparePayloadForHttpRequest(request);
            var url = $"https://{_host}/v2/devices/get?auth_key={_authKey}";

            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(async (context) =>
                await _httpClient.PostAsync(url, content), new Context { });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"FetchAndPopulateDevicesAsync failed with status {response.StatusCode}: {errorContent}");
            }

            var devices = await _deserializeApiResponseAsync<GetDevicesResponseModel>(response);

            if (devices == null || devices.Count == 0)
            {
                Console.WriteLine("[WARNING] FetchAndPopulateDevicesAsync: No devices returned from API.");
                return;
            }

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
                    // G1: name is at the top level of settings
                    string name = _tryGetStringProperty(settings, "name") ?? string.Empty;
                    if (string.IsNullOrEmpty(name))
                        Console.WriteLine($"[WARNING] Device {device.Id} (G1): name not found in settings.");

                    storeItems.Add(new DeviceNameMappingStoreItem
                    {
                        DeviceId = device.Id,
                        FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                        ChannelId = 0,
                        DeviceType = device?.Type ?? device?.Code ?? ""
                    });
                }
                else
                {
                    // G2/GBLE: look for switch:0, switch:1, etc.
                    bool foundSwitch = false;
                    for (int channel = 0; channel <= 3; channel++)
                    {
                        string switchKey = $"switch:{channel}";
                        if (settings.TryGetProperty(switchKey, out JsonElement switchElement))
                        {
                            foundSwitch = true;
                            string name = _tryGetStringProperty(switchElement, "name") ?? string.Empty;
                            if (string.IsNullOrEmpty(name))
                                Console.WriteLine($"[WARNING] Device {device.Id} ({switchKey}): name not found.");

                            storeItems.Add(new DeviceNameMappingStoreItem
                            {
                                DeviceId = device.Id,
                                FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                                ChannelId = channel,
                                DeviceType = device?.Type ?? device?.Code ?? ""
                            });
                        }
                    }

                    // Fallback: look in script:0, script:1, etc.
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
                                    Console.WriteLine($"[WARNING] Device {device.Id} ({scriptKey}): name not found.");

                                storeItems.Add(new DeviceNameMappingStoreItem
                                {
                                    DeviceId = device.Id,
                                    FriendlyNames = string.IsNullOrEmpty(name) ? [] : [name],
                                    ChannelId = i,
                                    DeviceType = device?.Type ?? device?.Code ?? ""
                                });
                            }
                        }

                        // Final fallback: no switch or script found
                        if (!foundScript)
                        {
                            Console.WriteLine($"[WARNING] Device {device.Id}: no switch or script found in settings, mapping with empty name.");
                            storeItems.Add(new DeviceNameMappingStoreItem
                            {
                                DeviceId = device.Id,
                                FriendlyNames = [],
                                ChannelId = 0,
                                DeviceType = device?.Type ?? device?.Code ?? ""
                            });
                        }
                    }
                }
            }

            _deviceStore.UpdateStore(storeItems);
            Console.WriteLine($"[INFO] FetchAndPopulateDevicesAsync: populated store with {storeItems.Count} device(s).");
        }

        /// <summary>
        /// Change state of device (ex. light on - light off)
        /// </summary>
        /// <param name="switchRequest"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<string> ControlSwitchDevice(CloudDeviceSwitchRequest switchRequest)
        {
            if (switchRequest.ToggleAfter <= 0) switchRequest.ToggleAfter = null;

            var content = _serializeAndPreparePayloadForHttpRequest(switchRequest);
            string url = $"https://{_host}/v2/devices/api/set/switch?auth_key={_authKey}";

            //API call with Polly
            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(async (context) =>
            await _httpClient.PostAsync(url, content), new Context { });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        #region Private

        private static string? _tryGetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }

        /// <summary>
        /// Serialize and encode object for api call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        public StringContent _serializeAndPreparePayloadForHttpRequest<T>(T payload)
        {
            string serializedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            return new StringContent(serializedPayload, System.Text.Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Deseralize message from HttpResponseMessage
        /// </summary>
        /// <typeparam name="T">Type of expected class</typeparam>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        public async Task<T?> _deserializeApiResponseAsync<T>(HttpResponseMessage apiResponse) where T : class
        {
            var responseContent = await apiResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }

        #endregion
    }
}
