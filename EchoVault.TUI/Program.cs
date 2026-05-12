using EchoVault.TUI.UI;
using EchoVault.Application.Services;
using EchoVault.Infrastructure.Data;
using EchoVault.Infrastructure.Services;
using EchoVault.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using DotNetEnv;

string envPath = ".env";

// If .env isn't in the current folder, check the parent
if (!File.Exists(envPath))
{
    envPath = Path.Combine("..", ".env");
}

// 1. Configuration
Env.Load(envPath);
string dbConn = Env.GetString("DB_CONNECTION");
string modelPath = Env.GetString("MODEL_PATH");

if (string.IsNullOrEmpty(modelPath))
{
    AnsiConsole.MarkupLine($"[red]Error:[/] Could not find .env at {envPath}");
    return;
}

// 2. DI Container Setup
var services = new ServiceCollection();
services.AddDbContext<VaultDbContext>(options => options.UseSqlite(dbConn));
services.AddSingleton<IEmbeddingService>(sp => new LocalEmbeddingService(modelPath));
services.AddScoped<IPdfIngestionService, PdfIngestionService>();
services.AddScoped<SyncService>();

var serviceProvider = services.BuildServiceProvider();

// 3. App Loop
AnsiConsole.Write(new FigletText("EchoVault").Color(Color.Cyan1));

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices("🔍 Search", "🔄 Sync", "❌ Exit"));

    if (choice == "❌ Exit") break;

    using var scope = serviceProvider.CreateScope();
    var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();
    var db = scope.ServiceProvider.GetRequiredService<VaultDbContext>();

    if (choice == "🔄 Sync")
    {
        // For the dummy test, we'll grab the first watch folder
        var folder = await db.WatchFolders.FirstOrDefaultAsync();
        var user = await db.Users.FirstOrDefaultAsync();

        if (folder != null && user != null)
        {
            await SyncDisplay.RunSyncWithProgress(syncService, folder, user.Id);

            // Save changes to DB (Important: SyncDisplay processes them, but we persist here)
            await db.SaveChangesAsync();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No WatchFolder found in DB. Please run the Dummy Test script first to seed data.[/]");
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        AnsiConsole.Clear();
    }

    if (choice == "🔍 Search")
    {
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        await SearchView.RunSearch(db, embeddingService);
        AnsiConsole.Clear();
    }
}