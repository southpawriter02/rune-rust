using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// General database service for common database operations.
/// v0.26.2: Provides lightweight database access for services like TremorsenseService.
/// </summary>
public class DatabaseService
{
    private static readonly ILogger _log = Log.ForContext<DatabaseService>();
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("DatabaseService initialized");
    }

    /// <summary>
    /// Get a new database connection.
    /// </summary>
    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Execute a non-query command.
    /// </summary>
    public int ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
    {
        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Execute a scalar query.
    /// </summary>
    public object? ExecuteScalar(string sql, params (string name, object value)[] parameters)
    {
        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command.ExecuteScalar();
    }

    /// <summary>
    /// Execute a query and return results.
    /// </summary>
    public List<T> ExecuteQuery<T>(string sql, Func<SqliteDataReader, T> mapper, params (string name, object value)[] parameters)
    {
        var results = new List<T>();

        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(mapper(reader));
        }

        return results;
    }
}
