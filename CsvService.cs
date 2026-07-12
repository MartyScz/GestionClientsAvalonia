using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Text;

namespace GestionClientsAvalonia;

public static class CsvService
{
    public static void ExportClients(string filePath, IEnumerable<Client> clients)
    {
        using var writer = new StreamWriter(
            filePath,
            false,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
        );

        writer.WriteLine("Id;Nom;Email");

        foreach (Client client in clients)
        {
            writer.WriteLine( $"{client.Id};{EscapeCsv(client.Nom)};{EscapeCsv(client.Email)}");
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
        {
            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }

        return value;
    }
}