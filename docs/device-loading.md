# Device loading and store initialization

## Overview

At startup, both the MCP Server and the API Gateway initialize a **singleton `ShellyCloudDeviceStore`** that holds the known devices. Loading happens in two sequential steps: local file → Shelly Cloud API.

---

## Step 1 — Local mapping file

`ShellyCloudDeviceStore` reads a JSON file at construction time (before the app starts serving requests).

| Config key | Description |
|---|---|
| `DeviceMappingFile` | Absolute path to the JSON file. If empty or omitted, the store starts empty. |

The file format is defined by the template at `docs/resources/template/devices.template.json`. Each entry maps a physical device ID to one or more **friendly names** (used by the MCP tools and REST endpoints to resolve natural-language references).

```json
[
  {
    "DeviceId": "your-device-id",
    "ChannelId": 0,
    "FriendlyNames": ["kitchen", "kitchen light"],
    "DeviceType": "switch"
  }
]
```

---

## Step 2 — Shelly Cloud API

Immediately after the application is started, both hosts (MCP and API) call:

```csharp
await shellyService.FetchAndPopulateDevicesAsync();
```

This fetches all devices from the Shelly Cloud API (`/v2/devices/get`) and calls `UpdateStore()`.

---

## Merge logic

`UpdateStore()` reconciles API data with the existing store on a per-device basis (matched by `DeviceId`):

| Scenario | Result |
|---|---|
| Device exists in file **and** API | `FriendlyNames` → union of both; `ChannelId` and `DeviceType` overwritten by API values |
| Device only in API | Added to the store as-is |
| Device only in file | Kept as-is (API did not return it) |

**Priority summary:** The file is the source of truth for friendly names and offline devices. The API enriches and overwrites structural metadata (`ChannelId`, `DeviceType`) and adds any devices not listed in the file.

