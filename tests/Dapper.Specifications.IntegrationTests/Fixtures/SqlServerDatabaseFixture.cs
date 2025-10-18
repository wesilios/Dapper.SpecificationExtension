using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class SqlServerDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithName("test-sqlserver-dapper")
            .WithPortBinding(1433, 1433)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "YourStrong@Passw0rd")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(1433)
                .UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1'"))
            .Build();

        await Container.StartAsync();

        ConnectionString = "Server=localhost,1433;Database=master;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}

