using EchoVault.Application.Services;
using EchoVault.Core.Entities;
using Spectre.Console;

namespace EchoVault.TUI.UI;

public static class SyncDisplay
{
    public static async Task RunSyncWithProgress(SyncService syncService, WatchFolder folder, Guid userId)
    {
        AnsiConsole.MarkupLine("[yellow]Initial scan started...[/]");
        var changes = await syncService.IdentifyChangesAsync(folder);

        if (changes.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Everything is up to date![/]");
            return;
        }

        // Create a Progress bar display
        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[] 
            {
                new TaskDescriptionColumn(),    // File name
                new ProgressBarColumn(),        // The bar
                new PercentageColumn(),         // 50%
                new SpinnerColumn(),            // Loading animation
            })
            .StartAsync(async ctx =>
            {
                // Add a task for the overall progress
                var totalTask = ctx.AddTask($"[green]Syncing {changes.Count} files[/]");
                var increment = 100.0 / changes.Count;

                foreach (var doc in changes)
                {
                    totalTask.Description = $"[cyan]Processing:[/] {doc.FileName}";
                    
                    try 
                    {
                        await syncService.ProcessDocumentAsync(doc, userId);
                        // In a real app, you'd call your DB context here to save
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error processing {doc.FileName}:[/] {ex.Message}");
                    }

                    totalTask.Increment(increment);
                }
                
                totalTask.Value = 100;
                totalTask.Description = "[bold green]Sync Complete![/]";
            });
    }
}