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
using Microsoft.Extensions.Logging;

namespace Giogdev.Shelly.Integrations.Services
{
    /// <summary>
    /// Integration service with Shelly cloud
    /// </summary>
    public partial class ShellyCloudService : IShellyCloudService
    {
        private const string DeviceCodeDoorWindowGen1 = "SHDW-2";
        private const string DeviceCodeRelayGen1 = "SHSW-1";
        private const string DeviceCodePlus2PmGen2 = "SNSW-102P16EU";

        private static readonly JsonSerializerOptions _serializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions _deserializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private static readonly Dictionary<string, Func<CloudDeviceResponseModel, int, GenericDeviceStatusModel>> _deviceMappers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [DeviceCodeDoorWindowGen1] = (device, _) => ShellyCloudMapper.MapDoorWindowGen1Device(device),
                [DeviceCodeRelayGen1] = (device, _) => ShellyCloudMapper.MapRelayDevice(device),
                [DeviceCodePlus2PmGen2] = (device, channel) => ShellyCloudMapper.MapSwitchDevice(device, channel)
            };

        private readonly string _host;
        private readonly string _authKey;
        private readonly HttpClient _httpClient;
        private readonly ShellyCloudDeviceStore _deviceStore;
        private readonly ILogger<ShellyCloudService> _logger;

        public ShellyCloudService(
            IConfiguration config,
            IHttpClientFactory clientFactory,
            ShellyCloudDeviceStore store,
            ILogger<ShellyCloudService> logger)
        {
            _httpClient = clientFactory.CreateClient("ShellyCloudClient");
            _host = config["SHELLY_API_ENDPOINT"] ?? throw new InvalidOperationException("SHELLY_API_ENDPOINT key not provided");
            _authKey = config["SHELLY_API_KEY"] ?? throw new InvalidOperationException("SHELLY_API_KEY key not provided");
            _deviceStore = store;
            _logger = logger;
        }

        /// <summary>
        /// Get list of devices
        /// </summary>
        public IEnumerable<DeviceNameMappingStoreItem> GetKnownDevices()
        {
            return _deviceStore.Store;
        }

        [Obsolete("Use GetKnownDevices instead.")]
        public IEnumerable<DeviceNameMappingStoreItem> GeKnownDevices()
        {
            return GetKnownDevices();
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

            var apiResult = await PostApiAsync<CloudDeviceRequest, CloudDeviceResponseModel[]>("/v2/devices/api/get", request);

            if(apiResult!= null)
            {
                List<GenericDeviceStatusModel> mappedDevices = new List<GenericDeviceStatusModel>();
                foreach (DeviceNameMappingStoreItem requestedDevice in devicesRequest)
                {
                    CloudDeviceResponseModel? cloudDevice = apiResult.FirstOrDefault(x => x.Id == requestedDevice.DeviceId);
                    
                    //Map response (only if I found device in cloud response)
                    if (cloudDevice != null)
                    {
                        if (!string.IsNullOrWhiteSpace(cloudDevice.Code) &&
                            _deviceMappers.TryGetValue(cloudDevice.Code, out var mapper))
                        {
                            mappedDevices.Add(mapper(cloudDevice, requestedDevice.ChannelId));
                        }
                        else
                        {
                            mappedDevices.Add(ShellyCloudMapper.MapSwitchDevice(cloudDevice, requestedDevice.ChannelId));
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

            var devices = await PostApiAsync<GetAllDevicesRequest, GetDevicesResponseModel>("/v2/devices/get", request);

            if (devices == null || devices.Count == 0)
            {
                _logger.LogWarning("FetchAndPopulateDevicesAsync: no devices returned from API.");
                return;
            }

            var storeItems = ShellyCloudMapper.MapDevicesToStoreItems(devices);

            _deviceStore.UpdateStore(storeItems);
            _logger.LogInformation("FetchAndPopulateDevicesAsync: populated store with {DeviceCount} device(s).", storeItems.Count);
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

            return await PostApiRawAsync("/v2/devices/api/set/switch", switchRequest);
        }

        #region Private

        private async Task<TResponse?> PostApiAsync<TRequest, TResponse>(string path, TRequest request)
            where TResponse : class
        {
            using var response = await PostApiAsync(path, request);
            return await _deserializeApiResponseAsync<TResponse>(response);
        }

        private async Task<string> PostApiRawAsync<TRequest>(string path, TRequest request)
        {
            using var response = await PostApiAsync(path, request);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> PostApiAsync<TRequest>(string path, TRequest request)
        {
            var content = _serializeAndPreparePayloadForHttpRequest(request);
            var url = $"https://{_host}{path}?auth_key={_authKey}";

            var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(
                async (context) => await _httpClient.PostAsync(url, content),
                new Context());

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }

            return response;
        }

        /// <summary>
        /// Serialize and encode object for api call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        private StringContent _serializeAndPreparePayloadForHttpRequest<T>(T payload)
        {
            string serializedPayload = JsonSerializer.Serialize(payload, _serializeOptions);

            return new StringContent(serializedPayload, System.Text.Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Deseralize message from HttpResponseMessage
        /// </summary>
        /// <typeparam name="T">Type of expected class</typeparam>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        private async Task<T?> _deserializeApiResponseAsync<T>(HttpResponseMessage apiResponse) where T : class
        {
            var responseContent = await apiResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseContent, _deserializeOptions);
        }

        #endregion
    }
}
