using System.Data;
using System.Text.Json;
using DD.Gateway.Data;
using DD.Gateway.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Modules.Dev.Controllers;

[ApiController]
[Route("api/dev/database")]
[Authorize(Policy = PartnerScopes.DevDatabaseRead)]
public sealed class DatabaseExplorerController(GatewayDbContext dbContext) : ControllerBase
{
    [HttpGet("tables")]
    [ProducesResponseType(typeof(IReadOnlyList<DatabaseTable>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<DatabaseTable>>> GetTables(
        CancellationToken cancellationToken)
    {
        var tables = await WithOpenConnectionAsync(async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT table_info.table_schema,
                       table_info.table_name,
                       table_info.table_type,
                       column_info.column_name,
                       column_info.data_type
                FROM information_schema.tables AS table_info
                INNER JOIN information_schema.columns AS column_info
                    ON column_info.table_schema = table_info.table_schema
                    AND column_info.table_name = table_info.table_name
                WHERE table_info.table_type IN ('BASE TABLE', 'VIEW')
                    AND table_info.table_schema NOT IN ('information_schema')
                    AND table_info.table_schema NOT LIKE 'pg_%'
                ORDER BY table_info.table_schema,
                         table_info.table_name,
                         column_info.ordinal_position
                """;

            var results = new List<DatabaseTableBuilder>();
            var byName = new Dictionary<string, DatabaseTableBuilder>(StringComparer.Ordinal);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var schema = reader.GetString(0);
                var table = reader.GetString(1);
                var key = $"{schema}\0{table}";
                if (!byName.TryGetValue(key, out var tableBuilder))
                {
                    tableBuilder = new DatabaseTableBuilder(schema, table, reader.GetString(2));
                    byName.Add(key, tableBuilder);
                    results.Add(tableBuilder);
                }

                tableBuilder.Columns.Add(new DatabaseColumn(reader.GetString(3), reader.GetString(4)));
            }

            return (IReadOnlyList<DatabaseTable>)results
                .Select(tableBuilder => new DatabaseTable(
                    tableBuilder.Schema,
                    tableBuilder.Name,
                    tableBuilder.Type,
                    tableBuilder.Columns))
                .ToArray();
        }, cancellationToken);

        return Ok(tables);
    }

    [HttpGet("rows")]
    [ProducesResponseType(typeof(DatabaseRowsPage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DatabaseRowsPage>> GetRows(
        [FromQuery] string schema,
        [FromQuery] string table,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(table))
        {
            return BadRequest(new { message = "Cần chọn schema và table." });
        }

        if (page < 1 || pageSize is < 1 or > 100
            || (long)(page - 1) * pageSize > int.MaxValue)
        {
            return BadRequest(new { message = "page phải từ 1; pageSize phải trong khoảng 1–100." });
        }

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (search?.Length > 200)
        {
            return BadRequest(new { message = "Từ khóa tìm kiếm tối đa 200 ký tự." });
        }

        var result = await WithOpenConnectionAsync(async connection =>
        {
            await using (var existsCommand = connection.CreateCommand())
            {
                existsCommand.CommandText = """
                    SELECT EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_schema = @schema
                            AND table_name = @table
                            AND table_type IN ('BASE TABLE', 'VIEW')
                            AND table_schema NOT IN ('information_schema')
                            AND table_schema NOT LIKE 'pg_%'
                    )
                    """;
                AddParameter(existsCommand, "schema", schema);
                AddParameter(existsCommand, "table", table);
                if (!Convert.ToBoolean(await existsCommand.ExecuteScalarAsync(cancellationToken)))
                {
                    return null;
                }
            }

            var columns = new List<DatabaseColumn>();
            await using (var columnsCommand = connection.CreateCommand())
            {
                columnsCommand.CommandText = """
                    SELECT column_name, data_type
                    FROM information_schema.columns
                    WHERE table_schema = @schema AND table_name = @table
                    ORDER BY ordinal_position
                    """;
                AddParameter(columnsCommand, "schema", schema);
                AddParameter(columnsCommand, "table", table);
                await using var columnReader = await columnsCommand.ExecuteReaderAsync(cancellationToken);
                while (await columnReader.ReadAsync(cancellationToken))
                {
                    columns.Add(new DatabaseColumn(columnReader.GetString(0), columnReader.GetString(1)));
                }
            }

            var rows = new List<JsonElement>(pageSize + 1);
            var offset = (long)(page - 1) * pageSize;
            var commandText = $"""
                SELECT to_jsonb(row_value)::text
                FROM (
                    SELECT * FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)}
                ) AS row_value
                WHERE @search = '' OR to_jsonb(row_value)::text ILIKE @search
                LIMIT @limit OFFSET @offset
                """;

            await using (var rowsCommand = connection.CreateCommand())
            {
                rowsCommand.CommandText = commandText;
                AddParameter(rowsCommand, "limit", pageSize + 1);
                AddParameter(rowsCommand, "offset", offset);
                AddParameter(rowsCommand, "search", search is null ? string.Empty : $"%{search}%");
                await using var rowReader = await rowsCommand.ExecuteReaderAsync(cancellationToken);
                while (await rowReader.ReadAsync(cancellationToken))
                {
                    using var document = JsonDocument.Parse(rowReader.GetString(0));
                    rows.Add(document.RootElement.Clone());
                }
            }

            var hasNext = rows.Count > pageSize;
            if (hasNext)
            {
                rows.RemoveAt(rows.Count - 1);
            }

            return new DatabaseRowsPage(
                schema,
                table,
                columns,
                rows,
                page,
                pageSize,
                hasNext);
        }, cancellationToken);

        return result is null ? NotFound(new { message = "Không tìm thấy bảng hoặc view." }) : Ok(result);
    }

    private async Task<T> WithOpenConnectionAsync<T>(
        Func<System.Data.Common.DbConnection, Task<T>> action,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == ConnectionState.Closed;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            return await action(connection);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    public sealed record DatabaseColumn(string Name, string DataType);

    public sealed record DatabaseTable(
        string Schema,
        string Name,
        string Type,
        IReadOnlyList<DatabaseColumn> Columns);

    public sealed record DatabaseRowsPage(
        string Schema,
        string Table,
        IReadOnlyList<DatabaseColumn> Columns,
        IReadOnlyList<JsonElement> Rows,
        int Page,
        int PageSize,
        bool HasNext);

    private sealed record DatabaseTableBuilder(string Schema, string Name, string Type)
    {
        public List<DatabaseColumn> Columns { get; } = [];
    }
}
