using DotNet.Testcontainers.Builders;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class MySqlDatabaseFixture : DatabaseFixture
{
    public override async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("mysql:8.0")
            .WithName("test-mysql-dapper")
            .WithPortBinding(3306, 3306)
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "password")
            .WithEnvironment("MYSQL_DATABASE", "testdb")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(3306)
                .UntilCommandIsCompleted("mysqladmin ping -h localhost -u root -ppassword"))
            .Build();

        await Container.StartAsync();

        ConnectionString = "Server=localhost;Port=3306;Database=testdb;Uid=root;Pwd=password;";
    }

    public override async Task DisposeAsync()
    {
        if (Container != null)
            await Container.StopAsync();
    }
}

