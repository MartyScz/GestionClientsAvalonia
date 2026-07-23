using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
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

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}