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
}