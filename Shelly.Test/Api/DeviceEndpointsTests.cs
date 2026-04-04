using FluentAssertions;
using Giogdev.Shelly.Integrations.Services;
using NSubstitute;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Exceptions;
using System.Net;
using System.Net.Http.Json;

namespace Shelly.Test.Api;

public class DeviceEndpointsTests : IClassFixture<ShellyApiFactory>
{
    private readonly HttpClient _client;
    private readonly IShellyCloudService _service;

    public DeviceEndpointsTests(ShellyApiFactory factory)
    {
        _client  = factory.CreateClient();
        _service = factory.CloudService;
    }

    // ─── GET /api/devices ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDevices_ReturnsOkWithList()
    {
        _service.GetKnownDevices().Returns(
        [
            new DeviceNameMappingStoreItem { DeviceId = "d1", FriendlyNames = ["Light"] }
        ]);

        var response = await _client.GetAsync("/api/devices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<DeviceNameMappingStoreItem>>();
        body.Should().HaveCount(1);
        body![0].DeviceId.Should().Be("d1");
    }

    [Fact]
    public async Task GetDevices_EmptyStore_ReturnsOkWithEmptyList()
    {
        _service.GetKnownDevices().Returns([]);

        var response = await _client.GetAsync("/api/devices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<DeviceNameMappingStoreItem>>();
        body.Should().BeEmpty();
    }

    // ─── GET /api/devices/{id}/status ───────────────────────────────────────

    [Fact]
    public async Task GetDeviceStatus_KnownDevice_ReturnsOk()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "dev-1", FriendlyNames = ["Lamp"] };
        _service.GetKnownDevices().Returns([storeItem]);
        _service.GetSingleDeviceStateAsync(storeItem).Returns(new GenericDeviceStatusModel
        {
            IsSuccess = true,
            Status    = "TURNED_ON"
        });

        var response = await _client.GetAsync("/api/devices/dev-1/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GenericDeviceStatusModel>();
        body!.Status.Should().Be("TURNED_ON");
    }

    [Fact]
    public async Task GetDeviceStatus_UnknownDevice_Returns404()
    {
        _service.GetKnownDevices().Returns([]);

        var response = await _client.GetAsync("/api/devices/nonexistent/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDeviceStatus_ServiceThrowsHttpRequestException_Returns502()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "dev-err", FriendlyNames = ["Broken"] };
        _service.GetKnownDevices().Returns([storeItem]);
        _service.GetSingleDeviceStateAsync(storeItem)
            .Returns(Task.FromException<GenericDeviceStatusModel?>(new HttpRequestException("upstream down")));

        var response = await _client.GetAsync("/api/devices/dev-err/status");

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task GetDeviceStatus_ServiceThrowsShellyApiException_DeviceNotFound_Returns404()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "dev-gone", FriendlyNames = ["Gone"] };
        _service.GetKnownDevices().Returns([storeItem]);
        _service.GetSingleDeviceStateAsync(storeItem)
            .Returns(Task.FromException<GenericDeviceStatusModel?>(
                new ShellyCloudApiException("device_not_found", "Device not found")));

        var response = await _client.GetAsync("/api/devices/dev-gone/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── POST /api/devices/{id}/switch ──────────────────────────────────────

    [Fact]
    public async Task SwitchDevice_ValidRequest_ReturnsOk()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "sw-1", ChannelId = 0 };
        _service.GetKnownDevices().Returns([storeItem]);
        _service.ControlSwitchDevice(Arg.Any<CloudDeviceSwitchRequest>()).Returns("ok");

        var response = await _client.PostAsJsonAsync("/api/devices/sw-1/switch", new { on = true });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SwitchDevice_UnknownDevice_Returns404()
    {
        _service.GetKnownDevices().Returns([]);

        var response = await _client.PostAsJsonAsync("/api/devices/unknown/switch", new { on = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SwitchDevice_InvalidDelay_Returns400()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "sw-2", ChannelId = 0 };
        _service.GetKnownDevices().Returns([storeItem]);

        var response = await _client.PostAsJsonAsync("/api/devices/sw-2/switch",
            new { on = true, delaySeconds = -5 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SwitchDevice_ServiceThrowsHttpRequestException_Returns502()
    {
        var storeItem = new DeviceNameMappingStoreItem { DeviceId = "sw-err", ChannelId = 0 };
        _service.GetKnownDevices().Returns([storeItem]);
        _service.ControlSwitchDevice(Arg.Any<CloudDeviceSwitchRequest>())
            .Returns(Task.FromException<string>(new HttpRequestException("upstream down")));

        var response = await _client.PostAsJsonAsync("/api/devices/sw-err/switch", new { on = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }
}
