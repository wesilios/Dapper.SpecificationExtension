using Dapper.Specifications.Dialects;
using FluentAssertions;
using Xunit;

namespace Dapper.Specifications.UnitTests.SpecificationTests;

public class SqlDialectFactoryTests
{
    [Fact]
    public void SqlDialect_SqlServer_ShouldReturnSqlServerDialect()
    {
        // Act
        var dialect = SqlDialect.SqlServer;

        // Assert
        dialect.Should().NotBeNull();
        dialect.Should().BeOfType<SqlServerDialect>();
        dialect.Name.Should().Be("SQLServer");
    }

    [Fact]
    public void SqlDialect_PostgreSql_ShouldReturnPgSqlDialect()
    {
        // Act
        var dialect = SqlDialect.PostgreSql;

        // Assert
        dialect.Should().NotBeNull();
        dialect.Should().BeOfType<PgSqlDialect>();
        dialect.Name.Should().Be("PostgreSQL");
    }

    [Fact]
    public void SqlDialect_MySql_ShouldReturnMySqlDialect()
    {
        // Act
        var dialect = SqlDialect.MySql;

        // Assert
        dialect.Should().NotBeNull();
        dialect.Should().BeOfType<MySqlDialect>();
        dialect.Name.Should().Be("MySQL");
    }

    [Fact]
    public void SqlDialect_Sqlite_ShouldReturnSqliteDialect()
    {
        // Act
        var dialect = SqlDialect.Sqlite;

        // Assert
        dialect.Should().NotBeNull();
        dialect.Should().BeOfType<SqliteDialect>();
        dialect.Name.Should().Be("SQLite");
    }

    [Fact]
    public void SqlDialect_ShouldReturnSameInstance()
    {
        // Act
        var dialect1 = SqlDialect.SqlServer;
        var dialect2 = SqlDialect.SqlServer;

        // Assert
        dialect1.Should().BeSameAs(dialect2);
    }

    [Fact]
    public void AllDialects_ShouldHaveAtSymbolParameterPrefix()
    {
        // Act & Assert
        SqlDialect.SqlServer.ParameterPrefix.Should().Be("@");
        SqlDialect.PostgreSql.ParameterPrefix.Should().Be("@");
        SqlDialect.MySql.ParameterPrefix.Should().Be("@");
        SqlDialect.Sqlite.ParameterPrefix.Should().Be("@");
    }
}