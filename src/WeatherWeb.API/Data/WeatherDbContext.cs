namespace WeatherWeb.Data;

using Microsoft.EntityFrameworkCore;
using WeatherWeb.Models;

public class WeatherDbContext : DbContext, IWeatherDbContext
{
    public DbSet<WeatherReport> WeatherReports { get; set; }

    // For simplicity storing the connection string in code, in real applications use configuration files or environment variables.
    public string DbPath { get; }

    public WeatherDbContext()
    {
        DbPath = GetDbPath();
    }

    public static string GetDbPath()
    {
        return "weatherapp.db";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

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
