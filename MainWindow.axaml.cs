using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace GestionClientsAvalonia;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Client> _clients = new();
    public MainWindow()
    {
        InitializeComponent();
        ClientListBox.ItemsSource = _clients;
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

        _clients.Add(client);

        NomTextBox.Text = "";
        EmailTextBox.Text = "";

        MessageTextBlock.Text = $"Client ajouté : Nom : {nom} | Email : {email}";
    }
}