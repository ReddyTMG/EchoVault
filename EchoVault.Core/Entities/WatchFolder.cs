namespace EchoVault.Core.Entities;

public class WatchFolder
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime LastScannedAt { get; set; }
    
    // Navigation property for EF Core
    public List<Document> Documents { get; set; } = new();
}