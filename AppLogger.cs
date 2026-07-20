using System;
using System.IO;
using System.Text;

namespace GestionClientsAvalonia;

public static class AppLogger
{
    private static readonly object SyncLock = new();

    private static readonly string LogsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionClientsAvalonia", "Logs");

    private static readonly string LogFilePath = Path.Combine(LogsFolder, "application.log");

    public static void LogError(string context, Exception exception)
    {
        try
        {
            string logEntry = 
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR - {context}" +
                Environment.NewLine +
                exception +
                Environment.NewLine +
                new string('-', 80) +
                Environment.NewLine;

            lock (SyncLock)
            {
                Directory.CreateDirectory(LogsFolder);

                File.AppendAllText(
                    LogFilePath,
                    logEntry,
                    Encoding.UTF8
                );
            }
        }
        catch
        {
            /*Une erreur d’écriture du journal ne doit jamais
            provoquer la fermeture de l’application.*/
        }
    }
}