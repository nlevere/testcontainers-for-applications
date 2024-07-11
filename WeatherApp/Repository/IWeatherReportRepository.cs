namespace WeatherApp;

public interface IWeatherReportRepository
{
    Task<int> SaveForecastAsync(WeatherForecast forecast, CancellationToken ct = default);
}
