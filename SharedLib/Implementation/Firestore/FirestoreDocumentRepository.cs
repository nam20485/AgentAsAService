using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using SharedLib.Abstractions;

namespace SharedLib.Implementation.Firestore;

/// <summary>
/// Firestore implementation of the document repository
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class FirestoreDocumentRepository<T> : IDocumentRepository<T> where T : class
{
    private readonly FirestoreDb _firestoreDb;
    private readonly string _collectionName;

    public FirestoreDocumentRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb ?? throw new ArgumentNullException(nameof(firestoreDb));
        _collectionName = GetCollectionName();
    }

    public async Task<T> UpsertAsync(string id, T document)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(id));
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var collection = _firestoreDb.Collection(_collectionName);
        await collection.Document(id).SetAsync(document);
        
        return document;
    }

    public async Task<T?> GetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        var collection = _firestoreDb.Collection(_collectionName);
        var snapshot = await collection.Document(id).GetSnapshotAsync();
        
        return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
    {
        // Note: Firestore has limited query capabilities compared to LINQ expressions
        // For complex queries, you might need to fetch all documents and filter in memory
        // or implement specific query methods for common patterns
        
        var collection = _firestoreDb.Collection(_collectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        var documents = snapshot.Documents
            .Where(doc => doc.Exists)
            .Select(doc => doc.ConvertTo<T>())
            .ToList();

        // Apply the predicate in memory (not ideal for large datasets)
        var compiledPredicate = predicate.Compile();
        return documents.Where(compiledPredicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var collection = _firestoreDb.Collection(_collectionName);
        var snapshot = await collection.GetSnapshotAsync();
        
        return snapshot.Documents
            .Where(doc => doc.Exists)
            .Select(doc => doc.ConvertTo<T>())
            .ToList();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        try
        {
            var collection = _firestoreDb.Collection(_collectionName);
            await collection.Document(id).DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        var collection = _firestoreDb.Collection(_collectionName);
        var snapshot = await collection.Document(id).GetSnapshotAsync();
        
        return snapshot.Exists;
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
}
