using Dapper.Specifications.Dialects;
using Shouldly;
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
        dialect.ShouldNotBeNull();
        dialect.ShouldBeOfType<SqlServerDialect>();
        dialect.Name.ShouldBe("SQLServer");
    }

    [Fact]
    public void SqlDialect_PostgreSql_ShouldReturnPgSqlDialect()
    {
        // Act
        var dialect = SqlDialect.PostgreSql;

        // Assert
        dialect.ShouldNotBeNull();
        dialect.ShouldBeOfType<PgSqlDialect>();
        dialect.Name.ShouldBe("PostgreSQL");
    }

    [Fact]
    public void SqlDialect_MySql_ShouldReturnMySqlDialect()
    {
        // Act
        var dialect = SqlDialect.MySql;

        // Assert
        dialect.ShouldNotBeNull();
        dialect.ShouldBeOfType<MySqlDialect>();
        dialect.Name.ShouldBe("MySQL");
    }

    [Fact]
    public void SqlDialect_Sqlite_ShouldReturnSqliteDialect()
    {
        // Act
        var dialect = SqlDialect.Sqlite;

        // Assert
        dialect.ShouldNotBeNull();
        dialect.ShouldBeOfType<SqliteDialect>();
        dialect.Name.ShouldBe("SQLite");
    }

    [Fact]
    public void SqlDialect_ShouldReturnSameInstance()
    {
        // Act
        var dialect1 = SqlDialect.SqlServer;
        var dialect2 = SqlDialect.SqlServer;

        // Assert
        dialect1.ShouldBeSameAs(dialect2);
    }

    [Fact]
    public void AllDialects_ShouldHaveAtSymbolParameterPrefix()
    {
        // Act & Assert
        SqlDialect.SqlServer.ParameterPrefix.ShouldBe("@");
        SqlDialect.PostgreSql.ParameterPrefix.ShouldBe("@");
        SqlDialect.MySql.ParameterPrefix.ShouldBe("@");
        SqlDialect.Sqlite.ParameterPrefix.ShouldBe("@");
    }
}