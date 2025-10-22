using Microsoft.Data.Sqlite;

namespace Dapper.Specifications.Examples.Examples;

/// <summary>
/// Interface for all example implementations
/// </summary>
public interface IExample
{
    /// <summary>
    /// Gets the example number (1-15)
    /// </summary>
    int ExampleNumber { get; }

    /// <summary>
    /// Gets the example title
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the example description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the example
    /// </summary>
    Task ExecuteAsync(SqliteConnection connection);
}