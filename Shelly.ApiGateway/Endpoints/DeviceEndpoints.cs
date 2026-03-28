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
        group.MapGet("/", Results<Ok<IEnumerable<DeviceNameMappingStoreItem>>, ProblemHttpResult> (IShellyCloudService shellyService) =>
        {
            try
            {
                IEnumerable<DeviceNameMappingStoreItem> devices = shellyService.GeKnownDevices();
                return TypedResults.Ok(devices);
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetDevices")
        .WithSummary("Get all known Shelly devices");

        // Looks up the device by ID, then fetches its live status from Shelly Cloud.
        group.MapGet("/{deviceId}/status", async Task<Results<Ok<GenericDeviceStatusModel>, NotFound<ApiErrorResponse>, ProblemHttpResult>> (
            string deviceId,
            IShellyCloudService shellyService) =>
        {
            DeviceNameMappingStoreItem? device = shellyService.GeKnownDevices()
                .FirstOrDefault(d => d.DeviceId == deviceId);
            if (device is null)
                return TypedResults.NotFound(new ApiErrorResponse($"No device found with id '{deviceId}'."));

            try
            {
                GenericDeviceStatusModel? status = await shellyService.GetSingleDeviceStateAsync(device);

                // Service returns null only when the upstream response is empty.
                if (status is null)
                    return TypedResults.Problem(
                        detail: "Shelly Cloud returned no data for the requested device.",
                        statusCode: StatusCodes.Status502BadGateway);

                return TypedResults.Ok(status);
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetDeviceStatus")
        .WithSummary("Get live status of a Shelly device by device ID");

        return app;
    }
}
