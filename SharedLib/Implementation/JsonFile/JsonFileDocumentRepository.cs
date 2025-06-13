using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using SharedLib.Abstractions;

namespace SharedLib.Implementation.JsonFile;

/// <summary>
/// JSON file-based implementation of the document repository.
/// Stores documents as individual JSON files in a directory structure.
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class JsonFileDocumentRepository<T> : IDocumentRepository<T> where T : class
{
    private readonly string _dataDirectory;
    private readonly string _collectionPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonFileDocumentRepository(string dataDirectory)
    {
        _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        _collectionPath = Path.Combine(_dataDirectory, GetCollectionName());
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Ensure collection directory exists
        Directory.CreateDirectory(_collectionPath);
    }

    public async Task<T> UpsertAsync(string id, T document)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(id));
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        // Set the ID property if it exists
        SetIdProperty(document, id);

        var filePath = GetDocumentPath(id);
        var json = JsonSerializer.Serialize(document, _jsonOptions);
        
        await File.WriteAllTextAsync(filePath, json);
        
        return document;
    }

    public async Task<T?> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        var filePath = GetDocumentPath(id);
        
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
    {
        var allDocuments = await GetAllAsync();
        var compiledPredicate = predicate.Compile();
        
        return allDocuments.Where(compiledPredicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        if (!Directory.Exists(_collectionPath))
            return Enumerable.Empty<T>();

        var documents = new List<T>();
        var jsonFiles = Directory.GetFiles(_collectionPath, "*.json");

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var document = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                
                if (document != null)
                    documents.Add(document);
            }
            catch
            {
                // Skip corrupted files
                continue;
            }
        }

        return documents;
    }

    public Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return Task.FromResult(false);

        var filePath = GetDocumentPath(id);
        
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        try
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return Task.FromResult(false);

        var filePath = GetDocumentPath(id);
        return Task.FromResult(File.Exists(filePath));
    }

    private string GetDocumentPath(string id)
    {
        // Sanitize the ID to be file-system safe
        var safeId = string.Join("_", id.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_collectionPath, $"{safeId}.json");
    }

    private static string GetCollectionName()
    {
        // Use the type name as collection name, but pluralized and lowercase
        var typeName = typeof(T).Name.ToLower();
        
        // Simple pluralization (can be improved with a proper pluralization library)
        if (typeName.EndsWith("y"))
            return typeName.Substring(0, typeName.Length - 1) + "ies";
        if (typeName.EndsWith("s") || typeName.EndsWith("x") || typeName.EndsWith("z") || 
            typeName.EndsWith("ch") || typeName.EndsWith("sh"))
            return typeName + "es";
        
        return typeName + "s";
    }

    private static void SetIdProperty(T document, string id)
    {
        // Try to set the Id property if it exists
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite && idProperty.PropertyType == typeof(string))
        {
            idProperty.SetValue(document, id);
        }
    }
}
