using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastPin.Api.Data;

public static class DatabaseInitializer
{
    public static void Initialize(FastPinApiDbContext context)
    {
        context.Database.EnsureCreated();
        ApplySqliteMigrations(context);
    }

    private static void ApplySqliteMigrations(FastPinApiDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection() as SqliteConnection;
            if (connection == null)
            {
                return;
            }

            connection.Open();
            EnsureColumn(connection, "Tags", "Class", "ALTER TABLE Tags ADD COLUMN Class TEXT NULL");
            EnsureColumn(connection, "PinnedItems", "RichTextContent", "ALTER TABLE PinnedItems ADD COLUMN RichTextContent TEXT NULL");
            connection.Close();
        }
        catch
        {
            // Ignore migration failures for compatibility with existing DB states.
        }
    }

    private static void EnsureColumn(SqliteConnection connection, string tableName, string columnName, string alterSql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName})";

        using var reader = command.ExecuteReader();
        var hasColumn = false;
        while (reader.Read())
        {
            var existingColumn = reader.GetString(1);
            if (existingColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                hasColumn = true;
                break;
            }
        }

        if (hasColumn)
        {
            return;
        }

        reader.Close();
        using var alter = connection.CreateCommand();
        alter.CommandText = alterSql;
        alter.ExecuteNonQuery();
    }
}
