namespace DocsUnmessed.CLI;

using DocsUnmessed.CLI.Commands;

/// <summary>
/// Routes CLI commands to appropriate handlers
/// </summary>
public sealed class CommandRouter
{
    private readonly AssessCommand _assessCommand;
    private readonly SimulateCommand _simulateCommand;
    private readonly MigrateCommand _migrateCommand;
    private readonly ValidateCommand _validateCommand;

    public CommandRouter(AssessCommand assessCommand, SimulateCommand simulateCommand, MigrateCommand migrateCommand, ValidateCommand validateCommand)
    {
        _assessCommand = assessCommand;
        _simulateCommand = simulateCommand;
        _migrateCommand = migrateCommand;
        _validateCommand = validateCommand;
    }

    public async Task<int> RouteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "assess" => await HandleAssessAsync(args[1..], cancellationToken),
            "simulate" => await HandleSimulateAsync(args[1..], cancellationToken),
            "migrate" => await HandleMigrateAsync(args[1..], cancellationToken),
            "validate" => await HandleValidateAsync(args[1..], cancellationToken),
            "help" or "--help" or "-h" => ShowHelp(),
            "version" or "--version" or "-v" => ShowVersion(),
            _ => ShowUnknownCommand(command)
        };
    }

    private async Task<int> HandleAssessAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = ParseAssessOptions(args);
        return await _assessCommand.ExecuteAsync(options, cancellationToken);
    }

    private async Task<int> HandleSimulateAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = ParseSimulateOptions(args);
        return await _simulateCommand.ExecuteAsync(options, cancellationToken);
    }

    private async Task<int> HandleMigrateAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = ParseMigrateOptions(args);
        return await _migrateCommand.ExecuteAsync(options, cancellationToken);
    }

    private async Task<int> HandleValidateAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = ParseValidateOptions(args);
        return await _validateCommand.ExecuteAsync(options, cancellationToken);
    }

    private AssessOptions ParseAssessOptions(string[] args)
    {
        var providers = new List<string>();
        string? root = null;
        string? output = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--providers" when i + 1 < args.Length:
                    providers.AddRange(args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries));
                    break;
                case "--root" when i + 1 < args.Length:
                    root = args[++i];
                    break;
                case "--out" when i + 1 < args.Length:
                    output = args[++i];
                    break;
            }
        }

        if (providers.Count == 0)
        {
            providers.Add("fs_local"); // Default to local file system
        }

        // Default to user's Documents folder if no root specified
        if (string.IsNullOrEmpty(root))
        {
            root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Console.WriteLine($"??  No root path specified, defaulting to: {root}");
        }

        return new AssessOptions
        {
            Providers = providers.ToArray(),
            Root = root,
            OutputPath = output
        };
    }

    private SimulateOptions ParseSimulateOptions(string[] args)
    {
        string? scanId = null;
        int batchSize = 500;
        string? output = null;
        string? rulesPath = null;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--scan-id" when i + 1 < args.Length:
                    scanId = args[++i];
                    break;
                case "--batch-size" when i + 1 < args.Length:
                    _ = int.TryParse(args[++i], out batchSize);
                    break;
                case "--out" when i + 1 < args.Length:
                    output = args[++i];
                    break;
                case "--rules" when i + 1 < args.Length:
                    rulesPath = args[++i];
                    break;
                case "--verbose" or "-v":
                    verbose = true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(scanId))
        {
            throw new ArgumentException("--scan-id is required for simulate command");
        }

        return new SimulateOptions
        {
            ScanId = scanId,
            BatchSize = batchSize,
            OutputPath = output,
            RulesPath = rulesPath,
            Verbose = verbose
        };
    }

    private ValidateOptions ParseValidateOptions(string[] args)
    {
        string? rulesPath = null;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--rules" when i + 1 < args.Length:
                    rulesPath = args[++i];
                    break;
                case "--verbose" or "-v":
                    verbose = true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(rulesPath))
        {
            throw new ArgumentException("--rules is required for validate command");
        }

        return new ValidateOptions
        {
            RulesPath = rulesPath,
            Verbose = verbose
        };
    }

    private MigrateOptions ParseMigrateOptions(string[] args)
    {
        string? scanId = null;
        string targetDir = "migrated";
        bool categorize = true;
        bool preserveStructure = true; // NEW: Default true
        bool dryRun = false;
        bool force = false;
        bool verbose = false;
        string? output = null;
        int maxPreview = 20;
        string conflict = "rename";
        bool useDefaultExclusions = true;
        var excludeDirs = new List<string>();
        var excludePatterns = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--scan-id" when i + 1 < args.Length:
                    scanId = args[++i];
                    break;
                case "--target" when i + 1 < args.Length:
                    targetDir = args[++i];
                    break;
                case "--categorize":
                    categorize = true;
                    break;
                case "--no-categorize":
                    categorize = false;
                    break;
                case "--preserve-structure": // NEW
                    preserveStructure = true;
                    break;
                case "--no-preserve-structure": // NEW
                    preserveStructure = false;
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "--force":
                    force = true;
                    break;
                case "--out" when i + 1 < args.Length:
                    output = args[++i];
                    break;
                case "--max-preview" when i + 1 < args.Length:
                    _ = int.TryParse(args[++i], out maxPreview);
                    break;
                case "--conflict" when i + 1 < args.Length:
                    conflict = args[++i];
                    break;
                case "--exclude-default":
                    useDefaultExclusions = true;
                    break;
                case "--no-exclude-default":
                    useDefaultExclusions = false;
                    break;
                case "--exclude-dirs" when i + 1 < args.Length:
                    excludeDirs.AddRange(args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries));
                    break;
                case "--exclude-patterns" when i + 1 < args.Length:
                    excludePatterns.AddRange(args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries));
                    break;
                case "--verbose" or "-v":
                    verbose = true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(scanId))
        {
            throw new ArgumentException("--scan-id is required for migrate command");
        }

        return new MigrateOptions
        {
            ScanId = scanId,
            TargetDirectory = targetDir,
            EnableCategorization = categorize,
            PreserveStructure = preserveStructure, // NEW
            DryRun = dryRun,
            Force = force,
            Verbose = verbose,
            OutputPath = output,
            MaxPreviewItems = maxPreview,
            ConflictResolution = conflict,
            UseDefaultExclusions = useDefaultExclusions,
            ExcludeDirectories = excludeDirs.Count > 0 ? excludeDirs.ToArray() : null,
            ExcludePatterns = excludePatterns.Count > 0 ? excludePatterns.ToArray() : null,
            SourceRoot = null // Will be set by MigrateCommand from scan result
        };
    }

    private int ShowHelp()
    {
        Console.WriteLine("DocsUnmessed - Personal File Organization Tool");
        Console.WriteLine("===============================================\n");
        Console.WriteLine("Usage: docsunmessed <command> [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  assess    - Scan and assess current storage setup");
        Console.WriteLine("  simulate  - Simulate migration with rules engine");
        Console.WriteLine("  migrate   - Execute migration plan");
        Console.WriteLine("  validate  - Validate folder structure and naming");
        Console.WriteLine("  help      - Show this help message");
        Console.WriteLine("  version   - Show version information\n");
        Console.WriteLine("Assess Options:");
        Console.WriteLine("  --providers <providers>  Comma-separated provider list (default: fs_local)");
        Console.WriteLine("  --root <path>            Root path to scan (default: user's Documents folder)");
        Console.WriteLine("  --out <path>             Output file for scan results (JSON)\n");
        Console.WriteLine("Simulate Options:");
        Console.WriteLine("  --scan-id <id>           Scan ID from assess command (required)");
        Console.WriteLine("  --rules <path>           Path to rules configuration file (JSON)");
        Console.WriteLine("  --out <path>             Output file for suggestions (JSON)");
        Console.WriteLine("  --batch-size <n>         Batch size for processing (default: 500)");
        Console.WriteLine("  --verbose, -v            Show detailed error information\n");
        Console.WriteLine("Validate Options:");
        Console.WriteLine("  --rules <path>           Path to rules configuration file to validate (required)");
        Console.WriteLine("  --verbose, -v            Show detailed error information\n");
        Console.WriteLine("Migrate Options:");
        Console.WriteLine("  --scan-id <id>           Scan ID from assess command (required)");
        Console.WriteLine("  --target <path>          Target directory for migrated files (default: migrated)");
        Console.WriteLine("  --categorize             Enable category-based organization (default)");
        Console.WriteLine("  --no-categorize          Disable categorization");
        Console.WriteLine("  --preserve-structure     Preserve source subdirectory structure (default)");
        Console.WriteLine("  --no-preserve-structure  Flatten all files to category root");
        Console.WriteLine("  --dry-run                Preview migration without moving files");
        Console.WriteLine("  --force                  Skip confirmation prompt");
        Console.WriteLine("  --out <path>             Save migration report to file (JSON)");
        Console.WriteLine("  --conflict <mode>        Conflict resolution: rename (default), skip, overwrite");
        Console.WriteLine("  --max-preview <n>        Max items to show in dry-run (default: 20)");
        Console.WriteLine("  --exclude-default        Use default directory exclusions");
        Console.WriteLine("  --exclude-dirs <dirs>    Comma-separated directories to exclude");
        Console.WriteLine("  --exclude-patterns <p>   Comma-separated file patterns to exclude");
        Console.WriteLine("  --verbose, -v            Show detailed error information\n");
        Console.WriteLine("Examples:");
        Console.WriteLine("  docsunmessed assess                                                             # Scan Documents folder");
        Console.WriteLine("  docsunmessed assess --root C:\\Users\\Me\\Documents --out scan.json               # Scan specific folder");
        Console.WriteLine("  docsunmessed assess --providers fs_local,onedrive_local                         # Scan multiple providers");
        Console.WriteLine("  docsunmessed simulate --scan-id abc123 --rules rules.json --out suggestions.json");
        Console.WriteLine("  docsunmessed migrate --scan-id abc123 --dry-run                                 # Preview migration");
        Console.WriteLine("  docsunmessed migrate --scan-id abc123 --target organized --categorize           # Execute migration");
        Console.WriteLine("  docsunmessed validate --rules examples/mapping-rules.json");
        return 0;
    }

    private int ShowVersion()
    {
        Console.WriteLine("DocsUnmessed v1.0.0");
        Console.WriteLine(".NET 10.0");
        return 0;
    }

    private int ShowUnknownCommand(string command)
    {
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine("Run 'docsunmessed help' for usage information.");
        return 1;
    }
}
