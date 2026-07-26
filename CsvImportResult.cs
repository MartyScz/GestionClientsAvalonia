
using System.Collections.Generic;

namespace GestionClientsAvalonia;

public class CsvImportResult
{
    public List<Client> Clients { get;  } = new();

    public int MalformedLineCount {get; set; }
}