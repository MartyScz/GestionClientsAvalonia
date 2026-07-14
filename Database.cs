using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GestionClientsAvalonia;

public static class Database
{
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
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Clients (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nom TEXT NOT NULL,
            Email TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();

        using var uniqueEmailCommand = connection.CreateCommand();

        uniqueEmailCommand.CommandText = 
        """
        CREATE UNIQUE INDEX IF NOT EXISTS UX_Clients_Email_NoCase
        ON Clients (Email COLLATE NOCASE);
        """;

        uniqueEmailCommand.ExecuteNonQuery();
    }

}