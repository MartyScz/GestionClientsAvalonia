using System;
using System.Net.Mail;

namespace GestionClientsAvalonia;

public static class EmailValidator
{
    public static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        string cleanedEmail = email.Trim();

        try
        {
            MailAddress address = new MailAddress(cleanedEmail);

            string host = address.Host;

            return address.Address == cleanedEmail
                && host.Contains('.')
                && !host.StartsWith('.')
                && !host.EndsWith('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }
}