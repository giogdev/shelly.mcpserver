using FluentAssertions;
using Giogdev.Shelly.Integrations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using Shelly.Services.Exceptions;
using Shelly.Services.Services;
using Shelly.Test.Helpers;
using System.Net;
using System.Text.Json;

namespace Shelly.Test.Services;

public class ShellyCloudServiceTests
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ShellyCloudDeviceStore _deviceStore;
    private readonly ILogger<ShellyCloudService> _logger;

    public ShellyCloudServiceTests()
    {
        _config = Substitute.For<IConfiguration>();
        _config["SHELLY_API_ENDPOINT"].Returns("shelly-00-eu.shelly.cloud");
        _config["SHELLY_API_KEY"].Returns("test-api-key");

        // ShellyCloudDeviceStore needs DeviceMappingFileRequired to be false for empty store
        var storeConfig = Substitute.For<IConfiguration>();
        storeConfig.GetSection("DeviceMappingFileRequired").Value.Returns("false");
        _deviceStore = new ShellyCloudDeviceStore(storeConfig);

        _clientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<ShellyCloudService>>();
    }

    private ShellyCloudService CreateService(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        _clientFactory.CreateClient("ShellyCloudClient").Returns(httpClient);
        return new ShellyCloudService(_config, _clientFactory, _deviceStore, _logger);
    }

    private static FakeHttpMessageHandler CreateHandlerWithJsonResponse(string jsonContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new FakeHttpMessageHandler((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });
    }

    #region FetchAndPopulateDevicesAsync

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_SuccessfulResponse_PopulatesStore()
    {
        var json = TestFixtureLoader.LoadEmbeddedJson("v2-devices-get.example.json");
        var handler = CreateHandlerWithJsonResponse(json);
        var service = CreateService(handler);

        await service.FetchAndPopulateDevicesAsync();

        _deviceStore.Store.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_SendsCorrectUrl()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new FakeHttpMessageHandler((request, ct) =>
        {
            capturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            });
        });
        var service = CreateService(handler);

        await service.FetchAndPopulateDevicesAsync();

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("/v2/devices/get");
        capturedRequest.RequestUri.ToString().Should().Contain("auth_key=test-api-key");
    }

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_SendsCorrectPayload()
    {
        string? capturedBody = null;
        var handler = new FakeHttpMessageHandler(async (request, ct) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };
        });
        var service = CreateService(handler);

        await service.FetchAndPopulateDevicesAsync();

        capturedBody.Should().NotBeNullOrEmpty();
        var payload = JsonDocument.Parse(capturedBody!);
        var root = payload.RootElement;
        root.GetProperty("select").EnumerateArray().Select(e => e.GetString()).Should().Contain("status").And.Contain("settings");
        root.GetProperty("show").EnumerateArray().Select(e => e.GetString()).Should().Contain("offline").And.Contain("shared");
    }

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_EmptyResponse_DoesNotPopulateStore()
    {
        var handler = CreateHandlerWithJsonResponse("[]");
        var service = CreateService(handler);

        await service.FetchAndPopulateDevicesAsync();

        _deviceStore.Store.Should().BeEmpty();
    }

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_HttpError_ThrowsHttpRequestException()
    {
        // Note: Polly retries twice with 1s delay before giving up, so this test takes ~2s
        var handler = CreateHandlerWithJsonResponse(
            """{"isok":false,"errors":{"server_error":"internal"}}""",
            HttpStatusCode.InternalServerError);
        var service = CreateService(handler);

        var act = () => service.FetchAndPopulateDevicesAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task FetchAndPopulateDevicesAsync_SmallResponse_MapsCorrectDeviceNames()
    {
        var smallJson = """
        [
            {
                "id": "test001",
                "type": "relay",
                "code": "SHSW-1",
                "gen": "G1",
                "online": 1,
                "status": {},
                "settings": {
                    "name": "Kitchen Light"
                }
            },
            {
                "id": "test002",
                "type": "relay",
                "code": "SNSW-002P16EU",
                "gen": "G2",
                "online": 1,
                "status": {},
                "settings": {
                    "switch:0": { "name": "Bedroom Left" },
                    "switch:1": { "name": "Bedroom Right" }
                }
            }
        ]
        """;
        var handler = CreateHandlerWithJsonResponse(smallJson);
        var service = CreateService(handler);

        await service.FetchAndPopulateDevicesAsync();

        var storeItems = _deviceStore.Store.ToList();
        storeItems.Should().HaveCount(3);

        storeItems.Should().Contain(x => x.DeviceId == "test001" && x.FriendlyNames.Contains("Kitchen Light"));
        storeItems.Should().Contain(x => x.DeviceId == "test002" && x.ChannelId == 0 && x.FriendlyNames.Contains("Bedroom Left"));
        storeItems.Should().Contain(x => x.DeviceId == "test002" && x.ChannelId == 1 && x.FriendlyNames.Contains("Bedroom Right"));
    }

    #endregion

    #region GetWeatherStationStatisticsAsync

    [Fact]
    public async Task GetWeatherStationStatisticsAsync_ValidFixtureResponse_DeserializesStatistics()
    {
        var json = TestFixtureLoader.LoadEmbeddedJson("v2-statistics-weather-station-get.example.json");
        var handler = CreateHandlerWithJsonResponse(json);
        var service = CreateService(handler);

        var request = new WeatherStationStatisticsRequest
        {
            DeviceId = "ws-123",
            DateFrom = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2026, 3, 30, 6, 46, 0, DateTimeKind.Utc)
        };

        var result = await service.GetWeatherStationStatisticsAsync(request);

        result.Should().NotBeNull();
        result!.Timezone.Should().Be("UTC");
        result.Interval.Should().Be("minute");
        result.History.Should().HaveCount(47);

        var first = result.History.First();
        first.Datetime.Should().Be(new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc));
        first.MinTemperature.Should().Be(7d);
        first.MaxTemperature.Should().Be(7d);
        first.Humidity.Should().Be(54d);
        first.WindSpeed.Should().BeNull();
    }

    [Fact]
    public async Task GetWeatherStationStatisticsAsync_SendsCorrectUrlAndQueryParameters()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new FakeHttpMessageHandler((request, ct) =>
        {
            capturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"timezone\":\"UTC\",\"interval\":\"minute\",\"history\":[]}", System.Text.Encoding.UTF8, "application/json")
            });
        });
        var service = CreateService(handler);

        var request = new WeatherStationStatisticsRequest
        {
            DeviceId = "weather-device-42",
            DateFrom = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2026, 3, 30, 6, 46, 0, DateTimeKind.Utc)
        };

        await service.GetWeatherStationStatisticsAsync(request);

        capturedRequest.Should().NotBeNull();
        var uri = capturedRequest!.RequestUri;
        uri.Should().NotBeNull();
        uri!.AbsolutePath.Should().Contain("/v2/statistics/weather-station");

        var query = uri.Query;
        query.Should().Contain("id=weather-device-42");
        query.Should().Contain("channel=0");
        query.Should().Contain("date_range=custom");
        query.Should().Contain("date_from=2026-03-30T06%3A00%3A00Z");
        query.Should().Contain("date_to=2026-03-30T06%3A46%3A00Z");
        query.Should().Contain("auth_key=test-api-key");
    }

    [Fact]
    public async Task GetWeatherStationStatisticsAsync_IsOkFalse_ThrowsShellyCloudApiException()
    {
        var errorJson = """
        {
          "isok": false,
          "errors": {
            "device_not_found": "Device not found"
          }
        }
        """;
        var handler = CreateHandlerWithJsonResponse(errorJson, HttpStatusCode.OK);
        var service = CreateService(handler);

        var request = new WeatherStationStatisticsRequest
        {
            DeviceId = "missing-device",
            DateFrom = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2026, 3, 30, 6, 46, 0, DateTimeKind.Utc)
        };

        var act = () => service.GetWeatherStationStatisticsAsync(request);

        var exception = await act.Should().ThrowAsync<ShellyCloudApiException>();
        exception.Which.ErrorKey.Should().Be("device_not_found");
        exception.Which.Message.Should().Be("Device not found");
    }

    [Fact]
    public async Task GetWeatherStationStatisticsAsync_HttpErrorWithoutShellyEnvelope_ThrowsHttpRequestException()
    {
        var handler = CreateHandlerWithJsonResponse("{\"message\":\"internal error\"}", HttpStatusCode.InternalServerError);
        var service = CreateService(handler);

        var request = new WeatherStationStatisticsRequest
        {
            DeviceId = "ws-123",
            DateFrom = new DateTime(2026, 3, 30, 6, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2026, 3, 30, 6, 46, 0, DateTimeKind.Utc)
        };

        var act = () => service.GetWeatherStationStatisticsAsync(request);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

        #region GetDeviceStateAsync

        [Fact]
        public async Task GetDeviceStateAsync_SendsCorrectUrlAndPayload()
        {
                HttpRequestMessage? capturedRequest = null;
                string? capturedBody = null;
                var handler = new FakeHttpMessageHandler(async (request, ct) =>
                {
                        capturedRequest = request;
                        capturedBody = await request.Content!.ReadAsStringAsync(ct);
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
                        };
                });
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "dev-1", ChannelId = 0 },
                        new DeviceNameMappingStoreItem { DeviceId = "dev-2", ChannelId = 1 }
                };

                await service.GetDeviceStateAsync(devices);

                capturedRequest.Should().NotBeNull();
                capturedRequest!.RequestUri!.ToString().Should().Contain("/v2/devices/api/get");
                capturedRequest.RequestUri.ToString().Should().Contain("auth_key=test-api-key");

                capturedBody.Should().NotBeNullOrEmpty();
                var payload = JsonDocument.Parse(capturedBody!);
                var root = payload.RootElement;
                root.GetProperty("ids").EnumerateArray().Select(e => e.GetString())
                        .Should().ContainInOrder("dev-1_0", "dev-2_1");
                root.GetProperty("select").EnumerateArray().Select(e => e.GetString())
                        .Should().ContainSingle().Which.Should().Be("status");
        }

        [Fact]
        public async Task GetDeviceStateAsync_RelayDevice_MapsRelayStatus()
        {
                var responseJson = """
                [
                    {
                        "id": "relay-1",
                        "code": "SHSW-1",
                        "status": {
                            "relays": [
                                { "ison": true }
                            ]
                        }
                    }
                ]
                """;
                var handler = CreateHandlerWithJsonResponse(responseJson);
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "relay-1", ChannelId = 0 }
                };

                var result = (await service.GetDeviceStateAsync(devices)).ToList();

                result.Should().HaveCount(1);
                result[0].Status.Should().Be("TURNED_ON");
                result[0].IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetDeviceStateAsync_SwitchDeviceChannel1_UsesSwitch1Values()
        {
                var responseJson = """
                [
                    {
                        "id": "switch-2pm",
                        "code": "SNSW-102P16EU",
                        "status": {
                            "switch:0": { "output": false, "apower": 10.5 },
                            "switch:1": { "output": true, "apower": 222.7 }
                        }
                    }
                ]
                """;
                var handler = CreateHandlerWithJsonResponse(responseJson);
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "switch-2pm", ChannelId = 1 }
                };

                var result = (await service.GetDeviceStateAsync(devices)).ToList();

                result.Should().HaveCount(1);
                result[0].Status.Should().Be("TURNED_ON");
                result[0].Watt.Should().Be(222.7);
        }

        [Fact]
        public async Task GetDeviceStateAsync_DoorWindowDevice_UsesDoorWindowMapping()
        {
                var responseJson = """
                [
                    {
                        "id": "door-1",
                        "code": "SHDW-2",
                        "status": {
                            "sensor": { "state": "open" },
                            "bat": { "value": 91 },
                            "tmp": { "value": 18.2 },
                            "lux": { "value": 450 }
                        }
                    }
                ]
                """;
                var handler = CreateHandlerWithJsonResponse(responseJson);
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "door-1", ChannelId = 0 }
                };

                var result = (await service.GetDeviceStateAsync(devices)).ToList();

                result.Should().HaveCount(1);
                result[0].Status.Should().Be("open");
                result[0].BatteryPercentage.Should().Be(91);
                result[0].Temperature.Should().Be(18.2);
                result[0].Lux.Should().Be(450);
        }

        [Fact]
        public async Task GetDeviceStateAsync_DeviceNotFoundInApiResponse_ReturnsEmptyCollection()
        {
                var responseJson = """
                [
                    {
                        "id": "other-device",
                        "code": "SHSW-1",
                        "status": {
                            "relays": [
                                { "ison": true }
                            ]
                        }
                    }
                ]
                """;
                var handler = CreateHandlerWithJsonResponse(responseJson);
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "requested-device", ChannelId = 0 }
                };

                var result = (await service.GetDeviceStateAsync(devices)).ToList();

                result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDeviceStateAsync_StatusMacAsNumber_DoesNotThrowAndMapsRelay()
        {
                var responseJson = """
                [
                    {
                        "id": "relay-1",
                        "code": "SHSW-1",
                        "status": {
                            "mac": 1,
                            "relays": [
                                { "ison": true }
                            ]
                        }
                    }
                ]
                """;
                var handler = CreateHandlerWithJsonResponse(responseJson);
                var service = CreateService(handler);

                var devices = new[]
                {
                        new DeviceNameMappingStoreItem { DeviceId = "relay-1", ChannelId = 0 }
                };

                var result = (await service.GetDeviceStateAsync(devices)).ToList();

                result.Should().HaveCount(1);
                result[0].Status.Should().Be("TURNED_ON");
        }

        #endregion
}
