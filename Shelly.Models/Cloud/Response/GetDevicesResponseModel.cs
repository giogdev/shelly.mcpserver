using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shelly.Models.Cloud.Response
{
    /// <summary>
    /// Response model for GET /v2/devices/get.
    /// The API returns a JSON array; this type is a typed list of device items.
    /// </summary>
    public class GetDevicesResponseModel : List<GetDevicesDeviceItem> { }

    /// <summary>
    /// A single Shelly device as returned by the cloud API.
    /// <para>
    /// <c>Status</c> and <c>Settings</c> are kept as <see cref="JsonElement"/> because their
    /// structure varies by generation (G1 / G2 / GBLE) and device type (relay / sensor /
    /// inputs_reader). Use the strongly-typed status and settings classes in this file to
    /// deserialize them when the gen/type combination is known.
    /// </para>
    /// </summary>
    public class GetDevicesDeviceItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>relay | sensor | inputs_reader</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>G1 | G2 | GBLE</summary>
        [JsonPropertyName("gen")]
        public string Gen { get; set; } = string.Empty;

        /// <summary>1 = online, 0 = offline.</summary>
        [JsonPropertyName("online")]
        public int Online { get; set; }

        [JsonPropertyName("pending_cmds")]
        public int? PendingCmds { get; set; }

        /// <summary>1 = sleeping, 0 = awake. Present only for battery-powered devices.</summary>
        [JsonPropertyName("sleeping")]
        public int? Sleeping { get; set; }

        [JsonPropertyName("status")]
        public JsonElement? Status { get; set; }

        [JsonPropertyName("settings")]
        public JsonElement? Settings { get; set; }

        [JsonIgnore]
        public bool IsOnline => Online == 1;
    }

    // ── G1 shared types ──────────────────────────────────────────────────────────

    public class GetDevicesG1WifiStaStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("rssi")]
        public double Rssi { get; set; }
    }

    public class GetDevicesG1CloudStatus
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class GetDevicesG1UpdateStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("has_update")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("new_version")]
        public string NewVersion { get; set; } = string.Empty;

        [JsonPropertyName("old_version")]
        public string OldVersion { get; set; } = string.Empty;

        [JsonPropertyName("beta_version")]
        public string? BetaVersion { get; set; }
    }

    public class GetDevicesG1InputItem
    {
        [JsonPropertyName("input")]
        public double Input { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("event_cnt")]
        public double EventCnt { get; set; }
    }

    public class GetDevicesG1RelayMeterItem
    {
        [JsonPropertyName("power")]
        public double Power { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        /// <summary>Can be a number or a boolean depending on firmware version.</summary>
        [JsonPropertyName("overpower")]
        public JsonElement? Overpower { get; set; }

        [JsonPropertyName("timestamp")]
        public double? Timestamp { get; set; }

        [JsonPropertyName("counters")]
        public List<double>? Counters { get; set; }

        [JsonPropertyName("total")]
        public double? Total { get; set; }
    }

    public class GetDevicesG1RelayRuntimeItem
    {
        [JsonPropertyName("ison")]
        public bool IsOn { get; set; }

        [JsonPropertyName("has_timer")]
        public bool HasTimer { get; set; }

        [JsonPropertyName("timer_started")]
        public double TimerStarted { get; set; }

        [JsonPropertyName("timer_duration")]
        public double TimerDuration { get; set; }

        [JsonPropertyName("timer_remaining")]
        public double TimerRemaining { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("overpower")]
        public bool? Overpower { get; set; }
    }

    public class GetDevicesG1ActionsStats
    {
        [JsonPropertyName("skipped")]
        public double Skipped { get; set; }
    }

    public class GetDevicesG1WifiStaSettings
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("ssid")]
        public string? Ssid { get; set; }

        [JsonPropertyName("ipv4_method")]
        public string Ipv4Method { get; set; } = string.Empty;

        [JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [JsonPropertyName("gw")]
        public string? Gw { get; set; }

        [JsonPropertyName("mask")]
        public string? Mask { get; set; }

        [JsonPropertyName("dns")]
        public string? Dns { get; set; }
    }

    public class GetDevicesG1WifiApSettings
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }

    public class GetDevicesG1SntpSettings
    {
        [JsonPropertyName("server")]
        public string Server { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class GetDevicesG1MqttSettings
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }

        [JsonPropertyName("server")]
        public string Server { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reconnect_timeout_max")]
        public double ReconnectTimeoutMax { get; set; }

        [JsonPropertyName("reconnect_timeout_min")]
        public double ReconnectTimeoutMin { get; set; }

        [JsonPropertyName("clean_session")]
        public bool CleanSession { get; set; }

        [JsonPropertyName("keep_alive")]
        public double KeepAlive { get; set; }

        [JsonPropertyName("max_qos")]
        public double MaxQos { get; set; }

        [JsonPropertyName("retain")]
        public bool Retain { get; set; }

        [JsonPropertyName("update_period")]
        public double UpdatePeriod { get; set; }
    }

    public class GetDevicesG1ActionsSettings
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new();
    }

    public class GetDevicesG1MqttConnected
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    // ── G1 Relay Status ───────────────────────────────────────────────────────────

    public class GetDevicesG1RelayStatus
    {
        [JsonPropertyName("mqtt")]
        public GetDevicesG1MqttConnected? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("inputs")]
        public List<GetDevicesG1InputItem> Inputs { get; set; } = new();

        [JsonPropertyName("meters")]
        public List<GetDevicesG1RelayMeterItem>? Meters { get; set; }

        [JsonPropertyName("uptime")]
        public double Uptime { get; set; }

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("has_update")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("ram_free")]
        public double RamFree { get; set; }

        [JsonPropertyName("wifi_sta")]
        public GetDevicesG1WifiStaStatus? WifiSta { get; set; }

        [JsonPropertyName("unixtime")]
        public double Unixtime { get; set; }

        /// <summary>Can be a string or a number depending on firmware version.</summary>
        [JsonPropertyName("mac")]
        public JsonElement? Mac { get; set; }

        [JsonPropertyName("update")]
        public GetDevicesG1UpdateStatus? Update { get; set; }

        [JsonPropertyName("ram_total")]
        public double RamTotal { get; set; }

        [JsonPropertyName("relays")]
        public List<GetDevicesG1RelayRuntimeItem>? Relays { get; set; }

        [JsonPropertyName("actions_stats")]
        public GetDevicesG1ActionsStats? ActionsStats { get; set; }

        [JsonPropertyName("cfg_changed_cnt")]
        public double CfgChangedCnt { get; set; }

        [JsonPropertyName("fs_free")]
        public double FsFree { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG1CloudStatus? Cloud { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("fs_size")]
        public double FsSize { get; set; }

        [JsonPropertyName("overtemperature")]
        public bool? Overtemperature { get; set; }

        [JsonPropertyName("temperature_status")]
        public string? TemperatureStatus { get; set; }

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }
    }

    // ── G1 Relay Settings ─────────────────────────────────────────────────────────

    public class GetDevicesG1RelaySettings
    {
        [JsonPropertyName("tz_dst_auto")]
        public bool TzDstAuto { get; set; }

        [JsonPropertyName("debug_enable")]
        public bool DebugEnable { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("eco_mode_enabled")]
        public bool EcoModeEnabled { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("wifi_sta")]
        public GetDevicesG1WifiStaSettings? WifiSta { get; set; }

        [JsonPropertyName("wifi_sta1")]
        public GetDevicesG1WifiStaSettings? WifiSta1 { get; set; }

        [JsonPropertyName("wifi_ap")]
        public GetDevicesG1WifiApSettings? WifiAp { get; set; }

        [JsonPropertyName("device")]
        public JsonElement? Device { get; set; }

        [JsonPropertyName("tz_utc_offset")]
        public double TzUtcOffset { get; set; }

        [JsonPropertyName("coiot")]
        public JsonElement? Coiot { get; set; }

        [JsonPropertyName("factory_reset_from_switch")]
        public bool FactoryResetFromSwitch { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonPropertyName("tz_dst")]
        public bool TzDst { get; set; }

        [JsonPropertyName("build_info")]
        public JsonElement? BuildInfo { get; set; }

        [JsonPropertyName("allow_cross_origin")]
        public bool AllowCrossOrigin { get; set; }

        [JsonPropertyName("pin_code")]
        public string PinCode { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        public JsonElement? Login { get; set; }

        [JsonPropertyName("longpush_time")]
        public double LongpushTime { get; set; }

        [JsonPropertyName("mqtt")]
        public GetDevicesG1MqttSettings? Mqtt { get; set; }

        [JsonPropertyName("sntp")]
        public GetDevicesG1SntpSettings? Sntp { get; set; }

        [JsonPropertyName("actions")]
        public GetDevicesG1ActionsSettings? Actions { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG1CloudStatus? Cloud { get; set; }

        [JsonPropertyName("fw")]
        public string Fw { get; set; } = string.Empty;

        [JsonPropertyName("relays")]
        public JsonElement? Relays { get; set; }

        [JsonPropertyName("max_power")]
        public double? MaxPower { get; set; }

        [JsonPropertyName("supply_voltage")]
        public double? SupplyVoltage { get; set; }

        [JsonPropertyName("power_correction")]
        public double? PowerCorrection { get; set; }

        [JsonPropertyName("led_status_disable")]
        public bool? LedStatusDisable { get; set; }
    }

    // ── G1 Sensor Status ──────────────────────────────────────────────────────────

    public class GetDevicesG1SensorStatus
    {
        [JsonPropertyName("mqtt")]
        public GetDevicesG1MqttConnected? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("bat")]
        public JsonElement? Bat { get; set; }

        [JsonPropertyName("uptime")]
        public double Uptime { get; set; }

        [JsonPropertyName("sensor")]
        public JsonElement? Sensor { get; set; }

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("has_update")]
        public bool HasUpdate { get; set; }

        [JsonPropertyName("ram_free")]
        public double RamFree { get; set; }

        [JsonPropertyName("wifi_sta")]
        public GetDevicesG1WifiStaStatus? WifiSta { get; set; }

        [JsonPropertyName("unixtime")]
        public double Unixtime { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("update")]
        public GetDevicesG1UpdateStatus? Update { get; set; }

        [JsonPropertyName("lux")]
        public JsonElement? Lux { get; set; }

        [JsonPropertyName("ram_total")]
        public double RamTotal { get; set; }

        [JsonPropertyName("tmp")]
        public JsonElement? Tmp { get; set; }

        [JsonPropertyName("actions_stats")]
        public GetDevicesG1ActionsStats? ActionsStats { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("cfg_changed_cnt")]
        public double CfgChangedCnt { get; set; }

        [JsonPropertyName("fs_free")]
        public double? FsFree { get; set; }

        [JsonPropertyName("accel")]
        public JsonElement? Accel { get; set; }

        [JsonPropertyName("act_reasons")]
        public List<string>? ActReasons { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG1CloudStatus? Cloud { get; set; }

        [JsonPropertyName("sensor_error")]
        public double SensorError { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("fs_size")]
        public double FsSize { get; set; }
    }

    // ── G1 Sensor Settings ────────────────────────────────────────────────────────

    public class GetDevicesG1SensorSettings
    {
        [JsonPropertyName("tz_dst_auto")]
        public bool TzDstAuto { get; set; }

        [JsonPropertyName("debug_enable")]
        public bool DebugEnable { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("lng")]
        public double? Lng { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("wifi_sta")]
        public GetDevicesG1WifiStaSettings? WifiSta { get; set; }

        [JsonPropertyName("wifi_sta1")]
        public GetDevicesG1WifiStaSettings? WifiSta1 { get; set; }

        [JsonPropertyName("wifi_ap")]
        public GetDevicesG1WifiApSettings? WifiAp { get; set; }

        [JsonPropertyName("device")]
        public JsonElement? Device { get; set; }

        [JsonPropertyName("tz_utc_offset")]
        public double TzUtcOffset { get; set; }

        [JsonPropertyName("coiot")]
        public JsonElement? Coiot { get; set; }

        [JsonPropertyName("tz_dst")]
        public bool TzDst { get; set; }

        [JsonPropertyName("sensors")]
        public JsonElement? Sensors { get; set; }

        [JsonPropertyName("temperature_offset")]
        public double? TemperatureOffset { get; set; }

        [JsonPropertyName("build_info")]
        public JsonElement? BuildInfo { get; set; }

        [JsonPropertyName("sleep_mode")]
        public JsonElement? SleepMode { get; set; }

        [JsonPropertyName("allow_cross_origin")]
        public bool AllowCrossOrigin { get; set; }

        [JsonPropertyName("pin_code")]
        public string PinCode { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        public JsonElement? Login { get; set; }

        [JsonPropertyName("mqtt")]
        public GetDevicesG1MqttSettings? Mqtt { get; set; }

        [JsonPropertyName("sntp")]
        public GetDevicesG1SntpSettings? Sntp { get; set; }

        [JsonPropertyName("actions")]
        public GetDevicesG1ActionsSettings? Actions { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG1CloudStatus? Cloud { get; set; }

        [JsonPropertyName("fw")]
        public string Fw { get; set; } = string.Empty;

        [JsonPropertyName("tzautodetect")]
        public bool Tzautodetect { get; set; }

        [JsonPropertyName("discoverable")]
        public bool Discoverable { get; set; }

        [JsonPropertyName("led_status_disable")]
        public bool? LedStatusDisable { get; set; }

        [JsonPropertyName("tilt_calibrated")]
        public bool? TiltCalibrated { get; set; }

        [JsonPropertyName("tilt_enabled")]
        public bool? TiltEnabled { get; set; }

        [JsonPropertyName("vibration_enabled")]
        public bool? VibrationEnabled { get; set; }

        [JsonPropertyName("vibration_sensitivity")]
        public double? VibrationSensitivity { get; set; }

        [JsonPropertyName("dark_threshold")]
        public double? DarkThreshold { get; set; }

        [JsonPropertyName("twilight_threshold")]
        public double? TwilightThreshold { get; set; }

        [JsonPropertyName("reverse_open_close")]
        public bool? ReverseOpenClose { get; set; }

        [JsonPropertyName("lux_wakeup_enable")]
        public bool? LuxWakeupEnable { get; set; }
    }

    // ── G2 shared types ──────────────────────────────────────────────────────────

    public class GetDevicesG2SysStatus
    {
        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("unixtime")]
        public double? Unixtime { get; set; }

        [JsonPropertyName("restart_required")]
        public bool? RestartRequired { get; set; }

        [JsonPropertyName("last_sync_ts")]
        public double? LastSyncTs { get; set; }

        [JsonPropertyName("uptime")]
        public double? Uptime { get; set; }

        [JsonPropertyName("ram_size")]
        public double? RamSize { get; set; }

        [JsonPropertyName("ram_free")]
        public double? RamFree { get; set; }

        [JsonPropertyName("ram_min_free")]
        public double? RamMinFree { get; set; }

        [JsonPropertyName("fs_size")]
        public double? FsSize { get; set; }

        [JsonPropertyName("fs_free")]
        public double? FsFree { get; set; }

        [JsonPropertyName("cfg_rev")]
        public double? CfgRev { get; set; }

        [JsonPropertyName("kvs_rev")]
        public double? KvsRev { get; set; }

        [JsonPropertyName("schedule_rev")]
        public double? ScheduleRev { get; set; }

        [JsonPropertyName("webhook_rev")]
        public double? WebhookRev { get; set; }

        [JsonPropertyName("btrelay_rev")]
        public double? BtrelayRev { get; set; }

        [JsonPropertyName("available_updates")]
        public JsonElement? AvailableUpdates { get; set; }

        [JsonPropertyName("reset_reason")]
        public double? ResetReason { get; set; }

        [JsonPropertyName("utc_offset")]
        public double? UtcOffset { get; set; }

        [JsonPropertyName("wakeup_reason")]
        public JsonElement? WakeupReason { get; set; }

        [JsonPropertyName("wakeup_period")]
        public double? WakeupPeriod { get; set; }
    }

    public class GetDevicesG2WifiStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("rssi")]
        public double Rssi { get; set; }

        [JsonPropertyName("sta_ip")]
        public string? StaIp { get; set; }

        [JsonPropertyName("sta_ip6")]
        public string? StaIp6 { get; set; }

        [JsonPropertyName("bssid")]
        public string? Bssid { get; set; }
    }

    public class GetDevicesG2CloudStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class GetDevicesG2MqttStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class GetDevicesG2WsStatus
    {
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }
    }

    public class GetDevicesG2InputStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("state")]
        public bool? State { get; set; }

        [JsonPropertyName("percent")]
        public double? Percent { get; set; }

        [JsonPropertyName("errors")]
        public List<JsonElement>? Errors { get; set; }
    }

    public class GetDevicesG2VEventStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("ev")]
        public string? Ev { get; set; }

        [JsonPropertyName("ttl")]
        public double? Ttl { get; set; }
    }

    public class GetDevicesG2SwitchStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("output")]
        public bool? Output { get; set; }

        [JsonPropertyName("apower")]
        public double? APower { get; set; }

        [JsonPropertyName("voltage")]
        public double? Voltage { get; set; }

        [JsonPropertyName("freq")]
        public double? Freq { get; set; }

        [JsonPropertyName("current")]
        public double? Current { get; set; }

        [JsonPropertyName("pf")]
        public double? Pf { get; set; }

        [JsonPropertyName("temperature")]
        public JsonElement? Temperature { get; set; }

        [JsonPropertyName("aenergy")]
        public JsonElement? AEnergy { get; set; }

        [JsonPropertyName("ret_aenergy")]
        public JsonElement? RetAEnergy { get; set; }

        [JsonPropertyName("_attrs")]
        public JsonElement? Attrs { get; set; }

        [JsonPropertyName("timer_duration")]
        public double? TimerDuration { get; set; }

        [JsonPropertyName("timer_started_at")]
        public double? TimerStartedAt { get; set; }
    }

    public class GetDevicesG2ScriptStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("running")]
        public bool Running { get; set; }

        [JsonPropertyName("mem_used")]
        public double? MemUsed { get; set; }

        [JsonPropertyName("mem_peak")]
        public double? MemPeak { get; set; }

        [JsonPropertyName("mem_free")]
        public double? MemFree { get; set; }

        [JsonPropertyName("cpu")]
        public double? Cpu { get; set; }
    }

    public class GetDevicesG2PmStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("aenergy")]
        public JsonElement? AEnergy { get; set; }

        [JsonPropertyName("ret_aenergy")]
        public JsonElement? RetAEnergy { get; set; }

        [JsonPropertyName("apower")]
        public double? APower { get; set; }

        [JsonPropertyName("current")]
        public double? Current { get; set; }

        [JsonPropertyName("freq")]
        public double? Freq { get; set; }

        [JsonPropertyName("voltage")]
        public double? Voltage { get; set; }
    }

    public class GetDevicesG2EmStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("act_power")]
        public double? ActPower { get; set; }

        [JsonPropertyName("aprt_power")]
        public double? AprtPower { get; set; }

        [JsonPropertyName("current")]
        public double? Current { get; set; }

        [JsonPropertyName("freq")]
        public double? Freq { get; set; }

        [JsonPropertyName("pf")]
        public double? Pf { get; set; }

        [JsonPropertyName("voltage")]
        public double? Voltage { get; set; }

        [JsonPropertyName("calibration")]
        public string? Calibration { get; set; }
    }

    public class GetDevicesG2EmDataStatus
    {
        [JsonPropertyName("id")]
        public double Id { get; set; }

        [JsonPropertyName("total_act_energy")]
        public double? TotalActEnergy { get; set; }

        [JsonPropertyName("total_act_ret_energy")]
        public double? TotalActRetEnergy { get; set; }
    }

    // ── G2 Relay Status ───────────────────────────────────────────────────────────
    // Pattern properties (switch:N, input:N, …) are mapped for indices 0–3.
    // Higher indices, if ever returned, will be silently ignored by the deserializer.

    public class GetDevicesG2RelayStatus
    {
        [JsonPropertyName("mqtt")]
        public GetDevicesG2MqttStatus? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("sys")]
        public GetDevicesG2SysStatus? Sys { get; set; }

        [JsonPropertyName("ble")]
        public JsonElement? Ble { get; set; }

        [JsonPropertyName("ws")]
        public GetDevicesG2WsStatus? Ws { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG2CloudStatus? Cloud { get; set; }

        [JsonPropertyName("wifi")]
        public GetDevicesG2WifiStatus? Wifi { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public double? Ts { get; set; }

        [JsonPropertyName("bthome")]
        public JsonElement? Bthome { get; set; }

        [JsonPropertyName("zigbee")]
        public JsonElement? Zigbee { get; set; }

        [JsonPropertyName("matter")]
        public JsonElement? Matter { get; set; }

        [JsonPropertyName("input:0")]
        public GetDevicesG2InputStatus? Input0 { get; set; }

        [JsonPropertyName("input:1")]
        public GetDevicesG2InputStatus? Input1 { get; set; }

        [JsonPropertyName("input:2")]
        public GetDevicesG2InputStatus? Input2 { get; set; }

        [JsonPropertyName("input:3")]
        public GetDevicesG2InputStatus? Input3 { get; set; }

        [JsonPropertyName("v_eve:0")]
        public GetDevicesG2VEventStatus? VEve0 { get; set; }

        [JsonPropertyName("v_eve:1")]
        public GetDevicesG2VEventStatus? VEve1 { get; set; }

        [JsonPropertyName("switch:0")]
        public GetDevicesG2SwitchStatus? Switch0 { get; set; }

        [JsonPropertyName("switch:1")]
        public GetDevicesG2SwitchStatus? Switch1 { get; set; }

        [JsonPropertyName("switch:2")]
        public GetDevicesG2SwitchStatus? Switch2 { get; set; }

        [JsonPropertyName("switch:3")]
        public GetDevicesG2SwitchStatus? Switch3 { get; set; }

        [JsonPropertyName("script:0")]
        public GetDevicesG2ScriptStatus? Script0 { get; set; }

        [JsonPropertyName("script:1")]
        public GetDevicesG2ScriptStatus? Script1 { get; set; }

        [JsonPropertyName("script:2")]
        public GetDevicesG2ScriptStatus? Script2 { get; set; }

        [JsonPropertyName("pm1:0")]
        public GetDevicesG2PmStatus? Pm10 { get; set; }

        [JsonPropertyName("pm1:1")]
        public GetDevicesG2PmStatus? Pm11 { get; set; }

        [JsonPropertyName("em1:0")]
        public GetDevicesG2EmStatus? Em10 { get; set; }

        [JsonPropertyName("em1:1")]
        public GetDevicesG2EmStatus? Em11 { get; set; }

        [JsonPropertyName("em1data:0")]
        public GetDevicesG2EmDataStatus? Em1Data0 { get; set; }

        [JsonPropertyName("em1data:1")]
        public GetDevicesG2EmDataStatus? Em1Data1 { get; set; }
    }

    // ── G2 InputsReader Status ────────────────────────────────────────────────────

    public class GetDevicesG2InputsReaderStatus
    {
        [JsonPropertyName("mqtt")]
        public GetDevicesG2MqttStatus? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("sys")]
        public GetDevicesG2SysStatus? Sys { get; set; }

        [JsonPropertyName("ble")]
        public JsonElement? Ble { get; set; }

        [JsonPropertyName("ws")]
        public GetDevicesG2WsStatus? Ws { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG2CloudStatus? Cloud { get; set; }

        [JsonPropertyName("wifi")]
        public GetDevicesG2WifiStatus? Wifi { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public double? Ts { get; set; }

        [JsonPropertyName("input:0")]
        public GetDevicesG2InputStatus? Input0 { get; set; }

        [JsonPropertyName("input:1")]
        public GetDevicesG2InputStatus? Input1 { get; set; }

        [JsonPropertyName("input:2")]
        public GetDevicesG2InputStatus? Input2 { get; set; }

        [JsonPropertyName("input:3")]
        public GetDevicesG2InputStatus? Input3 { get; set; }

        [JsonPropertyName("v_eve:0")]
        public GetDevicesG2VEventStatus? VEve0 { get; set; }

        [JsonPropertyName("v_eve:1")]
        public GetDevicesG2VEventStatus? VEve1 { get; set; }

        [JsonPropertyName("script:0")]
        public GetDevicesG2ScriptStatus? Script0 { get; set; }

        [JsonPropertyName("script:1")]
        public GetDevicesG2ScriptStatus? Script1 { get; set; }
    }

    // ── G2 Sensor Status ──────────────────────────────────────────────────────────

    public class GetDevicesG2SensorStatus
    {
        [JsonPropertyName("mqtt")]
        public GetDevicesG2MqttStatus? Mqtt { get; set; }

        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("sys")]
        public GetDevicesG2SysStatus? Sys { get; set; }

        [JsonPropertyName("ble")]
        public JsonElement? Ble { get; set; }

        [JsonPropertyName("ws")]
        public GetDevicesG2WsStatus? Ws { get; set; }

        [JsonPropertyName("cloud")]
        public GetDevicesG2CloudStatus? Cloud { get; set; }

        [JsonPropertyName("wifi")]
        public GetDevicesG2WifiStatus? Wifi { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public double? Ts { get; set; }

        [JsonPropertyName("zigbee")]
        public JsonElement? Zigbee { get; set; }

        [JsonPropertyName("matter")]
        public JsonElement? Matter { get; set; }

        [JsonPropertyName("devicepower:0")]
        public JsonElement? DevicePower0 { get; set; }

        [JsonPropertyName("devicepower:1")]
        public JsonElement? DevicePower1 { get; set; }

        [JsonPropertyName("flood:0")]
        public JsonElement? Flood0 { get; set; }

        [JsonPropertyName("flood:1")]
        public JsonElement? Flood1 { get; set; }
    }

    // ── G2 Settings Base ──────────────────────────────────────────────────────────
    // Shared by G2 relay, G2 inputs_reader and G2 sensor devices.

    public class GetDevicesG2SettingsBase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("_updated")]
        public string? Updated { get; set; }

        [JsonPropertyName("_gip_c")]
        public string? GipC { get; set; }

        [JsonPropertyName("mqtt")]
        public JsonElement? Mqtt { get; set; }

        [JsonPropertyName("ble")]
        public JsonElement? Ble { get; set; }

        [JsonPropertyName("ws")]
        public JsonElement? Ws { get; set; }

        [JsonPropertyName("sys")]
        public JsonElement? Sys { get; set; }

        [JsonPropertyName("device")]
        public JsonElement? Device { get; set; }

        [JsonPropertyName("DeviceInfo")]
        public JsonElement? DeviceInfo { get; set; }

        [JsonPropertyName("cloud")]
        public JsonElement? Cloud { get; set; }

        [JsonPropertyName("wifi")]
        public JsonElement? Wifi { get; set; }

        [JsonPropertyName("sleep_mode")]
        public JsonElement? SleepMode { get; set; }

        [JsonPropertyName("zigbee")]
        public JsonElement? Zigbee { get; set; }

        [JsonPropertyName("matter")]
        public JsonElement? Matter { get; set; }

        [JsonPropertyName("input:0")]
        public JsonElement? Input0 { get; set; }

        [JsonPropertyName("input:1")]
        public JsonElement? Input1 { get; set; }

        [JsonPropertyName("input:2")]
        public JsonElement? Input2 { get; set; }

        [JsonPropertyName("input:3")]
        public JsonElement? Input3 { get; set; }

        [JsonPropertyName("switch:0")]
        public JsonElement? Switch0 { get; set; }

        [JsonPropertyName("switch:1")]
        public JsonElement? Switch1 { get; set; }

        [JsonPropertyName("switch:2")]
        public JsonElement? Switch2 { get; set; }

        [JsonPropertyName("switch:3")]
        public JsonElement? Switch3 { get; set; }

        [JsonPropertyName("script:0")]
        public JsonElement? Script0 { get; set; }

        [JsonPropertyName("script:1")]
        public JsonElement? Script1 { get; set; }

        [JsonPropertyName("pm1:0")]
        public JsonElement? Pm10 { get; set; }

        [JsonPropertyName("pm1:1")]
        public JsonElement? Pm11 { get; set; }

        [JsonPropertyName("em1:0")]
        public JsonElement? Em10 { get; set; }

        [JsonPropertyName("em1:1")]
        public JsonElement? Em11 { get; set; }

        [JsonPropertyName("em1data:0")]
        public JsonElement? Em1Data0 { get; set; }

        [JsonPropertyName("em1data:1")]
        public JsonElement? Em1Data1 { get; set; }

        [JsonPropertyName("flood:0")]
        public JsonElement? Flood0 { get; set; }

        [JsonPropertyName("devicepower:0")]
        public JsonElement? DevicePower0 { get; set; }
    }

    // ── GBLE types ────────────────────────────────────────────────────────────────
    // BLE gateway devices expose a dynamic set of sensor readings via pattern
    // properties matching ^[A-Za-z_]+:[0-9]+$. JsonExtensionData captures them all.

    public class GetDevicesGBLEStatus
    {
        [JsonPropertyName("serial")]
        public double Serial { get; set; }

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonPropertyName("reporter")]
        public JsonElement? Reporter { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class GetDevicesGBLESettings
    {
        [JsonPropertyName("device")]
        public JsonElement? Device { get; set; }

        [JsonPropertyName("_updated")]
        public string Updated { get; set; } = string.Empty;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
