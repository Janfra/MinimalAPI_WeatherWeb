namespace WeatherWeb.Data;

using Microsoft.EntityFrameworkCore;
using WeatherWeb.Models;

public class WeatherDbContext : DbContext, IWeatherDbContext
{
    public DbSet<WeatherReport> WeatherReports { get; set; }

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) 
        : base(options) { }

    void IWeatherDbContext.Add(object entity)
    {
        Add(entity);
    }

    TEntity IWeatherDbContext.Add<TEntity>(TEntity entity)
    {
        return Add(entity).Entity;
    }

    void IWeatherDbContext.Remove(object entity)
    {
        Remove(entity);
    }

    TEntity IWeatherDbContext.Remove<TEntity>(TEntity entity)
    {
        return Remove(entity).Entity;
    }
}
