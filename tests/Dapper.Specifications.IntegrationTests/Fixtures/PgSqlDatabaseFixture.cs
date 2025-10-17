using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class PgSqlDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithName("test-postgres-dapper")
            .WithPortBinding(5432, 5432)
            .WithEnvironment("POSTGRES_PASSWORD", "password")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(5432)
                .UntilCommandIsCompleted("pg_isready -U postgres"))
            .Build();

        await Container.StartAsync();

        ConnectionString = $"Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres";
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}