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
}