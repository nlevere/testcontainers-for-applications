using Dapper;
using System.Data;

namespace WeatherApp;

public class WeatherReportRepository(IDbConnection connection) : IWeatherReportRepository //, IDisposable
{
    // private bool _disposed;

    public Task<int> SaveForecastAsync(WeatherForecast forecast, CancellationToken ct = default)
    {
        var sql = @"
            INSERT INTO weather_reports (date, temperature_c, summary)
            VALUES (@Date, @TemperatureC, @Summary);
        ";
        var param = new { forecast.Date, forecast.TemperatureC, forecast.Summary };
        CommandDefinition command = new(sql, param, cancellationToken: ct);
        lock(this)
        {
            var results = connection.Execute(command);
            return Task.FromResult(results);
        }
    }

    // public void Dispose()
    // {
    //     Dispose(true);
    //     GC.SuppressFinalize(this);
    // }

    // protected virtual void Dispose(bool disposing)
    // {
    //     if (_disposed)
    //     {
    //         return;
    //     }

    //     if (disposing)
    //     {
    //         _connection.Close();
    //     }

    //     _disposed = true;
    // }
}
