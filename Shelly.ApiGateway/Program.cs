using Asg.MCP.Services;
using Shelly.ApiGateway.Endpoints;
using Shelly.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// Dependency Injection
// ──────────────────────────────────────────────

// Named client mirrors the registration used in Shelly.McpServer.
builder.Services.AddHttpClient<ShellyCloudService>("ShellyCloudClient");

// Store is a Singleton because it reads the device mapping file once at startup.
builder.Services.AddSingleton<ShellyCloudDeviceStore>();
builder.Services.AddSingleton<IShellyCloudService, ShellyCloudService>();

// ──────────────────────────────────────────────
// OpenAPI
// ──────────────────────────────────────────────
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Expose the OpenAPI document only in development; use a dedicated tool in prod.
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ──────────────────────────────────────────────
// Endpoint registration — each group in its own file under Endpoints/
// ──────────────────────────────────────────────
app.MapDeviceEndpoints();
app.MapSwitchEndpoints();
app.MapStatisticsEndpoints();

app.Run();
