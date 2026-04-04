using Giogdev.Shelly.Integrations.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Shelly.Test.Api;

/// <summary>
/// WebApplicationFactory that replaces IShellyCloudService with a substitute
/// so integration tests never hit the real Shelly Cloud API.
/// </summary>
public class ShellyApiFactory : WebApplicationFactory<Program>
{
    public IShellyCloudService CloudService { get; } = Substitute.For<IShellyCloudService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            // Provide dummy values so ShellyCloudService constructor does not throw.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SHELLY_API_ENDPOINT"] = "shelly-test.example.com",
                ["SHELLY_API_KEY"]      = "test-key"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the real service with the substitute.
            services.RemoveAll<IShellyCloudService>();
            services.AddSingleton(CloudService);
        });
    }
}
