using Microsoft.EntityFrameworkCore;
using EchoVault.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

// This is the beginning of your Niche TUI
Console.WriteLine("--- EchoVault: Starting Secure Session ---");

// Setup Dependency Injection
var services = new ServiceCollection();

services.AddDbContext<VaultDbContext>(options =>
    options.UseSqlite("Data Source=vault.db"));

var serviceProvider = services.BuildServiceProvider();

// Ensure Database is Created
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VaultDbContext>();
    db.Database.EnsureCreated();
    Console.WriteLine("Vault Database [vault.db] is ready.");
}