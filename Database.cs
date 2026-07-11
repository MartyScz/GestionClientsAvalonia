using System.Threading;
using Microsoft.Data.Sqlite;

namespace GestionClientsAvalonia;

public static class Database
{
    private const string ConnectionString = "Data Source=GestionClient.db";

    public static SqliteConnection OpenConnection()
    {
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
    }
}