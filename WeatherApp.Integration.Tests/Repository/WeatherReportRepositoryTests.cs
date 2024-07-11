using System.Data;
using Npgsql;

namespace WeatherApp.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(nameof(TestCollectionFixture))]
public class WeatherReportRepositoryTests
{
    [Fact]
    public async void ValidateDataProperlyWrittenToDb()
    {
        // Arrange
        var forecast = new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now),
            20,
            "Warm"
        );

        var repository = new WeatherReportRepository(Connection);

        // Act
        var result = await repository.SaveForecastAsync(forecast);

        // Assert
        Assert.Equal(1, result);
    }

    private static IDbConnection Connection
    {
        get
        {
            var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
            connection.Open();
            return connection;
        }
    }
}