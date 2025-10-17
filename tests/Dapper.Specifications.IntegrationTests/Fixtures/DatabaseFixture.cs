using DotNet.Testcontainers.Containers;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

public abstract class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; protected set; } = string.Empty;
    protected IContainer? Container { get; set; }

    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
}