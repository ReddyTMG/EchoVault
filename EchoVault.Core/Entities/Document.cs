using EchoVault.Core.Enums;

namespace EchoVault.Core.Entities;

public class Document
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty; 
    public DateTime LastModified { get; set; }
    public ProcessingStatus Status { get; set; }
    
    public int WatchFolderId { get; set; }
    public WatchFolder WatchFolder { get; set; } = null!;
    public List<DocumentChunk> Chunks { get; set; } = new();
}