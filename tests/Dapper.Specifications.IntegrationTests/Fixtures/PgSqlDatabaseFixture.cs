using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class PgSqlDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithName("test-postgres-dapper")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_PASSWORD", "password")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(5432))
            .Build();

        await Container.StartAsync();

        // Get the mapped port
        var port = Container.GetMappedPublicPort(5432);
        ConnectionString = $"Host=localhost;Port={port};Username=postgres;Password=password;Database=postgres;";

        // Wait a bit for PostgreSQL to be fully ready
        await Task.Delay(3000);
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}