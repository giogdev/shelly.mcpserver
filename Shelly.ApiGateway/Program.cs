using Giogdev.Shelly.Integrations.Services;
using Scalar.AspNetCore;
using Shelly.ApiGateway.Endpoints;
using Shelly.ApiGateway.Middleware;
using Shelly.Services;
using Shelly.Services.Mapper;
using Shelly.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// Dependency Injection
// ──────────────────────────────────────────────

builder.Services.AddHttpClient<ShellyCloudService>(ShellyServiceConstants.HttpClientName);

// Store is a Singleton because it reads the device mapping file once at startup.
builder.Services.AddSingleton<ShellyCloudDeviceStore>();
builder.Services.AddSingleton<IShellyCloudMapper, ShellyCloudMapper>();
builder.Services.AddSingleton<IShellyCloudService, ShellyCloudService>();

// ──────────────────────────────────────────────
// Exception handling
// ──────────────────────────────────────────────

// Exceptions managed in this middleware
builder.Services.AddExceptionHandler<ShellyExceptionHandler>();
builder.Services.AddProblemDetails();

// ──────────────────────────────────────────────
// Health checks
// ──────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ──────────────────────────────────────────────
// OpenAPI
// ──────────────────────────────────────────────
builder.Services.AddOpenApi();

var app = builder.Build();

// Populate device store from Shelly Cloud at startup.
var shellyService = app.Services.GetRequiredService<IShellyCloudService>();
await shellyService.FetchAndPopulateDevicesAsync();

// Global exception handler — routes to ShellyExceptionHandler registered above.
app.UseExceptionHandler();

// Expose the OpenAPI document and Scalar UI.
app.MapOpenApi();
app.MapScalarApiReference();

// ──────────────────────────────────────────────
// Endpoint registration
// ──────────────────────────────────────────────
app.MapHealthChecks("/health");
app.MapDeviceEndpoints();
app.MapSwitchEndpoints();
app.MapStatisticsEndpoints();

app.Run();
