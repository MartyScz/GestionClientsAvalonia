using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using Avalonia.Platform.Storage;
using System.ComponentModel;
using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace GestionClientsAvalonia;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Client> _clients = new();
    private readonly ClientRepository _clientRepository = new();

    public MainWindow()
    {
        InitializeComponent();

        using var connection = Database.OpenConnection();
        Database.Initialize(connection);
        ClientListBox.ItemsSource = _clients;

        List<Client> clientsFromDatabase = _clientRepository.GetAll();

        foreach (Client client in clientsFromDatabase)
        {
            _clients.Add(client);
        }
    }

    private void AddClient_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string nom = NomTextBox.Text ?? "";
        string email = EmailTextBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
        {
            MessageTextBlock.Text = "Le nom et l'email sont obligatoires.";
            return;
        }

        nom = nom.Trim();
        email =email.Trim();

        if (!EmailValidator.IsValid(email))
        {
            MessageTextBlock.Text = "L'adresse email n'est pas valide";

            return;
        }

        if (_clientRepository.EmailExists(email))
        {
            MessageTextBlock.Text = "Un client possède déjà cette adresse email.";

            return;
        }

        Client client = new Client
        {
            Nom = nom,
            Email = email
        };

        try
        {
            client.Id = _clientRepository.Add(client);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19 && ex.SqliteExtendedErrorCode == 2067)
        {
            MessageTextBlock.Text = "Impossible d'ajouter ce client : cette adresse email existe déjà";

            return;
        }
        catch (SqliteException)
        {
            MessageTextBlock.Text = "Une erreur de base de données est survenue pendant l'ajout.";

            return;
        }

        _clients.Add(client);

        NomTextBox.Text = "";
        EmailTextBox.Text = "";

        MessageTextBlock.Text = $"Client enregistré : Id : {client.Id} | Nom : {nom} | Email : {email}";
    }

    private void ClientListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            return;
        }

        NomTextBox.Text = selectedClient.Nom;
        EmailTextBox.Text = selectedClient.Email;

        MessageTextBlock.Text = $"Client sélectionné : Id {selectedClient.Id} | Nom : {selectedClient.Nom} | Email : {selectedClient.Email}";
    }

    private void UpdateClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            MessageTextBlock.Text = "Sélectionne un client à modifier.";
            return;
        }

        string nom = NomTextBox.Text ?? "";
        string email = EmailTextBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
        {
            MessageTextBlock.Text = "Le nom et l'email sont obligatoires.";
            return;
        }

        nom = nom.Trim();
        email = email.Trim();

        if (!EmailValidator.IsValid(email))
        {
            MessageTextBlock.Text = "L'adresse email n'est pas valide.";
                return;
        }

        if (_clientRepository.EmailExistsForAnotherClient(email, selectedClient.Id))
        {
            MessageTextBlock.Text = "Un autre client possède déjà cette adresse email";

            return;
        }

        Client updateClient = new Client
        {
            Id = selectedClient.Id,
            Nom = nom,
            Email = email
        };

        bool isUpdated;

        try
        {
            isUpdated = _clientRepository.Update(updateClient);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19 && ex.SqliteExtendedErrorCode == 2067)
        {
            MessageTextBlock.Text = "Impossible de modifier ce client : cette adresse mail existe déjà.";

            return;
        }
        catch (SqliteException)
        {
            MessageTextBlock.Text = "Une erreur de base de données est survenue pendant la modification.";

            return;
        }

        if (!isUpdated)
        {
            MessageTextBlock.Text = " Le client n'a pas été trouvé dans la base.";
            return;
        }

        int SelectedIndex = ClientListBox.SelectedIndex;

        _clients[SelectedIndex] = updateClient;

        ClientListBox.SelectedItem = updateClient;

        MessageTextBlock.Text = $"Client modifié : Id {updateClient.Id} | Nom : {updateClient.Nom} | Email : {updateClient.Email}";
    }

    private async void DeleteClient_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            MessageTextBlock.Text = "Sélectionne un client à supprimer.";
            return;
        }

        DeleteConfirmationWindow confirmationWindow = new DeleteConfirmationWindow(selectedClient);

        bool isConfirmed = await confirmationWindow.ShowDialog<bool>(this);

        if (!isConfirmed)
        {
            MessageTextBlock.Text = "Suppression annulée.";
            return;
        }

        bool isDeleted;
        try
        {
            isDeleted = _clientRepository.Delete(selectedClient.Id);
        }
        catch (SqliteException)
        {
            MessageTextBlock.Text = "Une erreur de base de données est survenue pendant la suppression.";

            return;
        }

        if (!isDeleted)
        {
            MessageTextBlock.Text = "Le client n'a pas été trouvé dans la base.";
            return;
        }

        _clients.Remove(selectedClient);

        NomTextBox.Text ="";
        EmailTextBox.Text ="";

        MessageTextBlock.Text = $"Client supprimé : Id {selectedClient.Id} | Nom {selectedClient.Nom}.";
    }

    private void SearchClient_Click(object? sender, RoutedEventArgs e)
    {
        string searchText = SearchTextBox.Text ?? "";

        List<Client> searchResults;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            searchResults = _clientRepository.GetAll();
        }
        else
        {
            searchResults = _clientRepository.Search(searchText);
        }

        ClientListBox.SelectedItem = null;

        _clients.Clear();

        foreach (Client client in searchResults)
        {
            _clients.Add(client);
        }

        MessageTextBlock.Text = $"{searchResults.Count} client(s) trouvé(s).";
    }

   private async void ExportClients_Click( object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Exporter les clients",
                SuggestedFileName = "clients.csv"
            }
        );

        if (file is null)
        {
            MessageTextBlock.Text = "Export annulé.";
            return;
        }

        string? filePath = file.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            MessageTextBlock.Text = "Impossible de récupérer le chemin du fichier.";
            return;
        }

        List<Client> clients = _clientRepository.GetAll();

        try
        {
            CsvService.ExportClients(filePath, clients);
        }
        catch (UnauthorizedAccessException)
        {
            MessageTextBlock.Text = "L'accès au fichier a été refusé. Vérifie se permissions.";

            return;
        }
        catch (IOException)
        {
            MessageTextBlock.Text = "Impossible d'écrire dans le fichier. Il est peut-être déjà ouvert.";
        }


        MessageTextBlock.Text = $"{clients.Count} client(s) exporté(s) dans {file.Name}.";
    }

    private async void ImportClients_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Importer des clients",
                AllowMultiple = false,

                FileTypeFilter =
                [
                    new FilePickerFileType("Fichiers CSV")
                    {
                        Patterns = ["*.csv"]
                    }
                ]
            }
        );

        if (files.Count == 0)
        {
            MessageTextBlock.Text = "Import annulé.";
            return;
        }

        string? filePath = files[0].TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            MessageTextBlock.Text = "Impossible de récupérer le chemin du fichier.";
            return;
        }

        try
        {
            List<Client> clientsToImport = CsvService.ImportClients(filePath);

            int importedCount = 0;
            int ignoredCount = 0;
            int invalidCount = 0;

            foreach (Client client in clientsToImport)
            {
                client.Nom = client.Nom.Trim();
                client.Email = client.Email.Trim();

                if (!EmailValidator.IsValid(client.Email))
                {
                    invalidCount++;
                    continue;
                }

                if (_clientRepository.EmailExists(client.Email))
                {
                    ignoredCount++;
                    continue;
                }

                client.Id = _clientRepository.Add(client);

                importedCount++;
            }

            ClientListBox.SelectedItem = null;
            SearchTextBox.Text = "";

            _clients.Clear();

            List<Client> allClients = _clientRepository.GetAll();

            foreach (Client client in allClients)
            {
                _clients.Add(client);
            }

            MessageTextBlock.Text = 
                $"Import terminé : {importedCount} client(s) ajouté(s), " +
                $"{ignoredCount} doublon(s) ignoré(s), " +
                $"{invalidCount} adresse(s) invalide(s) ignorée(s)";
        }
        catch (InvalidOperationException ex)
        {
            MessageTextBlock.Text = ex.Message;
        }
        catch (UnauthorizedAccessException)
        {
            MessageTextBlock.Text = " L'accès au fichier a été refusé. Vérifie ses permissions.";
        }
        catch (IOException)
        {
            MessageTextBlock.Text = "Impossible de lire le fichier. Il est peut-être déjà utilisé.";
        }
        catch (SqliteException)
        {
            MessageTextBlock.Text = "Une erreur de base de données est survenue pendant l'import";
        }
    }

}