using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace TestEnvironment;

public class TestEnvironment : IAsyncDisposable
{
    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly DatabaseEnvironment _postgreSql;
    private bool _disposed;

    public TestEnvironment()
    {
        _postgreSql = new DatabaseEnvironment("weather_app", _network);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting test environment!");
        await _network.CreateAsync(cancellationToken);
        await _postgreSql.StartAsync(cancellationToken);

        // await _postgreSql.CreateDatabase("weather_app", cancellationToken);
        await _postgreSql.ExecuteSqlAsync("""
            CREATE TABLE weather_reports (
                id SERIAL PRIMARY KEY,
                date DATE NOT NULL,
                temperature_c INT NOT NULL,
                summary TEXT NOT NULL
            );
        """, ct: cancellationToken);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgreSql.ConnectionString);
        Console.WriteLine($"Internal connection string: {_postgreSql.InternalConnectionString}");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _postgreSql.StopAsync(cancellationToken);
        await _network.DeleteAsync(cancellationToken);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            await _postgreSql.DisposeAsync();
            await _network.DisposeAsync();
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("Disposing (async) test environment!");
        await DisposeAsync(true);
        Console.WriteLine("Test environment disposed!");
        GC.SuppressFinalize(this);
    }
}
