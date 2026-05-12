namespace EchoVault.Application.Interfaces;

public interface IEmbeddingService
{
    /// <summary>
    /// Converts a string chunk into a high-dimensional vector.
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text);

    // Helper to calculate similarity between two vectors
    public static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        float dotProduct = 0;
        float l2Norm1 = 0;
        float l2Norm2 = 0;
        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            l2Norm1 += vector1[i] * vector1[i];
            l2Norm2 += vector1[i] * vector2[i];
        }
        return dotProduct / (MathF.Sqrt(l2Norm1) * MathF.Sqrt(l2Norm2));
    }
}

