using Giogdev.Shelly.Integrations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shelly.Services.Services
{
    /// <summary>
    /// Periodically fetches the device list from Shelly Cloud and updates the local store,
    /// keeping the IsOnline flag current for all known devices.
    /// </summary>
    public class DeviceRefreshBackgroundService : BackgroundService
    {
        private const int DefaultIntervalSeconds = 30;

        private readonly IShellyCloudService _shellyCloudService;
        private readonly ILogger<DeviceRefreshBackgroundService> _logger;
        private readonly TimeSpan _interval;

        public DeviceRefreshBackgroundService(
            IShellyCloudService shellyCloudService,
            IConfiguration configuration,
            ILogger<DeviceRefreshBackgroundService> logger)
        {
            _shellyCloudService = shellyCloudService;
            _logger = logger;

            int intervalSeconds = configuration.GetValue<int?>("DeviceRefreshIntervalSeconds") ?? DefaultIntervalSeconds;
            _interval = TimeSpan.FromSeconds(intervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "DeviceRefreshBackgroundService started. Refresh interval: {Interval}s.",
                _interval.TotalSeconds);

            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await _shellyCloudService.FetchAndPopulateDevicesAsync();
                }
                catch (Exception ex)
                {
                    // Log and continue — the store retains the last known state.
                    // Clearing the store on transient failures would cause unnecessary 503s.
                    _logger.LogWarning(ex,
                        "DeviceRefreshBackgroundService: failed to refresh device list. Retrying in {Interval}s.",
                        _interval.TotalSeconds);
                }
            }
        }
    }
}
