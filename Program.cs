using DocsUnmessed.CLI;
using DocsUnmessed.CLI.Commands;
using DocsUnmessed.Connectors;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Data;
using DocsUnmessed.Data.Interfaces;
using DocsUnmessed.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

// Get connection string
var connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=docsunmessed.db";

// Configure DbContext
var optionsBuilder = new DbContextOptionsBuilder<DocsUnmessedDbContext>();
optionsBuilder.UseSqlite(connectionString);

// Initialize database
var dbContext = new DocsUnmessedDbContext(optionsBuilder.Options);
var dbInit = new DatabaseInitializationService(dbContext);
await dbInit.InitializeAsync(CancellationToken.None);

Console.WriteLine("✓ Database initialized");

// Initialize services
IHashService hashService = new HashService();
IConnector[] connectors = [new FileSystemConnector(hashService)];
IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
IInventoryService inventoryService = new DatabaseInventoryService(unitOfWork);
IMigrationOrchestrator migrationOrchestrator = new MigrationOrchestrator(inventoryService);

// Initialize commands
var assessCommand = new AssessCommand(connectors, inventoryService);
var simulateCommand = new SimulateCommand(migrationOrchestrator, inventoryService);
var migrateCommand = new MigrateCommand(inventoryService);
var validateCommand = new ValidateCommand();

// Initialize router
var router = new CommandRouter(assessCommand, simulateCommand, migrateCommand, validateCommand);

// Route command
var exitCode = await router.RouteAsync(args, CancellationToken.None);

// Cleanup
dbContext.Dispose();
unitOfWork.Dispose();

return exitCode;
