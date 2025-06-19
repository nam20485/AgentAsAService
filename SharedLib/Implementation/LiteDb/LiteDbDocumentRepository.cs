using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;
using SharedLib.Abstractions;

namespace SharedLib.Implementation.LiteDb;

/// <summary>
/// LiteDB implementation of the document repository
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class LiteDbDocumentRepository<T> : IDocumentRepository<T> where T : class
{
    private readonly string _connectionString;
    private readonly string _collectionName;

    public LiteDbDocumentRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _collectionName = GetCollectionName();

        // make sure the directory where the DB file will be created exists
        var directoryName = Path.GetDirectoryName(_connectionString);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }

    public Task<T> UpsertAsync(string id, T document)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(id));
        ArgumentNullException.ThrowIfNull(document);

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>(_collectionName);
        
        // Set the ID property if it exists
        SetIdProperty(document, id);
        
        collection.Upsert(document);
        
        return Task.FromResult(document);
    }

    public Task<T?> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return Task.FromResult<T?>(null);

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>(_collectionName);
        
        var result = collection.FindById(new BsonValue(id));
        return Task.FromResult<T?>(result);
    }

    public Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>(_collectionName);
        
        var results = collection.Find(predicate).ToList();
        return Task.FromResult<IEnumerable<T>>(results);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>(_collectionName);
        
        var results = collection.FindAll().ToList();
        return Task.FromResult<IEnumerable<T>>(results);
    }

    public Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return Task.FromResult(false);

        try
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<T>(_collectionName);
            
            var result = collection.Delete(new BsonValue(id));
            return Task.FromResult(result);
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

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>(_collectionName);
        
        var exists = collection.Exists(doc => true) && collection.FindById(new BsonValue(id)) != null;
        return Task.FromResult(exists);
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
