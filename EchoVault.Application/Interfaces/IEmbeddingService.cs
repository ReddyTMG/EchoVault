namespace EchoVault.Application.Interfaces;

public interface IEmbeddingService
{
    /// <summary>
    /// Converts a string chunk into a high-dimensional vector.
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text);
}