using EchoVault.Application.Interfaces;
using LLama;
using LLama.Common;

namespace EchoVault.Infrastructure.Services;

public class LocalEmbeddingService : IEmbeddingService, IDisposable
{
  private readonly LLamaWeights _weights;
  private readonly LLamaEmbedder _embedder;

  public LocalEmbeddingService(string modelPath)
  {
    // 1. Load the model weights
    var parameters = new ModelParams(modelPath)
    {
      Embeddings = true // CRITICAL: Tells LLamaSharp to act as an embedder, not a talker
    };
    _weights = LLamaWeights.LoadFromFile(parameters);

    // 2. Initialize the embedder
    _embedder = new LLamaEmbedder(_weights, parameters);
  }

  public async Task<float[]> GenerateEmbeddingAsync(string text)
  {
    // GetEmbeddings returns IReadOnlyList<float[]>. 
    // Since we pass one string, we want the first [0] element.
    var embeddings = await _embedder.GetEmbeddings(text);
    return embeddings[0];
  }

  public void Dispose()
  {
    _embedder.Dispose();
    _weights.Dispose();
  }
}