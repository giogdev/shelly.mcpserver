using Giogdev.Shelly.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shelly.Models;
using Shelly.Models.Cloud;
using Shelly.Models.Cloud.Request;

namespace Shelly.ApiGateway.Endpoints;

/// <summary>
/// Endpoints for controlling switch-capable Shelly devices.
/// </summary>
public static class SwitchEndpoints
{
    public static IEndpointRouteBuilder MapSwitchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Switch");

        // Sends an ON/OFF command to the device identified by device ID.
        // Optional delaySeconds causes the device to revert after that many seconds.
        group.MapPost("/{deviceId}/switch", async Task<Results<Ok<DefaultResponse>, NotFound<ApiErrorResponse>, ProblemHttpResult>> (
            string deviceId,
            SwitchRequest body,
            IShellyCloudService shellyService) =>
        {
            DeviceNameMappingStoreItem? device = shellyService.GetKnownDevices()
                .FirstOrDefault(d => d.DeviceId == deviceId);
            if (device is null)
                return TypedResults.NotFound(new ApiErrorResponse($"No device found with id '{deviceId}'."));

            try
            {
                var switchRequest = new CloudDeviceSwitchRequest
                {
                    Id = device.DeviceId,
                    Channel = device.ChannelId,
                    On = body.On,
                    // A null or zero delay is ignored by the service layer.
                    ToggleAfter = body.DelaySeconds > 0 ? body.DelaySeconds : null
                };

                await shellyService.ControlSwitchDevice(switchRequest);

                return TypedResults.Ok(new DefaultResponse { IsSuccess = true });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SwitchDevice")
        .WithSummary("Turn a Shelly switch device on or off");

        return app;
    }
}

/// <summary>
/// Request body for the switch endpoint.
/// </summary>
/// <param name="On">Target power state: true = ON, false = OFF.</param>
/// <param name="DelaySeconds">If provided and greater than zero, the device reverts after this many seconds.</param>
public record SwitchRequest(bool On, int? DelaySeconds);
