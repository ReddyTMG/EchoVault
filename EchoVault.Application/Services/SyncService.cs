using EchoVault.Application.Interfaces;
using EchoVault.Core.Entities;
using EchoVault.Core.Enums;
using System.Security.Cryptography;

namespace EchoVault.Application.Services;

public class SyncService
{
    private readonly IPdfIngestionService _ingestionService;

    public SyncService(IPdfIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    /// <summary>
    /// Synchronizes a physical folder with the database.
    /// Returns a list of documents that need to be processed.
    /// </summary>
    public async Task<List<Document>> IdentifyChangesAsync(WatchFolder folder)
    {
        var discoveredDocuments = new List<Document>();
        var physicalFiles = Directory.GetFiles(folder.Path, "*.pdf");

        foreach (var filePath in physicalFiles)
        {
            var fileInfo = new FileInfo(filePath);
            var hash = ComputeHash(filePath);

            // Check if this file is already in our DB and hasn't changed
            var existingDoc = folder.Documents.FirstOrDefault(d => d.FilePath == filePath);

            if (existingDoc == null || existingDoc.FileHash != hash)
            {
                discoveredDocuments.Add(new Document
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileHash = hash,
                    LastModified = fileInfo.LastWriteTime,
                    Status = ProcessingStatus.Pending,
                    WatchFolderId = folder.Id
                });
            }
        }

        return discoveredDocuments;
    }

    public async Task ProcessDocumentAsync(Document doc, Guid userId)
    {
        doc.Status = ProcessingStatus.Processing;
        
        try 
        {
            var chunks = await _ingestionService.ProcessDocumentAsync(doc.FilePath, userId);
            doc.Chunks = chunks;
            doc.Status = ProcessingStatus.Indexed;
        }
        catch (Exception)
        {
            doc.Status = ProcessingStatus.Error;
            throw;
        }
    }

    private string ComputeHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}