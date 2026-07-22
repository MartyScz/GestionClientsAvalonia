using System;
using System.Collections.Generic;
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

    public static List<Client> ImportClients(string filePath)
    {
        List<Client> clients = new();

        using var reader = new StreamReader(filePath, Encoding.UTF8);

        string? headerLine = reader.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return clients;
        }

        List<string> headers = ParseCsvLine(headerLine);

        int nomIndex = headers.FindIndex(
            header => header.Equals("Nom", StringComparison.OrdinalIgnoreCase)
        );
        int emailIndex = headers.FindIndex(
            header => header.Equals("Email", StringComparison.OrdinalIgnoreCase)
        );

        if (nomIndex == - 1 || emailIndex == -1)
        {
            throw new InvalidOperationException("Le fichier CSV doit contenir les colonnes Nom et Email.");
        }

        string? line;

        while((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            List<string> values = ParseCsvLine(line);

            if (values.Count <= nomIndex || values.Count <= emailIndex)
            {
                continue;
            }

            string nom = values[nomIndex].Trim();
            string email = values[emailIndex].Trim();

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            Client client = new Client
            {
                Nom = nom,
                Email = email
            };

            clients.Add(client);
        }

        return clients;
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

    private static List<string> ParseCsvLine(string line)
    {
        List<string> values = new();
        StringBuilder currentValue = new();

        bool isInsideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char currentChar = line[i];

            if (currentChar == '"')
            {
                if (isInsideQuotes && i + 1 < line.Length && line[ i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    isInsideQuotes = !isInsideQuotes;
                }
            }
            else if (currentChar == ';' && !isInsideQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(currentChar);
            }
        }

        values.Add(currentValue.ToString());

        return values;
    }

}