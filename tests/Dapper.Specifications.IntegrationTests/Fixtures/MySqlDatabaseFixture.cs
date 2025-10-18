using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class MySqlDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("mysql:8.0")
            .WithName("test-mysql-dapper")
            .WithPortBinding(3306, true)
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "password")
            .WithEnvironment("MYSQL_DATABASE", "testdb")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(3306))
            .Build();

        await Container.StartAsync();

        // Get the mapped port
        var port = Container.GetMappedPublicPort(3306);
        ConnectionString = $"Server=localhost;Port={port};Database=testdb;Uid=root;Pwd=password;";

        // Wait a bit for MySQL to be fully ready
        await Task.Delay(3000);
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}

