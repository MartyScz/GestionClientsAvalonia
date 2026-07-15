using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GestionClientsAvalonia;

public static class Database
{
    private const int CurrentDatabaseVersion = 1;
    private static readonly string ApplicationFolder = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionClientsAvalonia");

    private static readonly string DatabasePath = 
        Path.Combine(ApplicationFolder, "GestionClient.db");

    private static readonly string LegacyDatabasePath =
        Path.GetFullPath("GestionClient.db");
    
    private static readonly string ConnectionString =
        CreateConnectionString(DatabasePath);

    private static string CreateConnectionString(string databasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        };

        return builder.ToString();
    }

    private  static void PrepareDatabaseLocation(){
        Directory.CreateDirectory(ApplicationFolder);

        if (File.Exists(DatabasePath))
        {
            return;
        }

        if (!File.Exists(LegacyDatabasePath))
        {
            return;
        }

        using var sourceConnection = new SqliteConnection(CreateConnectionString(LegacyDatabasePath));

        using var destinationConnection = new SqliteConnection(ConnectionString);

        sourceConnection.Open();
        destinationConnection.Open();

        sourceConnection.BackupDatabase(destinationConnection);
    }

    public static SqliteConnection OpenConnection()
    {
        PrepareDatabaseLocation();

        SqliteConnection connection = new SqliteConnection(ConnectionString);

        connection.Open();

        return connection;
    }

    public static void Initialize(SqliteConnection connection)
    {
        int version = GetDatabaseVersion(connection);

        if (version > CurrentDatabaseVersion)
        {
            throw new InvalidOperationException(
                "La base de données utilise une version plus récente que l'application."
            );
        }

        if (version == 0)
        {
            if (ClientsTableExists(connection))
            {
                MigrateFromVersion0To1(connection);
            }
            else
            {
                CreateVersion1Schema(connection);
            }
        }
    }

    private static int GetDatabaseVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "PRAGMA user_version";

        object? result = command.ExecuteScalar();

        return Convert.ToInt32(result);
    }

    private static void SetDatabaseVersion(SqliteConnection connection, SqliteTransaction transaction, int version)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = $"PRAGMA user_version = {version};";

        command.ExecuteNonQuery();
    }

    private static void MigrateFromVersion0To1(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText =
            $"""
            CREATE TABLE Clients_New (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nom TEXT NOT NULL
                    CHECK (length(Nom) <= {ClientRules.MaxNameLength}),
                Email TEXT NOT NULL
                    CHECK (length(Email) <= {ClientRules.MaxEmailLength})
            );

            INSERT INTO Clients_New (Id, Nom, Email)
            SELECT Id, Nom, Email
            FROM Clients;

            DROP TABLE Clients;

            ALTER TABLE Clients_New RENAME TO Clients;

            CREATE UNIQUE INDEX UX_Clients_Email_NoCase
            ON Clients (Email COLLATE NOCASE);
            """;

            command.ExecuteNonQuery();

            SetDatabaseVersion(connection, transaction, 1);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static bool ClientsTableExists( SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT COUNT(*)
        FROM sqlite_master
        WHERE type = 'table'
            AND name = 'Clients'
        """;

        object? result = command.ExecuteScalar();

        return Convert.ToInt32(result) > 0;
    }

    private static void CreateVersion1Schema( SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = 
            $"""
            CREATE TABLE Clients (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nom TEXT NOT NULL
                CHECK (length(Nom) <= {ClientRules.MaxNameLength}),
            Email TEXT NOT NULL
                CHECK (length(Email) <= {ClientRules.MaxEmailLength})
            );

            CREATE UNIQUE INDEX UX_Clients_Email_NoCase
            ON Clients (Email COLLATE NOCASE);
            """;

            command.ExecuteNonQuery();

            SetDatabaseVersion(connection, transaction, 1);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

}