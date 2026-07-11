using Microsoft.Data.Sqlite;

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
}