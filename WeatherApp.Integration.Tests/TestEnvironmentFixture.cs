
using Dapper;

namespace WeatherApp.Integration.Tests;

public class TestEnvironmentFixture : IAsyncLifetime
{
    private readonly TestEnvironment.TestEnvironment _environment = new();

    public Task InitializeAsync()
    {
        SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        return _environment.StartAsync(cts.Token);
    }

    public async Task DisposeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _environment.StopAsync(cts.Token);
        await _environment.DisposeAsync();
    }
}
