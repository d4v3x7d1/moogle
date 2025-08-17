public class DocumentRepository
{
    private readonly string _pendingDocsFile = "pendingDocs.json";
    private readonly string _documentsFolder;

    private Dictionary<string, bool> _pendingDocuments;

    public DocumentRepository(string documentsFolder)
    {
        _documentsFolder = documentsFolder;
        _pendingDocuments = LoadOrUpdatePendingDocuments();
    }

    /// <summary>
    /// Inicializa o actualiza el diccionario de documentos pendientes.
    /// </summary>
    private Dictionary<string, bool> LoadOrUpdatePendingDocuments()
    {
        Dictionary<string, bool> pendingDocs;

        if (!File.Exists(_pendingDocsFile))
        {
            pendingDocs = Directory
                .GetFiles(_documentsFolder)
                .Select(file => Path.GetFileName(file))
                .ToDictionary(name => name, name => false);
            SaveJsonFile(_pendingDocuments, _pendingDocsFile);

        }
        else
        {
            var json = File.ReadAllText(_pendingDocsFile);

            pendingDocs = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, bool>>(json)!;

            // Añadir nuevos documentos que no estén en el diccionario
            var newDocs = Directory
                .GetFiles(_documentsFolder)
                .Select(file => Path.GetFileName(file))
                .Where(name => !pendingDocs.ContainsKey(name));

            foreach (var doc in newDocs)
            {
                pendingDocs[doc] = false;
            }
            SaveJsonFile(_pendingDocuments, _pendingDocsFile);
        }
        return pendingDocs;
    }

    /// <summary>
    /// Marca un documento como procesado y guarda el estado.
    /// </summary>
    public void MarkAsProcessed(string fileName)
    {
        if (!_pendingDocuments.ContainsKey(fileName))
            throw new ArgumentException($"El archivo '{fileName}' no está en el repositorio.");

        _pendingDocuments[fileName] = true;
        SaveJsonFile(_pendingDocuments, _pendingDocsFile);
    }

    /// <summary>
    /// Guarda el diccionario de documentos pendientes en el archivo JSON.
    /// </summary>
    private void SaveJsonFile(Dictionary<string, bool> data, string filePath)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        File.WriteAllText(filePath, json);
    }




    /// <summary>
    /// Iterador que devuelve el siguiente documento pendiente cada vez que se llama.
    /// No devuelve documentos ya procesados.
    /// </summary>
    public IEnumerable<(string Title, string Content)> GetNextDocuments()
    {
        while (true)
        {
            var nextFile = _pendingDocuments.FirstOrDefault(kv => !kv.Value);

            if (string.IsNullOrEmpty(nextFile.Key))
                yield break; // Todos procesados

            var filePath = Path.Combine(_documentsFolder, nextFile.Key);
            var content = File.ReadAllText(filePath);

            yield return (nextFile.Key, content);
        }
    }
}
