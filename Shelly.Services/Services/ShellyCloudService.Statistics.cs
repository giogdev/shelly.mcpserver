using Polly;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
using Shelly.Services.Exceptions;
using Shelly.Services.Utils;
using System.Text.Json;

namespace Asg.MCP.Services
{
    /// <summary>
    /// Partial class containing statistics operations.
    /// </summary>
    public partial class ShellyCloudService
    {
        // Shared deserialization options for statistics endpoints — case-insensitive to match
        // whatever casing the Shelly Cloud API returns for a given firmware version.
        private static readonly JsonSerializerOptions _statisticsJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Retrieves historical statistics for a Shelly weather station device over a custom date range.
        /// Data is aggregated by hour on the Shelly Cloud side.
        /// </summary>
        /// <param name="request">Device identifier and the date range to query.</param>
        /// <returns>The statistics response, or null if the body cannot be deserialized.</returns>
        /// <exception cref="HttpRequestException">Thrown when the API returns a non-success HTTP status with no parseable Shelly error key.</exception>
        /// <exception cref="ShellyCloudApiException">Thrown when the API returns a known business-level error (e.g. device_not_found).</exception>
        public async Task<WeatherStationStatisticsResponse?> GetWeatherStationStatisticsAsync(WeatherStationStatisticsRequest request)
        {
            var now = DateTime.UtcNow;
            var adjustedDateTo = request.DateTo;

            // If dateTo matches the current minute (ignoring seconds), subtract one minute
            // to avoid querying an incomplete time bucket.
            if (adjustedDateTo.Year == now.Year &&
                adjustedDateTo.Month == now.Month &&
                adjustedDateTo.Day == now.Day &&
                adjustedDateTo.Hour == now.Hour &&
                adjustedDateTo.Minute == now.Minute)
            {
                adjustedDateTo = adjustedDateTo.AddMinutes(-1);
            }

            // ISO 8601 format keeps the dates unambiguous across timezones
            var dateFrom = request.DateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var dateTo = adjustedDateTo.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var url = $"https://{_host}/v2/statistics/weather-station" +
                      $"?id={request.DeviceId}&channel=0&date_range=custom" +
                      $"&date_from={Uri.EscapeDataString(dateFrom)}&date_to={Uri.EscapeDataString(dateTo)}" +
                      $"&auth_key={_authKey}";

            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(
                async (context) => await _httpClient.GetAsync(url), new Context());

            // Read body once; both error inspection and deserialization consume it from the same string.
            var body = await response.Content.ReadAsStringAsync();

            // Non-success HTTP status: attempt to extract a Shelly error key before
            // falling back to a generic HttpRequestException.
            if (!response.IsSuccessStatusCode)
            {
                TryThrowShellyApiException(body);
                throw new HttpRequestException(
                    $"API call failed with status {response.StatusCode}: {body}",
                    null,
                    response.StatusCode);
            }

            // Guard against application-level rejection (HTTP 200 but isok: false).
            TryThrowShellyApiException(body);

            return JsonSerializer.Deserialize<WeatherStationStatisticsResponse>(body, _statisticsJsonOptions);
        }

        /// <summary>
        /// Retrieves historical power consumption statistics for a Shelly device over a custom date range.
        /// Data is aggregated by hour on the Shelly Cloud side.
        /// </summary>
        /// <param name="request">Device identifier and the date range to query.</param>
        /// <returns>The statistics response, or null if the body cannot be deserialized.</returns>
        /// <exception cref="HttpRequestException">Thrown when the API returns a non-success HTTP status with no parseable Shelly error key.</exception>
        /// <exception cref="ShellyCloudApiException">Thrown when the API returns a known business-level error (e.g. device_not_found).</exception>
        public async Task<PowerConsumptionStatisticsResponse?> GetPowerConsumptionStatisticsAsync(PowerConsumptionStatisticsRequest request)
        {
            // ISO 8601 format keeps the dates unambiguous across timezones
            var dateFrom = request.DateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var dateTo = request.DateTo.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var url = $"https://{_host}/v2/statistics/power-consumption" +
                      $"?id={request.DeviceId}&channel=0&date_range=custom" +
                      $"&date_from={Uri.EscapeDataString(dateFrom)}&date_to={Uri.EscapeDataString(dateTo)}" +
                      $"&auth_key={_authKey}";

            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(
                async (context) => await _httpClient.GetAsync(url), new Context());

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TryThrowShellyApiException(body);
                throw new HttpRequestException(
                    $"API call failed with status {response.StatusCode}: {body}",
                    null,
                    response.StatusCode);
            }

            TryThrowShellyApiException(body);

            return JsonSerializer.Deserialize<PowerConsumptionStatisticsResponse>(body, _statisticsJsonOptions);
        }

        /// <summary>
        /// Inspects the raw JSON body for the Shelly application-level error envelope.
        /// Throws <see cref="ShellyCloudApiException"/> when isok is false, surfacing the
        /// first error key so the presentation layer can map it to the right HTTP status.
        /// No-ops on bodies that do not contain the error envelope.
        /// </summary>
        private static void TryThrowShellyApiException(string body)
        {
            // Quick check avoids deserializing on the happy path.
            if (!body.Contains("\"isok\"", StringComparison.OrdinalIgnoreCase))
                return;

            var envelope = JsonSerializer.Deserialize<ShellyApiErrorResponse>(body, _statisticsJsonOptions);
            if (envelope is not { IsOk: false })
                return;

            // Use the first error entry as the canonical key + message.
            var (key, message) = envelope.Errors?.FirstOrDefault()
                ?? new KeyValuePair<string, string>("unknown_error", "Shelly API rejected the request.");

            throw new ShellyCloudApiException(key, message);
        }
    }
}
