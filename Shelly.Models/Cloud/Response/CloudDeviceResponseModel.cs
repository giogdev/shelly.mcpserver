using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Asg.MCP.Models.Shelly
{
    public class CloudDeviceResponseModel
    {
        /// <summary>
        /// Device identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Device type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Device code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Device generation.
        /// </summary>
        [JsonPropertyName("gen")]
        public string Gen { get; set; } = string.Empty;

        /// <summary>
        /// Device online status. 1 = online, 0 = offline.
        /// </summary>
        [JsonPropertyName("online")]
        public int Online { get; set; }

        /// <summary>
        /// Device status data.
        /// </summary>
        [JsonPropertyName("status")]
        public CloudDeviceStatus? Status { get; set; }

        /// <summary>
        /// Device settings data. Present only if specified in the request.
        /// </summary>
        [JsonPropertyName("settings")]
        public JsonElement? Settings { get; set; }

        /// <summary>
        /// Gets whether the device is online.
        /// </summary>
        [JsonIgnore]
        public bool IsOnline => Online == 1;
    }

    public class CloudDeviceStatus
    {
        [JsonPropertyName("input:0")]
        public InputStatus? Input0 { get; set; }

        [JsonPropertyName("input:1")]
        public InputStatus? Input1 { get; set; }

        [JsonPropertyName("switch:0")]
        public SwitchStatus? Switch0 { get; set; }

        [JsonPropertyName("switch:1")]
        public SwitchStatus? Switch1 { get; set; }

        [JsonPropertyName("mqtt")]
        public MqttStatus? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public int Serial { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("sys")]
        public SystemStatus? Sys { get; set; }

        [JsonPropertyName("v_eve:0")]
        public VirtualEventStatus? VirtualEvent0 { get; set; }

        [JsonPropertyName("v_eve:1")]
        public VirtualEventStatus? VirtualEvent1 { get; set; }

        [JsonPropertyName("ble")]
        public JsonElement? Ble { get; set; }

        [JsonPropertyName("ws")]
        public WebSocketStatus? Ws { get; set; }

        [JsonPropertyName("cloud")]
        public CloudStatus? Cloud { get; set; }

        [JsonPropertyName("wifi")]
        public WiFiStatus? Wifi { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public double Ts { get; set; }

        // Proprietà specifiche per sensori Gen 1
        [JsonPropertyName("bat")]
        public BatteryStatus? Battery { get; set; }

        [JsonPropertyName("uptime")]
        public int Uptime { get; set; }

        [JsonPropertyName("sensor")]
        public SensorStatus? Sensor { get; set; }

        [JsonPropertyName("has_update")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("ram_free")]
        public long RamFree { get; set; }

        [JsonPropertyName("wifi_sta")]
        public WiFiStationStatus? WifiSta { get; set; }

        [JsonPropertyName("unixtime")]
        public long Unixtime { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("update")]
        public UpdateStatus? Update { get; set; }

        [JsonPropertyName("lux")]
        public LuxStatus? Lux { get; set; }

        [JsonPropertyName("ram_total")]
        public long RamTotal { get; set; }

        [JsonPropertyName("tmp")]
        public TemperatureStatus? Temperature { get; set; }

        [JsonPropertyName("actions_stats")]
        public ActionsStats? ActionsStats { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("cfg_changed_cnt")]
        public int CfgChangedCnt { get; set; }

        [JsonPropertyName("fs_free")]
        public long FsFree { get; set; }

        [JsonPropertyName("accel")]
        public AccelStatus? Accel { get; set; }

        [JsonPropertyName("act_reasons")]
        public List<string> ActReasons { get; set; } = new List<string>();

        [JsonPropertyName("sensor_error")]
        public int SensorError { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("fs_size")]
        public long FsSize { get; set; }

        [JsonPropertyName("relays")]
        public List<RelayInfo>? Relays { get; set; }
    }

    public class RelayInfo
    {
        [JsonPropertyName("ison")]
        public bool IsOn { get; set; }

        [JsonPropertyName("has_timer")]
        public bool HasTimer { get; set; }

        [JsonPropertyName("timer_started")]
        public int TimerStarted { get; set; }

        [JsonPropertyName("timer_duration")]
        public int TimerDuration { get; set; }

        [JsonPropertyName("timer_remaining")]
        public int TimerRemaining { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }


    public class InputStatus
    {
        [JsonPropertyName("state")]
        public bool State { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class SwitchStatus
    {
        [JsonPropertyName("aenergy")]
        public EnergyData? AEnergy { get; set; }

        [JsonPropertyName("ret_aenergy")]
        public EnergyData? RetAEnergy { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("output")]
        public bool Output { get; set; }

        [JsonPropertyName("apower")]
        public double APower { get; set; }

        [JsonPropertyName("voltage")]
        public double Voltage { get; set; }

        [JsonPropertyName("freq")]
        public double Freq { get; set; }

        [JsonPropertyName("current")]
        public double Current { get; set; }

        [JsonPropertyName("pf")]
        public double Pf { get; set; }

        [JsonPropertyName("temperature")]
        public TemperatureStatus? Temperature { get; set; }

        [JsonPropertyName("_attrs")]
        public SwitchAttributes? Attrs { get; set; }
    }

    public class EnergyData
    {
        [JsonPropertyName("by_minute")]
        public List<double> ByMinute { get; set; } = new List<double>();

        [JsonPropertyName("minute_ts")]
        public long MinuteTs { get; set; }

        [JsonPropertyName("total")]
        public double Total { get; set; }
    }

    public class TemperatureStatus
    {
        [JsonPropertyName("tC")]
        public double TC { get; set; }

        [JsonPropertyName("tF")]
        public double TF { get; set; }

        // Per sensori Gen 1
        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("units")]
        public string Units { get; set; } = string.Empty;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }
    }

    public class SwitchAttributes
    {
        [JsonPropertyName("inputs")]
        public List<int> Inputs { get; set; } = new List<int>();
    }

    public class MqttStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class SystemStatus
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("unixtime")]
        public long Unixtime { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("restart_required")]
        public bool RestartRequired { get; set; }

        [JsonPropertyName("last_sync_ts")]
        public long LastSyncTs { get; set; }

        [JsonPropertyName("uptime")]
        public long Uptime { get; set; }

        [JsonPropertyName("ram_size")]
        public long RamSize { get; set; }

        [JsonPropertyName("ram_free")]
        public long RamFree { get; set; }

        [JsonPropertyName("ram_min_free")]
        public long RamMinFree { get; set; }

        [JsonPropertyName("fs_size")]
        public long FsSize { get; set; }

        [JsonPropertyName("fs_free")]
        public long FsFree { get; set; }

        [JsonPropertyName("cfg_rev")]
        public int CfgRev { get; set; }

        [JsonPropertyName("kvs_rev")]
        public int KvsRev { get; set; }

        [JsonPropertyName("schedule_rev")]
        public int ScheduleRev { get; set; }

        [JsonPropertyName("webhook_rev")]
        public int WebhookRev { get; set; }

        [JsonPropertyName("btrelay_rev")]
        public int BtrelayRev { get; set; }

        [JsonPropertyName("available_updates")]
        public AvailableUpdates? AvailableUpdates { get; set; }

        [JsonPropertyName("reset_reason")]
        public int ResetReason { get; set; }

        [JsonPropertyName("utc_offset")]
        public int UtcOffset { get; set; }
    }

    public class AvailableUpdates
    {
        [JsonPropertyName("beta")]
        public UpdateInfo? Beta { get; set; }
    }

    public class UpdateInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    public class VirtualEventStatus
    {
        [JsonPropertyName("ev")]
        public string Ev { get; set; } = string.Empty;

        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class WebSocketStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class CloudStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class WiFiStatus
    {
        [JsonPropertyName("sta_ip")]
        public string StaIp { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("rssi")]
        public int Rssi { get; set; }
    }

    // Nuove classi per gestire i dati specifici dei sensori Gen 1
    public class BatteryStatus
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("voltage")]
        public double Voltage { get; set; }
    }

    public class SensorStatus
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }
    }

    public class WiFiStationStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("rssi")]
        public int Rssi { get; set; }
    }

    public class UpdateStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("has_update")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("new_version")]
        public string NewVersion { get; set; } = string.Empty;

        [JsonPropertyName("old_version")]
        public string OldVersion { get; set; } = string.Empty;
    }

    public class LuxStatus
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("illumination")]
        public string Illumination { get; set; } = string.Empty;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }
    }

    public class ActionsStats
    {
        [JsonPropertyName("skipped")]
        public int Skipped { get; set; }
    }

    public class AccelStatus
    {
        [JsonPropertyName("tilt")]
        public int Tilt { get; set; }

        [JsonPropertyName("vibration")]
        public int Vibration { get; set; }
    }
}