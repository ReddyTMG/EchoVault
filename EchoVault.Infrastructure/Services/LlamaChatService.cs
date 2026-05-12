using EchoVault.Application.Interfaces;
using LLama;
using LLama.Common;

namespace EchoVault.Infrastructure.Services;

public class LlamaChatService : IDisposable
{
    private readonly LLamaWeights _weights;
    private readonly StatelessExecutor _executor;

    public LlamaChatService(string modelPath)
    {
        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048, // Large enough for PDF chunks + Question
            GpuLayerCount = 0   // Set to >0 if you have an Nvidia GPU
        };

        _weights = LLamaWeights.LoadFromFile(parameters);
        _executor = new StatelessExecutor(_weights, parameters);
    }

    public async Task<string> AskWithContextAsync(string question, string context)
    {
        // The "System Prompt" defines how the AI behaves
        string prompt = $"""
        <|begin_of_text|><|start_header_id|>system<|end_header_id|>
        You are EchoVault AI, a helpful assistant. Use the provided PDF context to answer the user's question. 
        If the answer isn't in the context, say you don't know.
        
        Context:
        {context}
        <|eot_id|><|start_header_id|>user<|end_header_id|>
        {question}
        <|eot_id|><|start_header_id|>assistant<|end_header_id|>
        """;

        var inferenceParams = new InferenceParams()
        {
            MaxTokens = 256,
            AntiPrompts = new[] { "<|eot_id|>" }
        };

        string response = "";
        await foreach (var text in _executor.InferAsync(prompt, inferenceParams))
        {
            response += text;
        }

        return response.Trim();
    }

    public void Dispose()
    {
        _weights.Dispose();
    }
}