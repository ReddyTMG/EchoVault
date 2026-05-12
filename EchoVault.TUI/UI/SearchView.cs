using EchoVault.Application.Interfaces;
using EchoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace EchoVault.TUI.UI;

public static class SearchView
{
  public static async Task RunSearch(VaultDbContext db, IEmbeddingService embeddingService)
  {
    var query = AnsiConsole.Ask<string>("[yellow]Search your vault:[/]");

    await AnsiConsole.Status()
        .Start("Thinking...", async ctx =>
        {
          // 1. Vectorize the query
          var queryVector = await embeddingService.GenerateEmbeddingAsync(query);

          // 2. Fetch all chunks (In a large app, we'd use a Vector DB, but for a portfolio, this works)
          var allChunks = await db.Chunks
                  .Include(c => c.Document)
                  .ToListAsync();

          // 3. Rank by similarity
          var results = allChunks
                  .Select(chunk => new
                {
                  Chunk = chunk,
                  Score = CosineSimilarity(queryVector, chunk.Embedding)
                })
                  .OrderByDescending(r => r.Score)
                  .Take(3)
                  .ToList();

          // 4. Display Results
          AnsiConsole.WriteLine();
          if (!results.Any() || results.First().Score < 0.3)
          {
            AnsiConsole.MarkupLine("[red]No relevant information found.[/]");
            return;
          }

          foreach (var res in results)
          {
            // Create a header text
            var headerText = $"[bold cyan]Source:[/] {res.Chunk.Document.FileName} | [bold yellow]Relevance:[/] {res.Score:P0}";

            var panel = new Panel(res.Chunk.Content)
            {
              Header = new PanelHeader(headerText),
              Border = BoxBorder.Rounded,
              Padding = new Padding(2, 0, 2, 0), // Add some breathing room
              Expand = true
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine(); // Gap between results
          }
        });

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
  }

  // Manual Cosine Similarity since we're using a standard DB
  private static float CosineSimilarity(float[] V1, float[] V2)
  {
    float dot = 0.0f, mag1 = 0.0f, mag2 = 0.0f;
    for (int i = 0; i < V1.Length; i++)
    {
      dot += V1[i] * V2[i];
      mag1 += MathF.Pow(V1[i], 2);
      mag2 += MathF.Pow(V2[i], 2);
    }
    return dot / (MathF.Sqrt(mag1) * MathF.Sqrt(mag2));
  }
}