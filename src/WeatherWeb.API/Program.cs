using FluentValidation;
using WeatherWeb.Services.Formatter;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Mappers;
using WeatherWeb.Models;
using WeatherWeb.Middleware;
using WeatherWeb.Validators;
using WeatherWeb.Data;

using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// Use environment variable or fallback to hardcoded path
var connectionString = builder.Configuration.GetConnectionString("WeatherDatabase") ?? WeatherDbContext.GetDbPath();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IWeatherFormatter, StandardWeatherFormatter>();
builder.Services.AddScoped<IWeatherReporter, WeatherReporter>();
builder.Services.AddDbContext<WeatherDbContext>();
builder.Services.AddScoped<IWeatherDbContext>(
    serviceProvider => serviceProvider.GetRequiredService<WeatherDbContext>()
);
builder.Services.AddScoped<IValidator<WeatherReportDTO>, WeatherReportDTOValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(
    options =>
    {
        // Adding rate limiting utilising the Fixed Window limits to fit duration and access limits of any utilised third party API. However, resetting the server will reset the windows, making possible for access limits to potentially be exceeded and reset times to misaligned. May add to the DB an entry to track this information to protect against resets.
        TimeSpan rateWindow = TimeSpan.FromDays(1);
        const int totalRequestLimit = 100;
        const int individualUserLimit = 10;

        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
            // Total server capacity (shared)
            PartitionedRateLimiter.Create<HttpContext, string>
            (
                context => RateLimitPartition.GetFixedWindowLimiter
                (
                    partitionKey: "TotalServerLimit",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = totalRequestLimit,
                        QueueLimit = 0,
                        Window = rateWindow
                    }
                )
            ),

            // Per user
            PartitionedRateLimiter.Create<HttpContext, string>
            (
                context => RateLimitPartition.GetFixedWindowLimiter
                (
                    partitionKey: 
                        context.User.Identity?.Name ?? 
                        context.Connection.RemoteIpAddress?.ToString() ??
                        "anonymous-fallback",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = individualUserLimit,
                        QueueLimit = 0,
                        Window = rateWindow
                    }
                )
            )
        );

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    }
);

var app = builder.Build();
app.UseRateLimiter();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Register of application mappers, each mapper will add their CRUD actions.
IMapper[] mappers = [
    new WeatherMapper()
    ];

foreach (var mapper in mappers)
{
    mapper.Map(app);
}

app.Run();