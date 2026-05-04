using EchoVault.Core.Enums;

namespace EchoVault.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Configuration for the RAG brain
    public LLMProvider PreferredProvider { get; set; }
    public string? LocalModelPath { get; set; } 
    public string? EncryptedApiKey { get; set; }
}