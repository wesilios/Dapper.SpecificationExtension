namespace Dapper.Specifications.IntegrationTests.Fixtures;

public class SqliteDatabaseFixture : DatabaseFixture
{
    public override Task InitializeAsync()
    {
        // SQLite uses in-memory database, no container needed
        ConnectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";
        return Task.CompletedTask;
    }

    public override Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

