# Shelly API Gateway — Documentation

The **Shelly API Gateway** is an optional REST API built in .NET 10 that exposes your Shelly Cloud devices over HTTP. It is useful for integrating with home automation platforms, dashboards, or any system that speaks REST but does not support the Model Context Protocol.

---

## Overview

The API Gateway is a **separate, independent service** from the MCP server. Both services share the same `devices.json` configuration and Shelly Cloud credentials, but run in separate containers and can be deployed independently.

To skip the REST API entirely, simply do not start the `shelly-api-gateway` service (see [Run with Docker](#run-with-docker) below), or set `EnableApiGateway: false` in `appsettings.json`.

---

## Endpoints

Base URL: `http://<host>:8080`

### Devices

#### `GET /api/devices`

Returns the full list of configured devices from the local store (no cloud call).

**Response `200 OK`:**
```json
[
  {
    "deviceId": "shellyplus1pm-441793d48064",
    "channelId": 0,
    "friendlyNames": ["living room", "salotto"],
    "deviceType": "switch"
  }
]
```

---

#### `GET /api/devices/{deviceId}/status`

Fetches the live status of a device from Shelly Cloud.

**Path parameters:**
- `deviceId` — the Shelly device ID (as defined in `devices.json`)

**Response `200 OK`:** live device status object
**Response `404 Not Found`:** device ID not found in local store
**Response `502 Bad Gateway`:** Shelly Cloud returned no data

---

### Switch

#### `POST /api/devices/{deviceId}/switch`

Turns a switch-capable device on or off.

**Path parameters:**
- `deviceId` — the Shelly device ID

**Request body:**
```json
{
  "on": true,
  "delaySeconds": 30
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `on` | boolean | yes | `true` = ON, `false` = OFF |
| `delaySeconds` | integer | no | If > 0, the device reverts to the opposite state after this many seconds |

**Response `200 OK`:**
```json
{ "isSuccess": true }
```

**Response `404 Not Found`:** device ID not found in local store

---

### Statistics

#### `GET /api/devices/{deviceId}/weather-statistics`

Retrieves historical temperature and humidity data for a weather station device.

**Path parameters:**
- `deviceId` — the Shelly device ID

**Query parameters:**
- `dateFrom` — start of the period (UTC, ISO 8601)
- `dateTo` — end of the period (UTC, ISO 8601)

**Example:**
```
GET /api/devices/shellyht-abc123/weather-statistics?dateFrom=2025-01-01T00:00:00Z&dateTo=2025-01-07T23:59:59Z
```

**Response `200 OK`:** weather statistics payload
**Response `404 Not Found`:** device not recognised as a weather station
**Response `400 Bad Request`:** invalid request parameters

---

#### `GET /api/devices/{deviceId}/power-statistics`

Retrieves historical energy consumption data for a power-metering device.

**Path parameters:**
- `deviceId` — the Shelly device ID

**Query parameters:**
- `dateFrom` — start of the period (UTC, ISO 8601)
- `dateTo` — end of the period (UTC, ISO 8601)

**Response `200 OK`:** power consumption statistics payload
**Response `404 Not Found`:** device not recognised
**Response `400 Bad Request`:** invalid request parameters

---

## Interactive API reference

When running in **Development** mode, the Scalar interactive API reference is available at:

```
http://localhost:8080/scalar
```

---

## Configuration

The API Gateway uses the same `devices.json` and environment variables as the MCP server:

| Variable | Description |
|----------|-------------|
| `SHELLY_API_KEY` | Your Shelly Cloud authorisation key |
| `SHELLY_API_ENDPOINT` | Your Shelly Cloud server URI (e.g. `https://shelly-52-eu.shelly.cloud`) |
| `DeviceMappingFile` | Path to your `devices.json` file |

---

## Run with Docker

> Docker and Docker Compose are required.

The API Gateway is defined as a separate service in `docker/docker-compose.yml`. To start it alongside the MCP server:

```bash
docker-compose up -d
```

To start only the API Gateway (without the MCP server):

```bash
docker-compose up -d shelly-api-gateway
```

To run only the MCP server and skip the REST API entirely:

```bash
docker-compose up -d shelly-cloud-mcp-server
```

The API Gateway listens on port **8080** by default.

### docker-compose service excerpt

```yaml
shelly-api-gateway:
  image: giogdev/shelly-api-gateway:latest
  container_name: shelly-api-gateway
  restart: unless-stopped
  ports:
    - "8080:8080"
  environment:
    - SHELLY_API_ENDPOINT=${SHELLY_API_ENDPOINT}
    - SHELLY_API_KEY=${SHELLY_API_KEY}
    - DeviceMappingFile=/app/devices.json
  volumes:
    - '/path/to/your/devices.json:/app/devices.json:ro'
```
