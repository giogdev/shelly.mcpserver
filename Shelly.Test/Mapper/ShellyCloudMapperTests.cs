using FluentAssertions;
using Giogdev.Shelly.Integrations.Models.Shelly;
using Microsoft.Extensions.Logging.Abstractions;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Response;
using Shelly.Services.Mapper;
using System.Text.Json;
using Shelly.Test.Helpers;

namespace Shelly.Test.Mapper;

public class ShellyCloudMapperTests
{
    private readonly ShellyCloudMapper _mapper = new(NullLogger<ShellyCloudMapper>.Instance);

    #region MapSwitchDevice

    [Fact]
    public void MapSwitchDevice_Channel0_OutputTrue_ReturnsStatusTurnedOn()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch0 = new SwitchStatus { Output = true, APower = 50.5 }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 0);

        result.Status.Should().Be("TURNED_ON");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapSwitchDevice_Channel0_OutputFalse_ReturnsStatusTurnedOff()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch0 = new SwitchStatus { Output = false }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 0);

        result.Status.Should().Be("TURNED_OFF");
    }

    [Fact]
    public void MapSwitchDevice_Channel0_ReadsAPowerFromSwitch0()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch0 = new SwitchStatus { Output = true, APower = 123.4 }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 0);

        result.Watt.Should().Be(123.4);
    }

    [Fact]
    public void MapSwitchDevice_Channel1_OutputTrue_ReturnsStatusTurnedOn()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch1 = new SwitchStatus { Output = true, APower = 75.0 }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 1);

        result.Status.Should().Be("TURNED_ON");
    }

    [Fact]
    public void MapSwitchDevice_Channel1_OutputFalse_ReturnsStatusTurnedOff()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch1 = new SwitchStatus { Output = false, APower = 0 }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 1);

        result.Status.Should().Be("TURNED_OFF");
    }

    [Fact]
    public void MapSwitchDevice_Channel1_ReadsAPowerFromSwitch1()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Switch0 = new SwitchStatus { Output = false, APower = 100.0 },
                Switch1 = new SwitchStatus { Output = true, APower = 200.0 }
            }
        };

        var result = _mapper.MapSwitchDevice(device, channel: 1);

        result.Watt.Should().Be(200.0);
    }

    [Fact]
    public void MapSwitchDevice_NullStatus_ReturnsDefaults()
    {
        var device = new CloudDeviceResponseModel { Status = null };

        var result = _mapper.MapSwitchDevice(device, channel: 0);

        result.Status.Should().Be("TURNED_OFF");
        result.Watt.Should().Be(-1);
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region MapRelayDevice

    [Fact]
    public void MapRelayDevice_RelayIsOn_ReturnsStatusTurnedOn()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Relays = [new RelayInfo { IsOn = true }]
            }
        };

        var result = _mapper.MapRelayDevice(device);

        result.Status.Should().Be("TURNED_ON");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapRelayDevice_RelayIsOff_ReturnsStatusTurnedOff()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Relays = [new RelayInfo { IsOn = false }]
            }
        };

        var result = _mapper.MapRelayDevice(device);

        result.Status.Should().Be("TURNED_OFF");
    }

    [Fact]
    public void MapRelayDevice_NullRelays_ReturnsStatusTurnedOff()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus { Relays = null }
        };

        var result = _mapper.MapRelayDevice(device);

        result.Status.Should().Be("TURNED_OFF");
    }

    #endregion

    #region MapDoorWindowGen1Device

    [Fact]
    public void MapDoorWindowGen1Device_AllFieldsPopulated_MapsCorrectly()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Sensor = new SensorStatus { State = "open", IsValid = true },
                Battery = new BatteryStatus { Value = 85, Voltage = 3.7 },
                Temperature = new TemperatureStatus { Value = 22.5, Units = "C", IsValid = true },
                Lux = new LuxStatus { Value = 150, Illumination = "bright", IsValid = true }
            }
        };

        var result = _mapper.MapDoorWindowGen1Device(device);

        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be("open");
        result.BatteryPercentage.Should().Be(85);
        result.Temperature.Should().Be(22.5);
        result.Lux.Should().Be(150);
    }

    [Fact]
    public void MapDoorWindowGen1Device_NullSensor_ReturnsUnknownStatus()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Sensor = null,
                Battery = new BatteryStatus { Value = 50 },
                Temperature = new TemperatureStatus { Value = 20.0 },
                Lux = new LuxStatus { Value = 100 }
            }
        };

        var result = _mapper.MapDoorWindowGen1Device(device);

        result.Status.Should().Be("unknown");
    }

    [Fact]
    public void MapDoorWindowGen1Device_NullBattery_ReturnsMinus1()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Sensor = new SensorStatus { State = "close" },
                Battery = null,
                Temperature = new TemperatureStatus { Value = 20.0 },
                Lux = new LuxStatus { Value = 100 }
            }
        };

        var result = _mapper.MapDoorWindowGen1Device(device);

        result.BatteryPercentage.Should().Be(-1);
    }

    [Fact]
    public void MapDoorWindowGen1Device_NullTemperature_ReturnsMinus1()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Sensor = new SensorStatus { State = "close" },
                Battery = new BatteryStatus { Value = 50 },
                Temperature = null,
                Lux = new LuxStatus { Value = 100 }
            }
        };

        var result = _mapper.MapDoorWindowGen1Device(device);

        result.Temperature.Should().Be(-1);
    }

    [Fact]
    public void MapDoorWindowGen1Device_NullLux_ReturnsMinus1()
    {
        var device = new CloudDeviceResponseModel
        {
            Status = new CloudDeviceStatus
            {
                Sensor = new SensorStatus { State = "close" },
                Battery = new BatteryStatus { Value = 50 },
                Temperature = new TemperatureStatus { Value = 20.0 },
                Lux = null
            }
        };

        var result = _mapper.MapDoorWindowGen1Device(device);

        result.Lux.Should().Be(-1);
    }

    #endregion

    #region MapDevicesToStoreItems

    [Fact]
    public void MapDevicesToStoreItems_G1Device_ExtractsNameFromSettingsRoot()
    {
        var settings = JsonSerializer.SerializeToElement(new { name = "Living Room" });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev001", Gen = "G1", Type = "relay", Code = "SHSW-1",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].DeviceId.Should().Be("dev001");
        result[0].FriendlyNames.Should().Contain("Living Room");
        result[0].ChannelId.Should().Be(0);
        result[0].DeviceType.Should().Be("relay");
    }

    [Fact]
    public void MapDevicesToStoreItems_G1Device_NoName_UsesFallbackFromSysDeviceId()
    {
        var settings = JsonSerializer.SerializeToElement(new
        {
            sys = new { device = new { id = "shellyswitch-AABBCC", mac = "AABBCC" } }
        });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev002", Gen = "G1", Type = "relay", Code = "SHSW-1",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].FriendlyNames.Should().Contain("shellyswitch");
    }

    [Fact]
    public void MapDevicesToStoreItems_G1Device_NoNameNoSys_UsesFallbackFromCode()
    {
        var settings = JsonSerializer.SerializeToElement(new { other = "value" });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev003", Gen = "G1", Type = "relay", Code = "SHSW-1",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].FriendlyNames.Should().Contain("SHSW-1");
    }

    [Fact]
    public void MapDevicesToStoreItems_G2DeviceWithSwitches_CreatesItemPerChannel()
    {
        var settings = JsonSerializer.SerializeToElement(new Dictionary<string, object>
        {
            ["switch:0"] = new { name = "Light A" },
            ["switch:1"] = new { name = "Light B" }
        });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev004", Gen = "G2", Type = "relay", Code = "SNSW-002P16EU",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(2);
        result[0].DeviceId.Should().Be("dev004");
        result[0].FriendlyNames.Should().Contain("Light A");
        result[0].ChannelId.Should().Be(0);
        result[1].DeviceId.Should().Be("dev004");
        result[1].FriendlyNames.Should().Contain("Light B");
        result[1].ChannelId.Should().Be(1);
    }

    [Fact]
    public void MapDevicesToStoreItems_G2DeviceWithScripts_CreatesItemPerScript()
    {
        var settings = JsonSerializer.SerializeToElement(new Dictionary<string, object>
        {
            ["script:1"] = new { name = "Automation Script" }
        });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev005", Gen = "G2", Type = "inputs_reader", Code = "SNSN-0D24X",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].FriendlyNames.Should().Contain("Automation Script");
        result[0].ChannelId.Should().Be(1);
    }

    [Fact]
    public void MapDevicesToStoreItems_G2Device_NoSwitchNoScript_UsesFallback()
    {
        var settings = JsonSerializer.SerializeToElement(new
        {
            sys = new { device = new { id = "shellypm-XXYYZZ", mac = "XXYYZZ" } }
        });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev006", Gen = "G2", Type = "relay", Code = "SNPM-001PCEU16",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].ChannelId.Should().Be(0);
        result[0].FriendlyNames.Should().NotBeEmpty();
    }

    [Fact]
    public void MapDevicesToStoreItems_NullSettings_SkipsDevice()
    {
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev007", Gen = "G1", Type = "relay", Code = "SHSW-1",
                Settings = null
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MapDevicesToStoreItems_EmptyList_ReturnsEmptyList()
    {
        var model = new GetDevicesResponseModel();

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().BeEmpty();
    }

    [Fact]
    public void MapDevicesToStoreItems_SetsDeviceTypeFromTypeOrCode()
    {
        var settings = JsonSerializer.SerializeToElement(new { name = "Test" });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev008", Gen = "G1", Type = "relay", Code = "SHSW-1",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result[0].DeviceType.Should().Be("relay");
    }

    [Fact]
    public void MapDevicesToStoreItems_DeviceTypeFallsBackToCode_WhenTypeIsNull()
    {
        var settings = JsonSerializer.SerializeToElement(new { name = "Test" });
        var item = new GetDevicesDeviceItem
        {
            Id = "dev009", Gen = "G1", Code = "SHSW-1",
            Settings = settings
        };
        // Type defaults to string.Empty via property initializer; set to null to test fallback
        item.Type = null!;
        var model = new GetDevicesResponseModel { item };

        var result = _mapper.MapDevicesToStoreItems(model);

        result[0].DeviceType.Should().Be("SHSW-1");
    }

    [Fact]
    public void MapDevicesToStoreItems_G2SwitchEmptyName_UsesFallbackName()
    {
        var settings = JsonSerializer.SerializeToElement(new Dictionary<string, object>
        {
            ["switch:0"] = new { name = "" },
            ["sys"] = new { device = new { id = "shellypro-AABB", mac = "AABB" } }
        });
        var model = new GetDevicesResponseModel
        {
            new GetDevicesDeviceItem
            {
                Id = "dev010", Gen = "G2", Type = "relay", Code = "SNSW-001P16EU",
                Settings = settings
            }
        };

        var result = _mapper.MapDevicesToStoreItems(model);

        result.Should().HaveCount(1);
        result[0].FriendlyNames.Should().NotContain("");
        result[0].FriendlyNames.Should().Contain("shellypro");
    }

    [Fact]
    public void MapDevicesToStoreItems_FullExampleJson_ReturnsExpectedCount()
    {
        var json = TestFixtureLoader.LoadEmbeddedJson("v2-devices-get.example.json");
        var devices = JsonSerializer.Deserialize<GetDevicesResponseModel>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        var result = _mapper.MapDevicesToStoreItems(devices);

        // The example JSON has 34 devices. Each device produces at least 1 store item,
        // multi-channel G2 devices produce more. Verify we get a reasonable count.
        result.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThanOrEqualTo(devices.Count);
    }

    #endregion
}
