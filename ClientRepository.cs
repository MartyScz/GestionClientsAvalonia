using Avalonia.Controls.Converters;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;


namespace GestionClientsAvalonia;

public class ClientRepository
{
    public int Add(Client client)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        INSERT INTO Clients (Nom, Email)
        VALUES (@nom, @email)
        RETURNING Id;
        """;

        command.Parameters.AddWithValue("@nom", client.Nom);
        command.Parameters.AddWithValue("@email", client.Email);

        long newId = (long)command.ExecuteScalar()!;

        return (int)newId;
    }

    public List<Client> GetAll()
    {
        List<Client> clients = new();

        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        SELECT Id, Nom, Email
        FROM Clients
        ORDER BY Id;
        """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            Client client = new Client
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Email = reader.GetString(2)
            };

            clients.Add(client);
        }
        
        return clients;
    }

    public bool Update(Client client)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        UPDATE Clients
        SET Nom = @nom,
            Email = @email
        WHERE Id = @id;
        """;

        command.Parameters.AddWithValue("@nom", client.Nom);
        command.Parameters.AddWithValue("@email", client.Email);
        command.Parameters.AddWithValue("@id", client.Id);

        int rowAffected = command.ExecuteNonQuery();

        return rowAffected > 0;
    }

    public bool Delete(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        DELETE FROM Clients
        WHERE Id = @id;
        """;

        command.Parameters.AddWithValue("@id", id);

        int rowAffected = command.ExecuteNonQuery();

        return rowAffected > 0;
    }

    public List<Client> Search(string searchText)
    {
        List<Client> clients = new();

        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        SELECT Id, Nom, Email
        FROM Clients
        WHERE Nom LIKE @search
            OR Email Like @search
        ORDER BY Id;
        """;

        command.Parameters.AddWithValue(
            "@search",
            $"%{searchText}%"
        );

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            Client client = new Client
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Email = reader.GetString(2)
            };

            clients.Add(client);
        }

        return clients;
    }

    public bool EmailExists(string email)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = 
        """
        SELECT COUNT(*)
        FROM Clients
        WHERE Email = @email COLLATE NOCASE;
        """;

        command.Parameters.AddWithValue("@email", email.Trim());

        long count = (long)command.ExecuteScalar()!;

        return count > 0;
    }
}