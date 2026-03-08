using Npgsql;
using QueryTune.Api.Models;
using System.Text.RegularExpressions;

namespace QueryTune.Api.Services
{
    public class ExplainPlanService
    {
        private readonly string _connectionString;

        public ExplainPlanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres");
        }

        public async Task<QueryAnalysisResult> AnalyzeQuery(string query)
        {
            var result = new QueryAnalysisResult
            {
                Score = 100
            };

            var plan = await GetExplainPlan(query);

            if (plan.Contains("Seq Scan"))
            {
                var tableName = ExtractTableName(plan);

                var column = ExtractWhereColumn(query);

                var indexes = await GetIndexes(tableName);

                var hasIndex = indexes.Any(i => i.Columns.Contains(column));

                if (!hasIndex && !string.IsNullOrEmpty(column))
                {
                    result.Issues.Add($"Sequential scan detected on {tableName}");

                    result.Suggestions.Add(
                        $"CREATE INDEX idx_{tableName}_{column} ON {tableName}({column});"
                    );

                    result.Score -= 30;
                }
            }

            return result;
        }

        private string ExtractTableName(string plan)
        {
            var match = Regex.Match(plan, @"Seq Scan on (\w+)");

            if (match.Success)
                return match.Groups[1].Value;

            return "";
        }

        private string ExtractWhereColumn(string query)
        {
            var match = Regex.Match(query, @"WHERE\s+(\w+)", RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Groups[1].Value;

            return "";
        }

        public async Task<string> GetExplainPlan(string query)
        {
            var explainQuery = $"EXPLAIN (ANALYZE, FORMAT JSON) {query}";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(explainQuery, conn);

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        public async Task<List<IndexInfo>> GetIndexes(string tableName)
        {
            var indexes = new List<IndexInfo>();

            var sql = @"
        SELECT
            i.relname AS index_name,
            array_to_string(array_agg(a.attname), ',') AS columns
        FROM
            pg_class t
            JOIN pg_index ix ON t.oid = ix.indrelid
            JOIN pg_class i ON i.oid = ix.indexrelid
            JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(ix.indkey)
        WHERE
            t.relname = @table
        GROUP BY
            i.relname;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var index = new IndexInfo
                {
                    IndexName = reader.GetString(0),
                    Columns = reader.GetString(1)
                        .Split(',')
                        .Select(c => c.Trim())
                        .ToList()
                };

                indexes.Add(index);
            }

            return indexes;
        }
    }
}
