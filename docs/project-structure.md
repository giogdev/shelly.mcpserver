# Project Structure

The solution is composed of five projects with clear separation of concerns.

```
shelly-integrations/
├── Shelly.ApiGateway/
├── Shelly.McpServer/
├── Shelly.Models/
├── Shelly.Services/
└── Shelly.Test/
```

---

## Shelly.Models

**Class library** — shared data contracts used across all other projects.

- Cloud API request/response models (`Cloud/Request`, `Cloud/Response`)
- `GenericDeviceStatusModel`, `DefaultResponse`, `ApiErrorResponse`
- `DeviceNameMappingStore` — in-memory store mapping physical device IDs to friendly names
- `Exceptions/` — domain exceptions (e.g. `ShellyCloudApiException`)

No business logic, no dependencies on other solution projects.

---

## Shelly.Services

**Class library** — all business logic and integration with the Shelly Cloud API.

- `ShellyCloudService` — main service: device listing, switch control, real-time status
- `ShellyCloudService.Statistics` (partial class) — historical and power consumption data
- `ShellyCloudDeviceStore` — initializes the device store at startup (see [Device Loading](device-loading.md))
- `IShellyCloudService` — public interface consumed by both entry-point projects
- `Mapper/`, `Utils/` — mapping helpers and utilities

---

## Shelly.ApiGateway

**ASP.NET Core Minimal API** — REST HTTP gateway for Shelly devices.

- `DeviceEndpoints` — list and get device info
- `SwitchEndpoints` — turn on/off, toggle
- `StatisticsEndpoints` — historical data and power consumption
- Configured via `appsettings.json` / `appsettings.Development.json`

Depends on `Shelly.Services` and `Shelly.Models`.

---

## Shelly.McpServer

**MCP (Model Context Protocol) server** — exposes Shelly devices as tools for AI assistants.

- `CloudMcpTools` — MCP tool definitions wrapping `IShellyCloudService`
- Configured via `appsettings.json`

Depends on `Shelly.Services` and `Shelly.Models`.

---

## Shelly.Test

**xUnit test project** — unit tests for services and mapping logic.

- `Services/` — tests for `ShellyCloudService` and `ShellyCloudDeviceStore`
- `Mapper/` — tests for mapping functions
- `Fixtures/` and `Helpers/` — shared test infrastructure

Depends on `Shelly.Services` and `Shelly.Models`.
