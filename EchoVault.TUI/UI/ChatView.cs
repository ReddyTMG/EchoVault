using EchoVault.Application.Interfaces;
using EchoVault.Infrastructure.Data;
using EchoVault.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace EchoVault.TUI.UI;

public static class ChatView
{
  public static async Task RunChat(VaultDbContext db, IEmbeddingService embeddingService, LlamaChatService chatService)
  {
    var query = AnsiConsole.Ask<string>("[bold yellow]Ask your Vault a question:[/]");

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Searching and Thinking...", async ctx =>
        {
          // 1. Retrieval (R)
          var queryVector = await embeddingService.GenerateEmbeddingAsync(query);
          var allChunks = await db.Chunks.Include(c => c.Document).ToListAsync();

          var topResults = allChunks
                  .Select(c => new { Chunk = c, Score = CosineSimilarity(queryVector, c.Embedding) })
                  .OrderByDescending(r => r.Score)
                  .Take(3)
                  .ToList();

          if (!topResults.Any() || topResults.First().Score < 0.2)
          {
            AnsiConsole.MarkupLine("[red]No relevant info found in your PDFs.[/]");
            return;
          }

          // 2. Combine top chunks into one context string
          var context = string.Join("\n\n", topResults.Select(r => r.Chunk.Content));

          // 3. Generation (G)
          var aiResponse = await chatService.AskWithContextAsync(query, context);

          // 4. Display Result
          AnsiConsole.WriteLine();
          AnsiConsole.Write(new Rule("[bold green]EchoVault AI Response[/]") { Justification = Justify.Left });

          // Use Text instead of Markup to avoid errors if AI uses brackets []
          var aiText = new Text(aiResponse);
          var panel = new Panel(aiText)
          {
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1, 2, 1),
            Expand = true
          };
          AnsiConsole.Write(panel);

          // 5. Show Sources
          var table = new Table().AddColumn("Source File").AddColumn("Relevance");
          foreach (var res in topResults)
          {
            table.AddRow(res.Chunk.Document.FileName, $"[green]{res.Score:P0}[/]");
          }

          AnsiConsole.WriteLine();
          AnsiConsole.Write(new Panel(table) { Header = new PanelHeader("Sources"), Border = BoxBorder.Rounded });
        });

    AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
    Console.ReadKey();
  }

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