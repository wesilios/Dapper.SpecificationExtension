using System.Data;
using Dapper.Specifications.IntegrationTests.Fixtures;

namespace Dapper.Specifications.IntegrationTests;

public abstract class SpecificationIntegrationTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : DatabaseFixture
{
    protected readonly TFixture Fixture;
    protected IDbConnection Connection;

    protected SpecificationIntegrationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }
}