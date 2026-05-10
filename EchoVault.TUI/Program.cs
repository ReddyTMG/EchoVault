using EchoVault.Infrastructure.Data;
using EchoVault.Infrastructure.Services;
using EchoVault.Application.Services;
using EchoVault.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var services = new ServiceCollection();

// 1. Setup Database
services.AddDbContext<VaultDbContext>(options =>
    options.UseSqlite("Data Source=vault.db"));

// 2. Setup AI Services (Hardcoded for now, will be dynamic in TUI)
string myModelPath = @"C:/Models/all-MiniLM-L6-v2.gguf"; 
services.AddSingleton<IEmbeddingService>(sp => new LocalEmbeddingService(myModelPath));

// 3. Setup Ingestion & Sync
services.AddScoped<IPdfIngestionService, PdfIngestionService>();
services.AddScoped<SyncService>();

var serviceProvider = services.BuildServiceProvider();

// Now you can "Ask" for the SyncService and it will 
// automatically have the IngestionService and EmbeddingService injected!
var syncService = serviceProvider.GetRequiredService<SyncService>();