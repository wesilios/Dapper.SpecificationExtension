using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class SqlServerDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithName("test-sqlserver-dapper")
            .WithPortBinding(1433, true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "YourStrong@Passw0rd")
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(1433))
            .Build();

        await Container.StartAsync();

        // Get the mapped port
        var port = Container.GetMappedPublicPort(1433);
        ConnectionString = $"Server=localhost,{port};Database=master;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Connection Timeout=30;";

        // Wait a bit for SQL Server to be fully ready
        await Task.Delay(5000);
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}

