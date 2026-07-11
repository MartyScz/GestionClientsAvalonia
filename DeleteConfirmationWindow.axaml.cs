using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GestionClientsAvalonia
{
    public partial class DeleteConfirmationWindow : Window
    {
        public DeleteConfirmationWindow()
        {
            InitializeComponent();
        }

        public DeleteConfirmationWindow(Client client) : this()
        {
            ClientInfoTextBlock.Text = $"Id {client.Id} | Nom : {client.Nom} | Email : {client.Email}";
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void Confirm_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }
    }
}