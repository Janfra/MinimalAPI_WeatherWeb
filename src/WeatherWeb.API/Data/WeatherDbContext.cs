namespace WeatherWeb.Data;

using WeatherWeb.Models;
using Microsoft.EntityFrameworkCore;

public class WeatherDbContext : DbContext
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
}
