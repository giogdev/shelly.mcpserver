using Giogdev.Shelly.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shelly.Models;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;

namespace Shelly.ApiGateway.Endpoints;

/// <summary>
/// Endpoints for historical statistics: weather station and power consumption.
/// </summary>
public static class StatisticsEndpoints
{
    public static IEndpointRouteBuilder MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Statistics");
        // Fetches temperature/humidity history for a weather station device.
        // dateFrom and dateTo are passed as UTC query parameters.
        group.MapGet("/{deviceId}/weather-statistics",
            async Task<Results<Ok<WeatherStationStatisticsResponse>, BadRequest<ApiErrorResponse>, ProblemHttpResult>> (
                string deviceId,
                DateTime dateFrom,
                DateTime dateTo,
                IShellyCloudService shellyService) =>
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return TypedResults.BadRequest(new ApiErrorResponse("The 'deviceId' parameter cannot be empty."));

            if (dateFrom >= dateTo)
                return TypedResults.BadRequest(new ApiErrorResponse("'dateFrom' must be earlier than 'dateTo'."));

            var request = new WeatherStationStatisticsRequest
            {
                DeviceId = deviceId,
                DateFrom = dateFrom,
                DateTo   = dateTo
            };

            WeatherStationStatisticsResponse? result = await shellyService.GetWeatherStationStatisticsAsync(request);

            if (result is null)
                return TypedResults.Problem(
                    detail: "Shelly Cloud returned no weather statistics for the requested range.",
                    statusCode: StatusCodes.Status502BadGateway);

            return TypedResults.Ok(result);
        })
        .WithName("GetWeatherStatistics")
        .WithSummary("Get weather station historical statistics for a date range");

        group.MapGet("/{deviceId}/power-statistics",
            async Task<Results<Ok<PowerConsumptionStatisticsResponse>, BadRequest<ApiErrorResponse>, ProblemHttpResult>> (
                string deviceId,
                DateTime dateFrom,
                DateTime dateTo,
                IShellyCloudService shellyService) =>
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return TypedResults.BadRequest(new ApiErrorResponse("The 'deviceId' parameter cannot be empty."));

            if (dateFrom >= dateTo)
                return TypedResults.BadRequest(new ApiErrorResponse("'dateFrom' must be earlier than 'dateTo'."));

            var request = new PowerConsumptionStatisticsRequest
            {
                DeviceId = deviceId,
                DateFrom = dateFrom,
                DateTo   = dateTo
            };

            PowerConsumptionStatisticsResponse? result = await shellyService.GetPowerConsumptionStatisticsAsync(request);

            if (result is null)
                return TypedResults.Problem(
                    detail: "Shelly Cloud returned no power statistics for the requested range.",
                    statusCode: StatusCodes.Status502BadGateway);

            return TypedResults.Ok(result);
        })
        .WithName("GetPowerStatistics")
        .WithSummary("Get power consumption historical statistics for a date range");

        return app;
    }
}
