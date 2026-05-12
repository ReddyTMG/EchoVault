// using EchoVault.Infrastructure.Data;
// using EchoVault.Infrastructure.Services;
// using EchoVault.Application.Services;
// using EchoVault.Application.Interfaces;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.EntityFrameworkCore;

// var services = new ServiceCollection();

// // 1. Setup Database
// services.AddDbContext<VaultDbContext>(options =>
//     options.UseSqlite("Data Source=vault.db"));

// // 2. Setup AI Services (Hardcoded for now, will be dynamic in TUI)
// string myModelPath = @"C:/Models/all-MiniLM-L6-v2.gguf"; 
// services.AddSingleton<IEmbeddingService>(sp => new LocalEmbeddingService(myModelPath));

// // 3. Setup Ingestion & Sync
// services.AddScoped<IPdfIngestionService, PdfIngestionService>();
// services.AddScoped<SyncService>();

// var serviceProvider = services.BuildServiceProvider();

// // Now you can "Ask" for the SyncService and it will 
// // automatically have the IngestionService and EmbeddingService injected!
// var syncService = serviceProvider.GetRequiredService<SyncService>();

using EchoVault.Infrastructure.Data;
using EchoVault.Infrastructure.Services;
using EchoVault.Application.Services;
using EchoVault.Application.Interfaces;
using EchoVault.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

Console.WriteLine("=== EchoVault: Dummy Sync Test ===");

// UPDATE THESE PATHS TO YOUR ACTUAL LOCAL PATHS
Env.Load();
string dbConnection = Env.GetString("DB_CONNECTION");
string modelPath = Env.GetString("MODEL_PATH");
string testPdfFolder = Env.GetString("TEST_PDF_FOLDER");
if (string.IsNullOrEmpty(modelPath) || string.IsNullOrEmpty(testPdfFolder))
{
    Console.WriteLine("Error: Please ensure MODEL_PATH and TEST_PDF_FOLDER are set in your .env file.");
    return;
}

// 1. Setup DI
var services = new ServiceCollection();

services.AddDbContext<VaultDbContext>(options =>
    options.UseSqlite("dbConnection"));


services.AddSingleton<IEmbeddingService>(sp => new LocalEmbeddingService(modelPath));
services.AddScoped<IPdfIngestionService, PdfIngestionService>();
services.AddScoped<SyncService>();

var sp = services.BuildServiceProvider();

// 2. Initialize DB and Create Dummy Data
using var scope = sp.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<VaultDbContext>();
db.Database.EnsureCreated();

// Create a dummy user if none exists
var user = await db.Users.FirstOrDefaultAsync() ?? new User { Username = "DevUser" };
if (db.Entry(user).State == EntityState.Detached) db.Users.Add(user);

// Create a watch folder if none exists
var folder = await db.WatchFolders.FirstOrDefaultAsync() ?? new WatchFolder { 
    Path = testPdfFolder, 
    UserId = user.Id 
};
if (db.Entry(folder).State == EntityState.Detached) db.WatchFolders.Add(folder);
await db.SaveChangesAsync();

// 3. Run the Sync Logic
var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();

Console.WriteLine($"Scanning folder: {testPdfFolder}...");
var changes = await syncService.IdentifyChangesAsync(folder);

if (changes.Count == 0)
{
    Console.WriteLine("No new or changed PDFs found.");
}
else
{
    Console.WriteLine($"Found {changes.Count} new/changed files. Processing...");
    foreach (var doc in changes)
    {
        Console.WriteLine($"-> Processing {doc.FileName}...");
        await syncService.ProcessDocumentAsync(doc, user.Id);
        
        // Save the indexed document and its chunks to the DB
        db.Documents.Add(doc);
        await db.SaveChangesAsync();
        Console.WriteLine($"   Success! {doc.Chunks.Count} chunks stored with embeddings.");
    }
}

Console.WriteLine("=== Test Complete ===");