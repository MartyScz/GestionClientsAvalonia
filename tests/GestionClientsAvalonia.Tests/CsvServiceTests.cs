using System;
using System.Collections.Generic;
using System.IO;
using GestionClientsAvalonia;
using Xunit;


namespace GestionClientsAvalonia.Tests;

public class CsvServiceTests
{
    [Fact]
    public void ExportThenImport_WithSimpleClients_PreserversNameAndEmail()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"clients-test-{Guid.NewGuid()}.csv");

        List<Client> clients = 
        [
            new Client
            {
                Id = 1,
                Nom = "Marty",
                Email = "marty@example.com"    
            },
            new Client
            {
                Id = 2,
                Nom = "Elise",
                Email = "elise@example.com"
            }
        ];

        try
        {
            CsvService.ExportClients(filePath, clients);

            List<Client> importedClients = CsvService.ImportClients(filePath);

            Assert.Equal(2, importedClients.Count);

            Assert.Equal("Marty", importedClients[0].Nom);
            Assert.Equal("marty@example.com", importedClients[0].Email);

            Assert.Equal("Elise", importedClients[1].Nom);
            Assert.Equal("elise@example.com", importedClients[1].Email);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ExportThenImport_WithSemicolonAndQuotes_PreservesValues()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"clients-test-{Guid.NewGuid()}.csv");
        
        List<Client> clients = 
        [
            new Client
            {
                Id = 3,
                Nom = "Martin; \"Le Rouge\"",
                Email = "martin@example.com"    
            },
        ];
        try
        {
            CsvService.ExportClients(filePath, clients);

            List<Client> importedClients = CsvService.ImportClients(filePath);

            Client importedClient = Assert.Single(importedClients);

            Assert.Equal("Martin; \"Le Rouge\"", importedClient.Nom);
            Assert.Equal("martin@example.com", importedClient.Email);
            
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ImportClients_WithoutRequiredColumns_ThrowsInvalidOperationException()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"clients-test-{Guid.NewGuid()}.csv");

        try
        {
            File.WriteAllText(filePath, "Id;Telephone" + Environment.NewLine + "1,0600000000");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CsvService.ImportClients(filePath));

            Assert.Equal("Le fichier CSV doit contenir les colonnes Nom et Email.", exception.Message);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ImportClients_WithInvalidRows_IgnoresThem()
    {
        string filePath = Path.Combine(Path.GetTempPath(),$"clients-test-{Guid.NewGuid()}.csv");

        string csvContent = string.Join(
            Environment.NewLine,
            "Nom;Email",
            "Marty;marty@example.com",
            "",
            "SansEmail;",
            ";sans.nom@example.com",
            "LigneIncomplete",
            "  Alice  ;  alice@example.com  "
        );

        try
        {
            File.WriteAllText(filePath, csvContent);

            List<Client> importedClients = CsvService.ImportClients(filePath);

            Assert.Equal(2, importedClients.Count);

            Assert.Equal("Marty", importedClients[0].Nom);
            Assert.Equal("marty@example.com", importedClients[0].Email);

            Assert.Equal("Alice", importedClients[1].Nom);
            Assert.Equal("alice@example.com", importedClients[1].Email);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ImportClients_WithDifferentHeaderCasing_ImportsClients()
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"clients-test-{Guid.NewGuid()}.csv");

        string csvContent = string.Join(Environment.NewLine,"nOm;eMaIl","Marty;marty@example.com");

        try
        {
            File.WriteAllText(filePath, csvContent);

            List<Client> importedClients = CsvService.ImportClients(filePath);

            Client importedClient = Assert.Single(importedClients);

            Assert.Equal("Marty", importedClient.Nom);
            Assert.Equal("marty@example.com", importedClient.Email);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ExportClients_WithNewLineInName_ThrowsInvalidOperationException()
    {
        string filePath = Path.Combine(Path.GetTempPath(),$"clients-test-{Guid.NewGuid()}.csv");

        string name = "Martin" + Environment.NewLine + "Le Rouge";

        List<Client> clients =
        [
            new Client
            {
                Id = 5,
                Nom = name,
                Email = "martinlerouge@example.com"
            }
        ];

        try
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CsvService.ExportClients(filePath, clients));

            Assert.Equal("Les retours à la ligne ne sont pas autorisés dans les champs CSV.", exception.Message);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

}