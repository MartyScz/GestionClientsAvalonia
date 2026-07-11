using Avalonia.Controls;
using Avalonia.Controls.Selection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;

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

        Client client = new Client
        {
            Nom = nom,
            Email = email
        };

        client.Id = _clientRepository.Add(client);
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
}