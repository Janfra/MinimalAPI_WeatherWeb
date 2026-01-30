using FluentValidation;
using WeatherWeb.Services.Formatter;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Mappers;
using WeatherWeb.Models;
using WeatherWeb.Middleware;
using WeatherWeb.Validators;
using WeatherWeb.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("WeatherDatabase") ?? WeatherDbContext.GetDbPath();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IWeatherFormatter, StandardWeatherFormatter>();
builder.Services.AddScoped<IWeatherReporter, WeatherReporter>();
builder.Services.AddDbContext<WeatherDbContext>();
builder.Services.AddScoped<IValidator<WeatherReportDTO>, WeatherReportDTOValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Register of application mappers
IMapper[] mappers = [
    new WeatherMapper()
    ];

foreach (var mapper in mappers)
{
    mapper.Map(app);
}

app.Run();