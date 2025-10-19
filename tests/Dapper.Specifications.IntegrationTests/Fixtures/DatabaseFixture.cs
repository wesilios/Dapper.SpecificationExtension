using DotNet.Testcontainers.Containers;

namespace Dapper.Specifications.IntegrationTests.Fixtures;

/// <summary>
/// Base class for database fixtures that manage container lifecycle.
/// Use ICollectionFixture to share containers across multiple test classes.
/// </summary>
public abstract class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; protected set; } = string.Empty;
    protected IContainer? Container { get; set; }

    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
}