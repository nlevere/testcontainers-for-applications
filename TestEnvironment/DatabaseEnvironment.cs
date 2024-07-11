using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TestEnvironment;

public class DatabaseEnvironment(string databaseName, INetwork network) : IAsyncDisposable
{
    private const string ContainerName = "postgres";
    private bool _disposed;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase(databaseName)
            .WithNetwork(network)
            .WithNetworkAliases(ContainerName)
            .Build();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting database environment!");
        return _postgreSqlContainer.StartAsync(cancellationToken);
    }

    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _postgreSqlContainer.StopAsync(cancellationToken);
    }

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public string InternalConnectionString
    {
        get
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_postgreSqlContainer.GetConnectionString())
            {
                Host = ContainerName,
                Port = 5432
            };
            return connectionStringBuilder.ConnectionString;
        }
    }

    public Task<ExecResult> CreateDatabase(string dbName, CancellationToken ct = default)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_postgreSqlContainer.GetConnectionString());
        if (connectionStringBuilder is null || connectionStringBuilder.Database is null || connectionStringBuilder.Username is null)
        {
            throw new InvalidOperationException("Bad connection string.");
        }
        return ExecuteSqlAsync("CREATE DATABASE {0} WITH OWNER = '{1}';", [ dbName, connectionStringBuilder.Username ], ct);
    }

    public Task<ExecResult> ExecuteSqlAsync(string template, object?[]? arguments = default, CancellationToken ct = default)
    {
        arguments ??= [];
        var sql = FormattableStringFactory.Create(template, arguments).ToString();
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_postgreSqlContainer.GetConnectionString());
        if (connectionStringBuilder is null || connectionStringBuilder.Database is null || connectionStringBuilder.Username is null)
        {
            throw new InvalidOperationException("Bad connection string.");
        }
        return _postgreSqlContainer.ExecAsync(["psql", "--username", connectionStringBuilder.Username, "--dbname", connectionStringBuilder.Database, "--command", sql], ct);
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("Disposing (async) database environment!");
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
        Console.WriteLine("database environment disposed!");
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        _disposed = true;
    }
}
