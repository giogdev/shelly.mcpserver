using Giogdev.Shelly.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shelly.Models;
using Shelly.Models.Cloud;

namespace Shelly.ApiGateway.Endpoints;

/// <summary>
/// Endpoints for querying device list and per-device status.
/// </summary>
public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices");

        // Returns the full list of known devices from the local store (no cloud call).
        group.MapGet("/", (IShellyCloudService shellyService) =>
        {
            IEnumerable<DeviceNameMappingStoreItem> devices = shellyService.GetKnownDevices();
            return TypedResults.Ok(devices);
        })
        .WithName("GetDevices")
        .WithSummary("Get all known Shelly devices");

        // Looks up the device by ID, then fetches its live status from Shelly Cloud.
        group.MapGet("/{deviceId}/status", async Task<Results<Ok<GenericDeviceStatusModel>, NotFound<ApiErrorResponse>, ProblemHttpResult>> (
            string deviceId,
            IShellyCloudService shellyService) =>
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return TypedResults.Problem(
                    detail: "The 'deviceId' parameter cannot be empty.",
                    statusCode: StatusCodes.Status400BadRequest);

            DeviceNameMappingStoreItem? device = shellyService.GetKnownDevices()
                .FirstOrDefault(d => d.DeviceId == deviceId);

            if (device is null)
                return TypedResults.NotFound(new ApiErrorResponse($"No device found with id '{deviceId}'."));

            if (!device.IsOnline)
                return TypedResults.Problem(
                    detail: $"Device '{deviceId}' is currently offline.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);

            GenericDeviceStatusModel? status = await shellyService.GetSingleDeviceStateAsync(device);

            // Service returns null only when the upstream response is empty.
            if (status is null)
                return TypedResults.Problem(
                    detail: "Shelly Cloud returned no data for the requested device.",
                    statusCode: StatusCodes.Status502BadGateway);

            return TypedResults.Ok(status);
        })
        .WithName("GetDeviceStatus")
        .WithSummary("Get live status of a Shelly device by device ID");

        // Returns true if the device is currently online, false if offline. Reads from the local store (no cloud call).
        group.MapGet("/{deviceId}/is-online", Results<Ok<bool>, NotFound<ApiErrorResponse>> (
            string deviceId,
            IShellyCloudService shellyService) =>
        {
            DeviceNameMappingStoreItem? device = shellyService.GetKnownDevices()
                .FirstOrDefault(d => d.DeviceId == deviceId);

            if (device is null)
                return TypedResults.NotFound(new ApiErrorResponse($"No device found with id '{deviceId}'."));

            return TypedResults.Ok(device.IsOnline);
        })
        .WithName("GetDeviceIsOnline")
        .WithSummary("Returns true if the device is currently online, false if offline");

        return app;
    }
}
