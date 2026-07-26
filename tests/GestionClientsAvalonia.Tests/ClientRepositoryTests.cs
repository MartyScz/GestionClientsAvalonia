using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Pipelines;
using GestionClientsAvalonia;
using HarfBuzzSharp;
using Microsoft.Data.Sqlite;
using Xunit;

namespace GestionClientsAvalonia.Tests;

public class ClientRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly ClientRepository _repository;

    public ClientRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(),$"gestion-clients-test-{Guid.NewGuid()}.db");

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Pooling = false
        }.ToString();

        using SqliteConnection connection = OpenTestConnection();

        Database.Initialize(connection);

        _repository = new ClientRepository(OpenTestConnection);
    }

    private SqliteConnection OpenTestConnection()
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);

        connection.Open();

        return connection;
    }

    [Fact]
    public void AddThenGetAll_WithValidClient_PersistsClient()
    {
        Client client = new Client
        {
            Nom ="Marty",
            Email = "marty@example.com"
        };

        int newId = _repository.Add(client);

        List<Client> clients = _repository.GetAll();

        Client savedClient = Assert.Single(clients);

        Assert.True(newId > 0);

        Assert.Equal(newId, savedClient.Id);

        Assert.Equal("Marty", savedClient.Nom);

        Assert.Equal("marty@example.com", savedClient.Email);
    }

    [Fact]
    public void Update_WithExistingClient_PersistsChanges()
    {
        Client originalClient = new Client
        {
            Nom = "Marty",
            Email = "marty@example.com"
        };

        int id = _repository.Add(originalClient);

        Client updatedClient = new Client
        {
            Id = id,
            Nom = "Martin",
            Email = "martin@example.com"
        };

        bool isUpdated = _repository.Update(updatedClient);

        List<Client> clients = _repository.GetAll();

        Assert.True(isUpdated);

        Client savedClient = Assert.Single(clients);

        Assert.Equal(id, savedClient.Id);

        Assert.Equal("Martin", savedClient.Nom);

        Assert.Equal("martin@example.com", savedClient.Email);
    }

    [Fact]
    public void Delete_WithExistingClient_RemovesClient()
    {
       
        Client deleteClient = new Client
        {
            Nom = "Marty",
            Email = "marty@example.com"
        };

        int id = _repository.Add(deleteClient);
        bool isDeleted = _repository.Delete(id);

        Assert.True(isDeleted);

        List<Client> clients = _repository.GetAll();

        Assert.Empty(clients);
    }

    [Fact]
    public void Delete_WithUnknownId_ReturnsFalse()
    {
        bool isDeleted = _repository.Delete(999);

        List<Client> clients = _repository.GetAll();

        Assert.False(isDeleted);
        
        Assert.Empty(clients);
    }

    [Fact]
    public void Search_WithMatchingName_ReturnsMatchingClient()
    {
        Client newClient = new Client
        {
            Nom = "Marty",
            Email = "marty1@example.com"
        };

        Client otherClient = new Client
        {
            Nom= "Luna",
            Email = "luna@example.com"
        };

        _repository.Add(newClient);
        _repository.Add(otherClient);

        List<Client> searchResults = _repository.Search("Mar");

        Client searchClient = Assert.Single(searchResults);

        Assert.Equal("Marty", searchClient.Nom);
    }

    [Fact]
    public void Search_WithNoMatchingName_ReturnsEmptyList()
    {
        Client fakeClient = new Client
        {
            Nom = "Marty",
            Email = "marty@example.com"

        };

        _repository.Add(fakeClient);

        List<Client> searchResult = _repository.Search("Luna");

        Assert.Empty(searchResult);
    }

    [Fact]
    public void Search_IsCaseInsensitive()
    {
        Client newClient = new Client
        {
            Nom = "Marty",
            Email = "marty@example.com"
        };

        _repository.Add(newClient);

        List<Client> searchResult = _repository.Search("marty");

        Client searchClient = Assert.Single(searchResult);

        Assert.Equal("Marty", searchClient.Nom);
    }

    [Fact]
    public void AddMany_WithDuplicateEmails_InsertsOnlyClient()
    {
        List<Client> clients =
        [
            new Client
            {
                Nom = "Premier client",
                Email = "doublon@example.com"
            },

            new Client
            {
                Nom = "Deuxième client",
                Email = "doublon@example.com"
            }
        ];

        int importedCount = _repository.AddMany(clients);

        List<Client> savedClients = _repository.GetAll();

        Assert.Equal(1, importedCount);

        Client savedClient = Assert.Single(savedClients);

        Assert.Equal("Premier client", savedClient.Nom);
        Assert.Equal("doublon@example.com", savedClient.Email);
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}