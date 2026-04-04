using FluentAssertions;
using Giogdev.Shelly.Integrations.Services;
using NSubstitute;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using Shelly.Models.Exceptions;
using System.Net;

namespace Shelly.Test.Api;

public class StatisticsEndpointsTests : IClassFixture<ShellyApiFactory>
{
    private readonly HttpClient _client;
    private readonly IShellyCloudService _service;

    public StatisticsEndpointsTests(ShellyApiFactory factory)
    {
        _client  = factory.CreateClient();
        _service = factory.CloudService;
    }

    // ─── GET /api/devices/{id}/weather-statistics ────────────────────────────

    [Fact]
    public async Task GetWeatherStatistics_ValidRequest_ReturnsOk()
    {
        _service
            .GetWeatherStationStatisticsAsync(Arg.Any<WeatherStationStatisticsRequest>())
            .Returns(new WeatherStationStatisticsResponse
            {
                Timezone = "UTC",
                Interval = "hour",
                History  = []
            });

        var response = await _client.GetAsync(
            "/api/devices/ws-1/weather-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetWeatherStatistics_DateFromAfterDateTo_Returns400()
    {
        var response = await _client.GetAsync(
            "/api/devices/ws-1/weather-statistics?dateFrom=2026-01-02T00:00:00Z&dateTo=2026-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWeatherStatistics_DeviceNotFound_Returns404()
    {
        _service
            .GetWeatherStationStatisticsAsync(Arg.Any<WeatherStationStatisticsRequest>())
            .Returns(Task.FromException<WeatherStationStatisticsResponse?>(
                new ShellyCloudApiException("device_not_found", "Device not found")));

        var response = await _client.GetAsync(
            "/api/devices/missing/weather-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWeatherStatistics_OtherShellyException_Returns400()
    {
        _service
            .GetWeatherStationStatisticsAsync(Arg.Any<WeatherStationStatisticsRequest>())
            .Returns(Task.FromException<WeatherStationStatisticsResponse?>(
                new ShellyCloudApiException("bad_request", "Invalid parameters")));

        var response = await _client.GetAsync(
            "/api/devices/ws-1/weather-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWeatherStatistics_HttpRequestException_Returns502()
    {
        _service
            .GetWeatherStationStatisticsAsync(Arg.Any<WeatherStationStatisticsRequest>())
            .Returns(Task.FromException<WeatherStationStatisticsResponse?>(
                new HttpRequestException("upstream unreachable")));

        var response = await _client.GetAsync(
            "/api/devices/ws-1/weather-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    // ─── GET /api/devices/{id}/power-statistics ───────────────────────────────

    [Fact]
    public async Task GetPowerStatistics_ValidRequest_ReturnsOk()
    {
        _service
            .GetPowerConsumptionStatisticsAsync(Arg.Any<PowerConsumptionStatisticsRequest>())
            .Returns(new PowerConsumptionStatisticsResponse
            {
                Timezone = "UTC",
                Interval = "hour",
                History  = []
            });

        var response = await _client.GetAsync(
            "/api/devices/pm-1/power-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPowerStatistics_DeviceNotFound_Returns404()
    {
        _service
            .GetPowerConsumptionStatisticsAsync(Arg.Any<PowerConsumptionStatisticsRequest>())
            .Returns(Task.FromException<PowerConsumptionStatisticsResponse?>(
                new ShellyCloudApiException("device_not_found", "Device not found")));

        var response = await _client.GetAsync(
            "/api/devices/missing/power-statistics?dateFrom=2026-01-01T00:00:00Z&dateTo=2026-01-02T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
