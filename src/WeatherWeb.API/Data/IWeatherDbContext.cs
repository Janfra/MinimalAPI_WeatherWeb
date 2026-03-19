namespace WeatherWeb.Data;

using Microsoft.EntityFrameworkCore;
using WeatherWeb.Models;

public interface IWeatherDbContext
{
    public DbSet<WeatherReport> WeatherReports { get; }
    public void Add(object entity);
    public TEntity Add<TEntity>(TEntity entity) where TEntity : class;
    public void Remove(object entity);
    public TEntity Remove<TEntity>(TEntity entity) where TEntity : class;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
