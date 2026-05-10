using EchoVault.Application.Interfaces;
using EchoVault.Core.Entities;
using UglyToad.PdfPig;
using Microsoft.SemanticKernel.Text;

namespace EchoVault.Infrastructure.Services;

public class PdfIngestionService : IPdfIngestionService
{
    private readonly IEmbeddingService _embeddingService;

    public PdfIngestionService(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    public async Task<List<DocumentChunk>> ProcessDocumentAsync(string filePath, Guid userId)
    {
        var chunks = new List<DocumentChunk>();
        
        using var pdf = PdfDocument.Open(filePath);
        var fullText = string.Join(" ", pdf.GetPages().Select(p => p.Text));

        #pragma warning disable SKEXP0050
        var lines = TextChunker.SplitPlainTextLines(fullText, 100);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 500);
        #pragma warning restore SKEXP0050

        foreach (var para in paragraphs)
        {
            // NEW: Generate the real vector
            var vector = await _embeddingService.GenerateEmbeddingAsync(para);

            chunks.Add(new DocumentChunk
            {
                Content = para,
                Embedding = vector 
            });
        }

        return chunks;
    }
}