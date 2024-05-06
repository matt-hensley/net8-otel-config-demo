using System.Reflection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: Assembly.GetExecutingAssembly()?.GetName().Name ?? "unknown-service",
        serviceNamespace: builder.Environment.IsProduction() ? "production" : "development",
        serviceVersion: Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "0.0.0",
        autoGenerateServiceInstanceId: true);

builder.Services.AddOpenTelemetry().UseOtlpExporter()
    .WithMetrics(builder =>
    {
        builder.SetResourceBuilder(resourceBuilder);
        builder.AddAspNetCoreInstrumentation();
        //builder.AddConsoleExporter();
        builder.AddProcessInstrumentation();
        builder.AddRuntimeInstrumentation();
    })
    .WithTracing(builder =>
    {
        builder.SetResourceBuilder(resourceBuilder);
        builder.AddAspNetCoreInstrumentation();
        //builder.AddConsoleExporter();
    });
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    //logging.AddConsoleExporter();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
