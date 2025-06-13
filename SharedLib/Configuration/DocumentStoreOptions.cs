using System;

namespace SharedLib.Configuration;

/// <summary>
/// Configuration options for document store providers
/// </summary>
public class DocumentStoreOptions
{
    public const string SectionName = "DocumentStore";
    
    /// <summary>
    /// The document store provider to use (Firestore, LiteDb, JsonFile)
    /// </summary>
    public string Provider { get; set; } = "LiteDb";
    
    /// <summary>
    /// Connection string or configuration for the provider
    /// - Firestore: Google Cloud Project ID
    /// - LiteDb: Database file path
    /// - JsonFile: Data directory path
    /// </summary>
    public string ConnectionString { get; set; } = "data.db";
    
    /// <summary>
    /// Google Cloud Project ID (for Firestore provider)
    /// </summary>
    public string? ProjectId { get; set; }
    
    /// <summary>
    /// Data directory path (for JsonFile provider)
    /// </summary>
    public string? DataDirectory { get; set; }
}

/// <summary>
/// Supported document store provider types
/// </summary>
public static class DocumentStoreProviders
{
    public const string Firestore = "Firestore";
    public const string LiteDb = "LiteDb";
    public const string JsonFile = "JsonFile";
}
