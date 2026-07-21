using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using System;
using Microsoft.Data.Sqlite;
using System.IO;
using Avalonia.Media;

namespace GestionClientsAvalonia;


public partial class MainWindow : Window
{
    private readonly ObservableCollection<Client> _clients = new();
    private readonly ClientRepository _clientRepository = new();

    private bool _isDatabaseAvailable;

    private enum MessageType
    {
        Information,
        Success,
        Error
    }

    public MainWindow()
    {
        InitializeComponent();

        ClientListBox.ItemsSource = _clients;

        _isDatabaseAvailable = TryLoadApplicationData();

        UpdateDatabaseControls();
    }

    private void ShowMessage(string message, MessageType messageType)
    {
        MessageTextBlock.Text = message;

        MessageTextBlock.Foreground = messageType switch
        {
            MessageType.Success => Brushes.LightGreen,
            MessageType.Error => Brushes.LightCoral,
            _ => Brushes.LightSkyBlue
        };
    }

    private void NewClient_Click(object? sender, RoutedEventArgs e)
    {
        ClientListBox.SelectedItem = null;
        UpdateActionButtons();

        NomTextBox.Text = "";
        EmailTextBox.Text = "";

        ShowMessage("Tu peux saisir un nouveau client.", MessageType.Information);

        NomTextBox.Focus();
    }

    private void AddClient_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

        string nom = NomTextBox.Text ?? "";
        string email = EmailTextBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("Le nom et l'email sont obligatoires.", MessageType.Error);
            return;
        }

        nom = nom.Trim();
        email = email.Trim();

        if (nom.Length > ClientRules.MaxNameLength)
        {
            ShowMessage($"Le nom ne peut pas dépasser {ClientRules.MaxNameLength} caractères.", MessageType.Error);
            return;
        }

        if (email.Length > ClientRules.MaxEmailLength)
        {
            ShowMessage($"L'adresse email ne peux pas dépasser {ClientRules.MaxEmailLength} caractères.", MessageType.Error);
            return;
        }

        if (!EmailValidator.IsValid(email))
        {
            ShowMessage("L'adresse email n'est pas valide", MessageType.Error);

            return;
        }


        Client client = new Client
        {
            Nom = nom,
            Email = email
        };

        try
        {
            if (_clientRepository.EmailExists(email))
            {
                ShowMessage("Un client possède déjà cette adresse email.", MessageType.Error);

                return;
            }

            client.Id = _clientRepository.Add(client);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19 && ex.SqliteExtendedErrorCode == 2067)
        {
            ShowMessage("Impossible d'ajouter ce client : cette adresse email existe déjà.", MessageType.Error);

            return;
        }
        catch (SqliteException ex)
        {
            AppLogger.LogError("Ajout d'un client - vérification ou insertion dans la base de données", ex);

            ShowMessage("Une erreur de base de données est survenue pendant l'ajout.", MessageType.Error);

            return;
        }

        _clients.Add(client);
        UpdateClientCount();

        NomTextBox.Text = "";
        EmailTextBox.Text = "";

       ShowMessage($"Client enregistré : Id : {client.Id} | Nom : {nom} | Email : {email}", MessageType.Success);
    }

    private void ClientListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateActionButtons();

        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            return;
        }


        NomTextBox.Text = selectedClient.Nom;
        EmailTextBox.Text = selectedClient.Email;

        ShowMessage($"Client sélectionné : Id {selectedClient.Id} | Nom : {selectedClient.Nom} | Email : {selectedClient.Email}", MessageType.Information);
    }

    private void UpdateActionButtons()
    {
        bool canEditSelectedClient = _isDatabaseAvailable && ClientListBox.SelectedItem is Client;

        UpdateButton.IsEnabled = canEditSelectedClient;
        DeleteButton.IsEnabled = canEditSelectedClient;
    }

    private void UpdateClient_Click(object? sender, RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            ShowMessage("Sélectionne un client à modifier.", MessageType.Information);
            return;
        }

        string nom = NomTextBox.Text ?? "";
        string email = EmailTextBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("Le nom et l'email sont obligatoires.", MessageType.Error);
            return;
        }

        nom = nom.Trim();
        email = email.Trim();

        if (nom.Length > ClientRules.MaxNameLength)
        {
            ShowMessage($"Le nom ne peut dépasser {ClientRules.MaxNameLength} caractères.", MessageType.Error);
            return;
        }

        if (email.Length > ClientRules.MaxEmailLength)
        {
            ShowMessage($"L'adresse email ne peut pas dépasser {ClientRules.MaxEmailLength} caractères.", MessageType.Error);
            return;
        }

        if (!EmailValidator.IsValid(email))
        {
            ShowMessage("L'adresse email n'est pas valide.", MessageType.Error);
                return;
        }

        if (_clientRepository.EmailExistsForAnotherClient(email, selectedClient.Id))
        {
            ShowMessage("Un autre client possède déjà cette adresse email", MessageType.Error);

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
            ShowMessage("Impossible de modifier ce client : cette adresse mail existe déjà.", MessageType.Error);

            return;
        }
        catch (SqliteException)
        {
            ShowMessage("Une erreur de base de données est survenue pendant la modification.", MessageType.Error);

            return;
        }

        if (!isUpdated)
        {
            ShowMessage(" Le client n'a pas été trouvé dans la base.", MessageType.Error);
            return;
        }

        int SelectedIndex = ClientListBox.SelectedIndex;

        _clients[SelectedIndex] = updateClient;

        ClientListBox.SelectedItem = updateClient;

        ShowMessage($"Client modifié : Id {updateClient.Id} | Nom : {updateClient.Nom} | Email : {updateClient.Email}", MessageType.Information);
    }

    private async void DeleteClient_Click(object? sender, RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

        if (ClientListBox.SelectedItem is not Client selectedClient)
        {
            ShowMessage("Sélectionne un client à supprimer.", MessageType.Information);
            return;
        }

        DeleteConfirmationWindow confirmationWindow = new DeleteConfirmationWindow(selectedClient);

        bool isConfirmed = await confirmationWindow.ShowDialog<bool>(this);

        if (!isConfirmed)
        {
            ShowMessage("Suppression annulée.", MessageType.Information);
            return;
        }

        bool isDeleted;
        try
        {
            isDeleted = _clientRepository.Delete(selectedClient.Id);
        }
        catch (SqliteException)
        {
            ShowMessage("Une erreur de base de données est survenue pendant la suppression.", MessageType.Error);

            return;
        }

        if (!isDeleted)
        {
            ShowMessage("Le client n'a pas été trouvé dans la base.", MessageType.Error);
            return;
        }

        _clients.Remove(selectedClient);
        UpdateClientCount();

        NomTextBox.Text ="";
        EmailTextBox.Text ="";

        ShowMessage($"Client supprimé : Id {selectedClient.Id} | Nom {selectedClient.Nom}.", MessageType.Information);
    }

    private void SearchClient_Click(object? sender, RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

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

        UpdateClientCount();
        ShowMessage($"{searchResults.Count} client(s) trouvé(s).", MessageType.Information);
    }

   private async void ExportClients_Click( object? sender, RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Exporter les clients",
                SuggestedFileName = "clients.csv"
            }
        );

        if (file is null)
        {
            ShowMessage("Export annulé.", MessageType.Information);
            return;
        }

        string? filePath = file.TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            ShowMessage("Impossible de récupérer le chemin du fichier.", MessageType.Error);
            return;
        }


        try
        {
            List<Client> clients = _clientRepository.GetAll();

            CsvService.ExportClients(filePath, clients);

            ShowMessage($"{clients.Count} client(s) exporté(s) dans {file.Name}.", MessageType.Information);
        }
        catch (UnauthorizedAccessException ex)
        {
            AppLogger.LogError("Export CSV - accès  refusé au fichier", ex);

            ShowMessage("L'accès au fichier a été refusé. Vérifie ses permissions.", MessageType.Error);

            return;
        }
        catch (IOException ex)
        {
            AppLogger.LogError("Export CSV - écriture dans le fichier", ex);

            ShowMessage("Impossible d'écrire dans le fichier. Il est peut-être déjà ouvert.", MessageType.Error);
        }
        catch (SqliteException ex)
        {
            AppLogger.LogError("Export CSV - lecture des clients dans la base de données", ex);

            ShowMessage("Une erreur de base de données est survenue pendant l'export", MessageType.Error);
        }


    }

    private async void ImportClients_Click(object? sender, RoutedEventArgs e)
    {
        if (!EnsureDatabaseAvailable())
        {
            return;
        }

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
            ShowMessage("Import annulé.", MessageType.Information);
            return;
        }

        string? filePath = files[0].TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            ShowMessage("Impossible de récupérer le chemin du fichier.", MessageType.Error);
            return;
        }

        try
        {
            List<Client> csvClients = CsvService.ImportClients(filePath);

            int importedCount = 0;
            int ignoredCount = 0;
            int invalidCount = 0;
            int tooLongCount = 0;

            List<Client> clientsToImport = new();

            foreach (Client client in csvClients)
            {
                client.Nom = client.Nom.Trim();
                client.Email = client.Email.Trim();

                if (client.Nom.Length > ClientRules.MaxNameLength || client.Email.Length > ClientRules.MaxEmailLength)
                {
                    tooLongCount++;
                    continue;
                }

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

                clientsToImport.Add(client);
            }

            if (clientsToImport.Count > 0)
            {
                importedCount = _clientRepository.AddMany(clientsToImport);

                ignoredCount += clientsToImport.Count - importedCount;
            }

            ClientListBox.SelectedItem = null;
            SearchTextBox.Text = "";

            _clients.Clear();

            List<Client> allClients = _clientRepository.GetAll();

            foreach (Client client in allClients)
            {
                _clients.Add(client);
            }
            UpdateClientCount();

            ShowMessage( 
                $"Import terminé : {importedCount} client(s) ajouté(s), " +
                $"{ignoredCount} doublon(s) ignoré(s), " +
                $"{invalidCount} adresse(s) invalide(s) ignorée(s) "+
                $"{tooLongCount} ligne(s) trop longue(s) ignorée(s).", MessageType.Information);
        }
        catch (InvalidOperationException ex)
        {
            AppLogger.LogError("Import CSV - validation ou lecture du contenu", ex);

            ShowMessage(ex.Message, MessageType.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            AppLogger.LogError("Import CSV - accès refusé au fichier", ex);

            ShowMessage("L'accès au fichier a été refusé. Vérifie ses permissions.", MessageType.Error);
        }
        catch (IOException ex)
        {
            AppLogger.LogError("Import CSV - lecture du fichier", ex);

            ShowMessage("Impossible de lire le fichier. Il est peut-être déjà utilisé.", MessageType.Error);
        }
        catch (SqliteException ex)
        {
            AppLogger.LogError("Import CSV - écriture dans la base de données", ex);

            ShowMessage("Une erreur de base de données est survenue pendant l'import.", MessageType.Error);
        }
    }

    private void UpdateClientCount()
    {
        ClientCountTextBlock.Text = $"{_clients.Count} client(s) affiché(s).";
    }

    private bool TryLoadApplicationData()
    {
        try
        {
            using var connection = Database.OpenConnection();

            Database.Initialize(connection);

            List<Client> clientsFromDatabase = _clientRepository.GetAll();

            foreach (Client client in clientsFromDatabase)
            {
                _clients.Add(client);
            }

            UpdateClientCount();

            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            AppLogger.LogError("Accès refusé au dossier de données pendant le démarrage", ex);

            ShowMessage("L'accès au dossier de données a été refusé.", MessageType.Error);
        
            return false;
        }
        catch (IOException ex)
        {
            AppLogger.LogError("Accès aux fichiers de l'application pendant le démarrage", ex);

            ShowMessage("Impossible d'accéder aux fichiers de l'application.", MessageType.Error);

            return false;
        }
        catch (InvalidOperationException ex)
        {
            AppLogger.LogError("Vérification de la version de la base de données", ex);

            ShowMessage(ex.Message, MessageType.Error);

            return false;
        }
        catch (SqliteException ex)
        {
            AppLogger.LogError("Ouverture ou initialisation de la base de données", ex);

            ShowMessage("Impossible d'ouvrir ou d'initialiser la base de données.", MessageType.Error);

            return false;
        }
    }

    private void UpdateDatabaseControls()
    {
        NewButton.IsEnabled = _isDatabaseAvailable;
        AddButton.IsEnabled = _isDatabaseAvailable;
        ExportButton.IsEnabled = _isDatabaseAvailable;
        ImportButton.IsEnabled = _isDatabaseAvailable;
        SearchButton.IsEnabled = _isDatabaseAvailable;

        NomTextBox.IsEnabled = _isDatabaseAvailable;        
        EmailTextBox.IsEnabled = _isDatabaseAvailable;        
        SearchTextBox.IsEnabled = _isDatabaseAvailable;
        ClientListBox.IsEnabled = _isDatabaseAvailable;

        UpdateActionButtons();

    }

    private bool EnsureDatabaseAvailable()
    {
        if (_isDatabaseAvailable)
        {
            return true;
        }

        ShowMessage("La base de données n'est pas disponible. Rétablis son accès puis redémarre l'application.", MessageType.Error);

        return false;
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isDatabaseAvailable)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            return;
        }

        try
        {
            List<Client> allClients = _clientRepository.GetAll();

            ClientListBox.SelectedItem = null;

            _clients.Clear();

            foreach (Client client in allClients)
            {
                _clients.Add(client);
            }

            UpdateClientCount();

            ShowMessage("Tous les clients sont de nouveau affichés.", MessageType.Information);
        }
        catch (SqliteException)
        {
            ShowMessage("Impossible de recharger la liste des clients.", MessageType.Error);
        }
    }

}