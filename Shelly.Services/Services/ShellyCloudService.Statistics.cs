using Polly;
using Shelly.Models.Cloud.Request;
using Shelly.Models.Cloud.Response;
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
        /// <exception cref="HttpRequestException">Thrown when the API returns a non-success HTTP status.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the API returns isok: false.</exception>
        public async Task<WeatherStationStatisticsResponse?> GetWeatherStationStatisticsAsync(WeatherStationStatisticsRequest request)
        {
            // ISO 8601 format keeps the dates unambiguous across timezones
            var dateFrom = request.DateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var dateTo = request.DateTo.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var url = $"https://{_host}/v2/statistics/weather-station" +
                      $"?id={request.DeviceId}&channel=0&date_range=custom" +
                      $"&date_from={Uri.EscapeDataString(dateFrom)}&date_to={Uri.EscapeDataString(dateTo)}" +
                      $"&auth_key={_authKey}";

            using var response = await HttpPolicies.standardRetryPolicy.ExecuteAsync(
                async (context) => await _httpClient.GetAsync(url), new Context());

            // Read body once; both error inspection and deserialization consume it from the same string.
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"API call failed with status {response.StatusCode}: {body}",
                    null,
                    response.StatusCode);

            // Guard against application-level rejection (HTTP 200 but isok: false).
            ThrowIfApiError(body);

            return JsonSerializer.Deserialize<WeatherStationStatisticsResponse>(body, _statisticsJsonOptions);
        }

        /// <summary>
        /// Retrieves historical power consumption statistics for a Shelly device over a custom date range.
        /// Data is aggregated by hour on the Shelly Cloud side.
        /// </summary>
        /// <param name="request">Device identifier and the date range to query.</param>
        /// <returns>The statistics response, or null if the body cannot be deserialized.</returns>
        /// <exception cref="HttpRequestException">Thrown when the API returns a non-success HTTP status.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the API returns isok: false.</exception>
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
                throw new HttpRequestException(
                    $"API call failed with status {response.StatusCode}: {body}",
                    null,
                    response.StatusCode);

            ThrowIfApiError(body);

            return JsonSerializer.Deserialize<PowerConsumptionStatisticsResponse>(body, _statisticsJsonOptions);
        }

        /// <summary>
        /// Inspects the raw JSON body for the Shelly application-level error envelope.
        /// Throws when the API signals isok: false even under a 2xx HTTP status.
        /// </summary>
        private static void ThrowIfApiError(string body)
        {
            // A quick check avoids deserializing the full body on the happy path.
            if (!body.Contains("\"isok\"", StringComparison.OrdinalIgnoreCase))
                return;

            var errorEnvelope = JsonSerializer.Deserialize<ShellyApiErrorResponse>(body, _statisticsJsonOptions);
            if (errorEnvelope is { IsOk: false })
            {
                var errors = errorEnvelope.Errors is not null
                    ? JsonSerializer.Serialize(errorEnvelope.Errors)
                    : "(no error details)";

                throw new InvalidOperationException($"Shelly API returned isok: false. Errors: {errors}");
            }
        }
    }
}
