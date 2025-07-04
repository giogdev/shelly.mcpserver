using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Services.Utils
{
    public static class HttpPolicies
    {
        public static readonly AsyncRetryPolicy<HttpResponseMessage> standardRetryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r =>
          r.StatusCode != HttpStatusCode.OK
          )
        .WaitAndRetryAsync(2,
            //Wait 1 second because I can perform only 1 request per seconds to Shelly Cloud
            retryAttempt => TimeSpan.FromSeconds(1),
            async (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine("*************************************");
                  Console.WriteLine($"Request failed, retry {retryCount} after {timespan.TotalSeconds} seconds");

                  if (outcome.Exception != null)
                  {
                      Console.WriteLine($"Exception: {outcome.Exception.Message}");
                  }
                  else if (outcome.Result != null)
                  {
                      Console.WriteLine($"HTTP Status: {outcome.Result.StatusCode}");
                      Console.WriteLine($"Reason: {outcome.Result.ReasonPhrase}");
                    Console.WriteLine($"Reason: {await outcome.Result.Content.ReadAsStringAsync()}");
                }

                  Console.WriteLine("*************************************");
              });

    }
}
