using WeatherWeb.Middleware;
using WeatherWeb.EndpointMappers;
using WeatherWeb.ServiceConfigurators;

using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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
builder.Services.AddWeatherServices();

var app = builder.Build();
app.UseRateLimiter();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapWeatherEndpoints();

app.Run();