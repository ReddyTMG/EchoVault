namespace EchoVault.Core.Entities;

public class DocumentChunk
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;

    // Vector data for sqlite-vec
    // float[] is the standard way to represent embeddings in C#.
    // We will configure EF Core later to map this to a BLOB for sqlite-vec.
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public Document Document { get; set; } = null!;
}