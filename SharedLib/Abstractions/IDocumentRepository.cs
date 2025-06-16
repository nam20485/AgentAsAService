using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SharedLib.Abstractions;

/// <summary>
/// Generic document repository interface for NoSQL document stores.
/// Provides basic CRUD operations that can be implemented by various NoSQL providers.
/// </summary>
/// <typeparam name="T">The document type</typeparam>
public interface IDocumentRepository<T> where T : class
{
    /// <summary>
    /// Insert or update a document with the specified ID
    /// </summary>
    /// <param name="id">Document identifier</param>
    /// <param name="document">Document to store</param>
    /// <returns>The stored document</returns>
    Task<T> UpsertAsync(string id, T document);

    /// <summary>
    /// Get a document by its ID
    /// </summary>
    /// <param name="id">Document identifier</param>
    /// <returns>The document if found, null otherwise</returns>
    Task<T?> GetAsync(string id);

    /// <summary>
    /// Query documents using a predicate expression
    /// </summary>
    /// <param name="predicate">Filter condition</param>
    /// <returns>Collection of matching documents</returns>
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Get all documents in the collection
    /// </summary>
    /// <returns>All documents</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Delete a document by its ID
    /// </summary>
    /// <param name="id">Document identifier</param>
    /// <returns>True if the document was deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Check if a document exists with the specified ID
    /// </summary>
    /// <param name="id">Document identifier</param>
    /// <returns>True if the document exists</returns>
    Task<bool> ExistsAsync(string id);
}
