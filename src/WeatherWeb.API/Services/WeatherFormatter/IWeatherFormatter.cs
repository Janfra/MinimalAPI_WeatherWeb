namespace WeatherWeb.Services.WeatherFormatter;

using WeatherWeb.Models;
   
public interface IWeatherFormatter
{
    public string Format(WeatherReport report);
    public string GetTemperatureFormat(WeatherReport report);
}

