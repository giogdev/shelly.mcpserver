using Asg.MCP.Models.Shelly;
using System.Text.Json.Serialization;
using System.Text.Json;
using Shelly.Models.Cloud.Request;
using Microsoft.Extensions.Configuration;
using Polly;
using Shelly.Services.Utils;
using Shelly.Models.Cloud;
using Shelly.Models;
using Shelly.Services.Mapper;
using Shelly.Services.Services;

namespace Asg.MCP.Services
{
    /*4022d8dbf95c*/
    public class ShellyCloudService : IShellyCloudService
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

        public DeviceNameMappingStoreItem? GetDeviceByFriendlyName(string name) 
        {
            return _deviceStore.Store.Where(x=> x.FriendlyNames.Contains(name)).FirstOrDefault();
        }

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

        public async Task<GenericDeviceStatusModel?> GetSingleDeviceStateAsync(DeviceNameMappingStoreItem deviceRequest)
        {
            var devices = await GetDeviceStateAsync([deviceRequest]);
            return devices?.FirstOrDefault();
        }

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

            //var actionResult = await _deserializeApiResponseAsync<CloudDeviceResponseModel[]>(response);
            //return actionResult ?? Array.Empty<CloudDeviceResponseModel>();
        }

        #region Private

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
