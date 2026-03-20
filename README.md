# Shelly integration (MCP + REST API)

.NET 10 integration project that connects AI assistants and external systems to your Shelly Cloud devices.

[![Docker Image Size (MCP)](https://img.shields.io/docker/image-size/giogdev/shelly-cloud-mcp-server?label=MCP%20image)](https://hub.docker.com/r/giogdev/shelly-cloud-mcp-server)
[![Docker Pulls (MCP)](https://img.shields.io/docker/pulls/giogdev/shelly-cloud-mcp-server?label=MCP%20pulls)](https://hub.docker.com/r/giogdev/shelly-cloud-mcp-server)


[![Docker Image Size (API)](https://img.shields.io/docker/image-size/giogdev/shelly-api-gateway?label=API%20image)](https://hub.docker.com/r/giogdev/shelly-api-gateway)
[![Docker Pulls](https://img.shields.io/docker/pulls/giogdev/shelly-api-gateway?label=API%20pulls)](https://hub.docker.com/r/giogdev/shelly-api-gateway)


The project provides two independent components:
- **[MCP Server](docs/mcp-server.md)** — exposes Shelly devices as tools for AI assistants via the Model Context Protocol
- **[REST API Gateway](docs/rest-api.md)** — exposes Shelly devices over HTTP for dashboards, home automation platforms, and any REST-capable client

![alt text](./docs/images/n8n-example.png)

---

## Contest winner

> 🏆 **1st place** in the "Crafter" category at the **[Shelly Smart Home Challenge 2025](https://www.shelly.com/pages/shelly-smart-home-challenge-2025)** 🏆

---

## Usage example
- I'm using this project (api) as tool used by **AI agents**, with purpose to get historical/real time data from [Shelly weather station](https://www.shelly.com/it/products/ecowitt-ws90-7-in-1-weather-station)
- Control shelly device from AI Agents (like n8n or OpenClaw 🦞)
- Get information about weather history

---

## Documentation

- [MCP Server](docs/mcp-server.md) — configuration, Docker setup, Claude Desktop and Claude Code integration
- [REST API Gateway](docs/rest-api.md) — available endpoints, Docker setup, how to enable or disable the service

## Changelog
## v2.0.0
* New api project
* New methods to get historical and statistics data (API+MCP)
* New methods to get power consumption data (API+MCP)
* Bug & fix

## v1.0.0
* MCP server
* Light and device control (switch, get states)
