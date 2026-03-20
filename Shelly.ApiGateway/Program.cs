using Asg.MCP.Services;
using Scalar.AspNetCore;
using Shelly.ApiGateway.Endpoints;
using Shelly.Models;
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

}

// Expose the OpenAPI document only in development; use a dedicated tool in prod.
app.MapOpenApi();
// Scalar serves the interactive API reference UI at /scalar by default.
app.MapScalarApiReference();

// Global exception handler — catches anything that escapes endpoint-level try/catch.
// Returns { "message": "..." } with status 500. Never exposes a stack trace.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var message = exceptionFeature?.Error?.Message ?? "An unexpected error occurred.";

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(message));
    });
});


//app.UseHttpsRedirection();

// ──────────────────────────────────────────────
// Endpoint registration — each group in its own file under Endpoints/
// ──────────────────────────────────────────────
app.MapDeviceEndpoints();
app.MapSwitchEndpoints();
app.MapStatisticsEndpoints();

app.Run();
