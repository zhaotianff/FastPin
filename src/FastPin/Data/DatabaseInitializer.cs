using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastPin.Data
{
    /// <summary>
    /// Handles database initialization and migrations
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Initializes the database and applies any necessary schema changes
        /// </summary>
        public static void Initialize(FastPinDbContext context)
        {
            context.Database.EnsureCreated();

            // Apply schema migrations for existing databases
            if (context.Database.IsSqlite())
            {
                ApplySqliteMigrations(context);
            }
            else if (context.Database.IsMySql())
            {
                ApplyMySqlMigrations(context);
            }
        }

        private static void ApplySqliteMigrations(FastPinDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection() as SqliteConnection;
                if (connection != null)
                {
                    connection.Open();

                    // Check if Class column exists in Tags table
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "PRAGMA table_info(Tags)";
                        using (var reader = command.ExecuteReader())
                        {
                            bool hasClassColumn = false;
                            while (reader.Read())
                            {
                                var columnName = reader.GetString(1);
                                if (columnName.Equals("Class", StringComparison.OrdinalIgnoreCase))
                                {
                                    hasClassColumn = true;
                                    break;
                                }
                            }

                            // Add Class column if it doesn't exist
                            if (!hasClassColumn)
                            {
                                reader.Close();
                                using (var alterCommand = connection.CreateCommand())
                                {
                                    alterCommand.CommandText = "ALTER TABLE Tags ADD COLUMN Class TEXT NULL";
                                    alterCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception)
            {
                // If migration fails, the column might already exist or database might be new
                // In either case, we can safely continue
            }
        }

        private static void ApplyMySqlMigrations(FastPinDbContext context)
        {
            try
            {
                using var connection = context.Database.GetDbConnection();
                connection.Open();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Tags' 
                    AND COLUMN_NAME = 'Class'";
                
                var result = command.ExecuteScalar();
                var hasClassColumn = result != null && Convert.ToInt32(result) > 0;

                if (!hasClassColumn)
                {
                    using var alterCommand = connection.CreateCommand();
                    alterCommand.CommandText = "ALTER TABLE Tags ADD COLUMN Class VARCHAR(50) NULL";
                    alterCommand.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // If migration fails, the column might already exist or database might be new
                // In either case, we can safely continue
            }
        }
    }
}
