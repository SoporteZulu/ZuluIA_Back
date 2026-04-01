using Npgsql;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: ZuluIA_SmokeDbRunner <scalar|nonquery|file> [options]");
    return 1;
}

var mode = args[0].Trim().ToLowerInvariant();
var options = ParseOptions(args.Skip(1).ToArray());

var host = GetRequired(options, "host");
var port = GetRequired(options, "port");
var database = GetRequired(options, "database");
var username = GetRequired(options, "username");
var password = GetOptional(options, "password");

var builder = new NpgsqlConnectionStringBuilder
{
    Host = host,
    Port = int.Parse(port),
    Database = database,
    Username = username,
    Timeout = 15,
    CommandTimeout = 60
};

if (!string.IsNullOrEmpty(password))
{
    builder.Password = password;
}

var sql = mode switch
{
    "scalar" or "nonquery" => GetSql(options),
    "file" => null,
    _ => throw new InvalidOperationException($"Unsupported mode '{mode}'.")
};

var filePaths = mode == "file"
    ? ResolveSqlFilePaths(GetRequired(options, "file"))
    : Array.Empty<string>();

await using var connection = new NpgsqlConnection(builder.ConnectionString);
await connection.OpenAsync();

switch (mode)
{
    case "scalar":
    {
        await using var command = CreateCommand(connection, sql!);
        var result = await command.ExecuteScalarAsync();
        if (result is not null && result is not DBNull)
        {
            Console.WriteLine(Convert.ToString(result));
        }

        return 0;
    }
    case "nonquery":
    {
        await using var command = CreateCommand(connection, sql!);
        await command.ExecuteNonQueryAsync();
        return 0;
    }
    case "file":
        foreach (var filePath in filePaths)
        {
            var fileSql = await ReadSqlDocumentAsync(filePath);
            await using var command = CreateCommand(connection, fileSql);
            await command.ExecuteNonQueryAsync();
        }

        return 0;
    default:
        throw new InvalidOperationException($"Unsupported mode '{mode}'.");
}

static NpgsqlCommand CreateCommand(NpgsqlConnection connection, string sql)
{
    return new NpgsqlCommand(sql, connection)
    {
        CommandTimeout = 60
    };
}

static Dictionary<string, string> ParseOptions(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < args.Length; i++)
    {
        var current = args[i];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for option '{current}'.");
        }

        result[key] = args[++i];
    }

    return result;
}

static string GetRequired(IReadOnlyDictionary<string, string> options, string key)
{
    if (options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    throw new ArgumentException($"Missing required option '--{key}'.");
}

static string? GetOptional(IReadOnlyDictionary<string, string> options, string key)
{
    return options.TryGetValue(key, out var value) ? value : null;
}

static string GetSql(IReadOnlyDictionary<string, string> options)
{
    if (options.TryGetValue("sql-base64", out var sqlBase64) && !string.IsNullOrWhiteSpace(sqlBase64))
    {
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sqlBase64));
    }

    return GetRequired(options, "sql");
}

static string[] ResolveSqlFilePaths(string filePath)
{
    var fullPath = ResolveSqlDocumentPath(filePath);

    if (string.Equals(Path.GetFileNameWithoutExtension(fullPath), "zuluia_back_local_smoke_schema_fixes", StringComparison.OrdinalIgnoreCase))
    {
        var databaseDirectory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{fullPath}'.");

        return
        [
            Path.Combine(databaseDirectory, "zuluia_back_terceros_runtime_detected_fixes.md"),
            Path.Combine(databaseDirectory, "zuluia_back_terceros_full_upgrade.md")
        ];
    }

    return [fullPath];
}

static string ResolveSqlDocumentPath(string filePath)
{
    var fullPath = Path.GetFullPath(filePath);
    if (File.Exists(fullPath))
    {
        return fullPath;
    }

    if (string.Equals(Path.GetExtension(fullPath), ".sql", StringComparison.OrdinalIgnoreCase))
    {
        var markdownPath = Path.ChangeExtension(fullPath, ".md");
        if (File.Exists(markdownPath))
        {
            return markdownPath;
        }
    }

    if (string.Equals(Path.GetExtension(fullPath), ".md", StringComparison.OrdinalIgnoreCase))
    {
        var sqlPath = Path.ChangeExtension(fullPath, ".sql");
        if (File.Exists(sqlPath))
        {
            return sqlPath;
        }
    }

    return fullPath;
}

static async Task<string> ReadSqlDocumentAsync(string filePath)
{
    var content = await File.ReadAllTextAsync(filePath);
    if (!string.Equals(Path.GetExtension(filePath), ".md", StringComparison.OrdinalIgnoreCase))
    {
        return content;
    }

    var matches = System.Text.RegularExpressions.Regex.Matches(
        content,
        "(?ms)```sql\\s*(.*?)\\s*```");

    if (matches.Count == 0)
    {
        throw new InvalidOperationException($"No SQL fenced blocks were found in '{filePath}'.");
    }

    return string.Join(
        Environment.NewLine + Environment.NewLine,
        matches.Select(match => match.Groups[1].Value.Trim()));
}
