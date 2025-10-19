namespace Dapper.Specifications;

/// <summary>
/// Contains SQL keyword constants used throughout the specification builder.
/// </summary>
public static class SqlKeywords
{
    /// <summary>
    /// SELECT keyword
    /// </summary>
    public const string Select = "SELECT";

    /// <summary>
    /// FROM keyword
    /// </summary>
    public const string From = "FROM";

    /// <summary>
    /// WHERE keyword
    /// </summary>
    public const string Where = "WHERE";

    /// <summary>
    /// GROUP BY keywords
    /// </summary>
    public const string GroupBy = "GROUP BY";

    /// <summary>
    /// HAVING keyword
    /// </summary>
    public const string Having = "HAVING";

    /// <summary>
    /// ORDER BY keywords
    /// </summary>
    public const string OrderBy = "ORDER BY";

    /// <summary>
    /// COUNT(*) function
    /// </summary>
    public const string CountAll = "COUNT(*)";

    /// <summary>
    /// LIMIT keyword
    /// </summary>
    public const string Limit = "LIMIT";

    /// <summary>
    /// OFFSET keyword
    /// </summary>
    public const string Offset = "OFFSET";

    /// <summary>
    /// EXISTS keyword
    /// </summary>
    public const string Exists = "EXISTS";

    /// <summary>
    /// SELECT 1 query fragment
    /// </summary>
    public const string SelectOne = "SELECT 1";

    /// <summary>
    /// SELECT CASE WHEN EXISTS query fragment for PostgreSQL
    /// </summary>
    public const string SelectCaseWhenExists = "SELECT CASE WHEN EXISTS";

    /// <summary>
    /// THEN 1 ELSE 0 END query fragment
    /// </summary>
    public const string ThenOneElseZeroEnd = "THEN 1 ELSE 0 END";

    /// <summary>
    /// SELECT EXISTS query fragment
    /// </summary>
    public const string SelectExists = "SELECT EXISTS";

    /// <summary>
    /// ROWS keyword (SQL Server pagination)
    /// </summary>
    public const string Rows = "ROWS";

    /// <summary>
    /// FETCH NEXT keywords (SQL Server pagination)
    /// </summary>
    public const string FetchNext = "FETCH NEXT";

    /// <summary>
    /// ONLY keyword (SQL Server pagination)
    /// </summary>
    public const string Only = "ONLY";

    /// <summary>
    /// OFFSET keyword with ROWS (SQL Server pagination)
    /// </summary>
    public const string OffsetRows = "OFFSET";

    /// <summary>
    /// Space character for SQL formatting
    /// </summary>
    public const string Space = " ";

    /// <summary>
    /// Comma character for SQL formatting
    /// </summary>
    public const string Comma = ",";

    /// <summary>
    /// INNER JOIN - Returns only rows that have matching values in both tables.
    /// </summary>
    public const string InnerJoin = "INNER JOIN";

    /// <summary>
    /// LEFT JOIN (LEFT OUTER JOIN) - Returns all rows from the left table, and the matching rows from the right table.
    /// </summary>
    public const string LeftJoin = "LEFT JOIN";

    /// <summary>
    /// RIGHT JOIN (RIGHT OUTER JOIN) - Returns all rows from the right table, and the matching rows from the left table.
    /// </summary>
    public const string RightJoin = "RIGHT JOIN";

    /// <summary>
    /// FULL JOIN (FULL OUTER JOIN) - Returns all rows from both tables.
    /// </summary>
    public const string FullJoin = "FULL JOIN";

    /// <summary>
    /// CROSS JOIN - Returns the Cartesian product of both tables (all possible combinations).
    /// </summary>
    public const string CrossJoin = "CROSS JOIN";
}

