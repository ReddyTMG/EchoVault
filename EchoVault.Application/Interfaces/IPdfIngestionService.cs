using EchoVault.Core.Entities;

namespace EchoVault.Application.Interfaces;

public interface IPdfIngestionService
{
    /// <summary>
    /// Processes a local PDF file, chunks it, and generates embeddings.
    /// </summary>
    Task<List<DocumentChunk>> ProcessDocumentAsync(string filePath, Guid userId);
}