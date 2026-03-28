using Giogdev.Shelly.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shelly.Models;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using Shelly.Services.Exceptions;

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
            async Task<Results<Ok<WeatherStationStatisticsResponse>, NotFound<ApiErrorResponse>, BadRequest<ApiErrorResponse>, ProblemHttpResult>> (
                string deviceId,
                DateTime dateFrom,
                DateTime dateTo,
                IShellyCloudService shellyService) =>
        {
            try
            {
                var request = new WeatherStationStatisticsRequest
                {
                    DeviceId = deviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                WeatherStationStatisticsResponse? result = await shellyService.GetWeatherStationStatisticsAsync(request);

                // A null result means the cloud API returned nothing for this range.
                if (result is null)
                    return TypedResults.Problem(
                        detail: "Shelly Cloud returned no weather statistics for the requested range.",
                        statusCode: StatusCodes.Status502BadGateway);

                return TypedResults.Ok(result);
            }
            catch (ShellyCloudApiException ex) when (ex.ErrorKey == "device_not_found")
            {
                // The device ID is not recognised by the cloud as a weather station
                // (or has never been connected). Surface as 404 with a human-readable message.
                return TypedResults.NotFound(new ApiErrorResponse(ex.Message));
            }
            catch (ShellyCloudApiException ex)
            {
                // Other application-level rejections from Shelly (bad request semantics).
                return TypedResults.BadRequest(new ApiErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetWeatherStatistics")
        .WithSummary("Get weather station historical statistics for a date range");

        // Fetches energy consumption history for a power-metering device.
        group.MapGet("/{deviceId}/power-statistics",
            async Task<Results<Ok<PowerConsumptionStatisticsResponse>, NotFound<ApiErrorResponse>, BadRequest<ApiErrorResponse>, ProblemHttpResult>> (
                string deviceId,
                DateTime dateFrom,
                DateTime dateTo,
                IShellyCloudService shellyService) =>
        {
            try
            {
                var request = new PowerConsumptionStatisticsRequest
                {
                    DeviceId = deviceId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                PowerConsumptionStatisticsResponse? result = await shellyService.GetPowerConsumptionStatisticsAsync(request);

                if (result is null)
                    return TypedResults.Problem(
                        detail: "Shelly Cloud returned no power statistics for the requested range.",
                        statusCode: StatusCodes.Status502BadGateway);

                return TypedResults.Ok(result);
            }
            catch (ShellyCloudApiException ex) when (ex.ErrorKey == "device_not_found")
            {
                return TypedResults.NotFound(new ApiErrorResponse(ex.Message));
            }
            catch (ShellyCloudApiException ex)
            {
                return TypedResults.BadRequest(new ApiErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetPowerStatistics")
        .WithSummary("Get power consumption historical statistics for a date range");

        return app;
    }
}
