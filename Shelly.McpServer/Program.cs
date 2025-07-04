
using Asg.MCP.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shelly.Models.Cloud;
using Shelly.Services.Services;

var builder = Host.CreateApplicationBuilder(args);

#region init appsettings

//Initialize arguments
builder.Configuration
    .AddCommandLine(args);

//get "--settingPath"
var settingsPath = builder.Configuration["settingsPath"];

if (!string.IsNullOrEmpty(settingsPath) && File.Exists(settingsPath))
{
    builder.Configuration
        .AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>(); ;
}
else
{
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>(); ;
}

#endregion

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

#region Dependency Injection

builder.Services.AddHttpClient<ShellyCloudService>("PhoneProviderClient");
builder.Services.AddSingleton<ShellyCloudDeviceStore>();
builder.Services.AddSingleton<IShellyCloudService, ShellyCloudService>();


builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

#endregion

var app = builder.Build();

//to run inspector
//npx @modelcontextprotocol/inspector dotnet run


await app.RunAsync();
