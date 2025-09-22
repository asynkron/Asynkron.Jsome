using SwaggerGen;
using SwaggerGen.Models;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Configuration;
using System.CommandLine;
using System.Globalization;
using Spectre.Console;

namespace SwaggerGen;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Display welcome banner
        DisplayWelcomeBanner();

        // Create root command
        var rootCommand = new RootCommand("SwaggerGen - Advanced Swagger 2.0 Code Generator")
        {
            CreateGenerateCommand(),
            CreateHelpCommand()
        };

        return await rootCommand.InvokeAsync(args);
    }

    private static void DisplayWelcomeBanner()
    {
        var rule = new Rule("[bold blue]SwaggerGen - Advanced Swagger 2.0 Code Generator[/]")
        {
            Style = Style.Parse("blue")
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[dim]Generate clean C# DTOs and FluentValidation validators from Swagger 2.0 specifications[/]");
        AnsiConsole.WriteLine();
    }

    private static Command CreateGenerateCommand()
    {
        var swaggerFileArgument = new Argument<FileInfo?>(
            name: "swaggerFile",
            description: "Path to the Swagger 2.0 JSON file to process")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        
        var configOption = new Option<FileInfo?>(
            aliases: ["--config", "-c"],
            description: "Path to YAML or JSON configuration file for customizing generation");
            
        var namespaceOption = new Option<string?>(
            aliases: ["--namespace", "-n"],
            description: "Override the default namespace for generated code");
            
        var outputOption = new Option<DirectoryInfo?>(
            aliases: ["--output", "-o"],
            description: "Output directory for generated files (default: console output only)");

        var yesOption = new Option<bool>(
            aliases: ["--yes", "-y"],
            description: "Skip confirmation prompts and proceed automatically");

        var templateDirOption = new Option<DirectoryInfo?>(
            aliases: new[] { "--template-dir", "-t" },
            description: "Custom directory containing Handlebars template files (DTO.hbs, Validator.hbs, etc.)");

        var modernFeaturesOption = new Option<bool>(
            aliases: new[] { "--modern", "-m" },
            description: "Enable modern C# features: nullable reference types and required keyword");

        var generateRecordsOption = new Option<bool>(
            aliases: new[] { "--records" },
            description: "Generate C# records instead of classes for DTOs");

        var useSystemTextJsonOption = new Option<bool>(
            aliases: new[] { "--system-text-json" },
            description: "Use System.Text.Json attributes and enhanced validation (JsonPropertyName, JsonIgnore conditions, Required(AllowEmptyStrings = false), StringLength)");

        var schemaDirOption = new Option<DirectoryInfo?>(
            aliases: new[] { "--schema-dir", "-s" },
            description: "Directory containing multiple JSON Schema files to process instead of a single Swagger file");
            
        var generateCommand = new Command("generate", "Generate C# code from a Swagger specification")
        {
            swaggerFileArgument,
            configOption,
            namespaceOption,
            outputOption,
            yesOption,
            templateDirOption,
            modernFeaturesOption,
            generateRecordsOption,
            useSystemTextJsonOption,
            schemaDirOption
        };

        generateCommand.SetHandler(async (context) =>
        {
            var swaggerFile = context.ParseResult.GetValueForArgument(swaggerFileArgument);
            var configFile = context.ParseResult.GetValueForOption(configOption);
            var namespaceOverride = context.ParseResult.GetValueForOption(namespaceOption);
            var outputDir = context.ParseResult.GetValueForOption(outputOption);
            var skipConfirmation = context.ParseResult.GetValueForOption(yesOption);
            var templateDir = context.ParseResult.GetValueForOption(templateDirOption);
            var useModern = context.ParseResult.GetValueForOption(modernFeaturesOption);
            var generateRecords = context.ParseResult.GetValueForOption(generateRecordsOption);
            var useSystemTextJson = context.ParseResult.GetValueForOption(useSystemTextJsonOption);
            var schemaDir = context.ParseResult.GetValueForOption(schemaDirOption);
            
            await HandleGenerateCommand(swaggerFile, configFile, namespaceOverride, outputDir, skipConfirmation, templateDir, useModern, generateRecords, useSystemTextJson, schemaDir);
        });

        return generateCommand;
    }

    private static Command CreateHelpCommand()
    {
        var helpCommand = new Command("help", "Show detailed help and examples");
        
        helpCommand.SetHandler(() =>
        {
            DisplayDetailedHelp();
        });

        return helpCommand;
    }

    private static async Task HandleGenerateCommand(FileInfo? swaggerFile, FileInfo? configFile, string? namespaceOverride, DirectoryInfo? outputDir, bool skipConfirmation, DirectoryInfo? templateDir, bool useModern, bool generateRecords, bool useSystemTextJson, DirectoryInfo? schemaDir)
    {
        try
        {
            SwaggerDocument document;

            // Validate that either swaggerFile or schemaDir is provided, but not both
            if (swaggerFile != null && schemaDir != null)
            {
                throw new InvalidOperationException("Cannot specify both a Swagger file and a schema directory. Please use either --swagger-file or --schema-dir, but not both.");
            }

            if (schemaDir != null)
            {
                // Generate from JSON Schema directory
                AnsiConsole.MarkupLine($"[blue]📁 Loading JSON Schema files from directory:[/] [yellow]{schemaDir.FullName}[/]");
                document = JsonSchemaParser.ParseDirectory(schemaDir.FullName);
                DisplayJsonSchemaSummary(document, schemaDir.FullName);
            }
            else
            {
                // Generate from single Swagger file (existing behavior)
                string swaggerFilePath = await DetermineSwaggerFilePath(swaggerFile);
                document = await LoadSwaggerDocument(swaggerFilePath);
                DisplaySwaggerSummary(document);
            }

            // Load configuration if provided
            ModifierConfiguration? config = null;
            if (configFile != null)
            {
                config = await LoadAndDisplayConfiguration(configFile, skipConfirmation);
            }

            // Validate configuration against schema if both are provided
            if (config != null)
            {
                ValidateConfiguration(config, document, skipConfirmation);
            }

            // Generate code
            await GenerateCode(document, config, namespaceOverride, outputDir, templateDir, useModern, generateRecords, useSystemTextJson);

            AnsiConsole.MarkupLine("[green]✓ Code generation completed successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
        }
    }

    private static async Task<string> DetermineSwaggerFilePath(FileInfo? swaggerFile)
    {
        string filePath;
        
        if (swaggerFile != null)
        {
            filePath = swaggerFile.FullName;
        }
        else
        {
            // Use default sample file
            var currentDirectory = Directory.GetCurrentDirectory();
            filePath = Path.Combine(currentDirectory, "Samples", "petstore-swagger.json");
            
            // Check if we're running from the build output directory
            if (!File.Exists(filePath))
            {
                var sourceDirectory = FindSourceDirectory(currentDirectory);
                if (sourceDirectory != null)
                {
                    filePath = Path.Combine(sourceDirectory, "Samples", "petstore-swagger.json");
                }
            }
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Swagger file not found at: {filePath}");
        }

        return filePath;
    }

    private static async Task<ModifierConfiguration> LoadAndDisplayConfiguration(FileInfo configFile, bool skipConfirmation)
    {
        AnsiConsole.MarkupLine($"[yellow]📁 Loading configuration from: {configFile.FullName}[/]");
        
        if (!configFile.Exists)
        {
            throw new FileNotFoundException($"Configuration file not found: {configFile.FullName}");
        }

        var config = await ConfigurationLoader.LoadAsync(configFile.FullName);
        
        // Display configuration contents
        DisplayConfigurationContents(config, configFile.FullName, skipConfirmation);
        
        return config;
    }

    private static void DisplayConfigurationContents(ModifierConfiguration config, string filePath, bool skipConfirmation)
    {
        var panel = new Panel(CreateConfigurationTable(config))
        {
            Header = new PanelHeader($"[bold]Configuration Loaded from {Path.GetFileName(filePath)}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("yellow")
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Ask for user confirmation only if interactive
        if (skipConfirmation)
        {
            AnsiConsole.MarkupLine("[dim]Skipping confirmation (--yes flag specified)...[/]");
        }
        else
        {
            bool isInteractive = !Console.IsInputRedirected && !Console.IsOutputRedirected && Environment.UserInteractive;
            if (!isInteractive)
            {
                AnsiConsole.MarkupLine("[dim]Running in non-interactive mode, proceeding with configuration...[/]");
            }
            else if (!AnsiConsole.Confirm("[yellow]Do you want to proceed with this configuration?[/]"))
            {
                throw new OperationCanceledException("Generation cancelled by user");
            }
        }
        
        AnsiConsole.WriteLine();
    }

    private static Table CreateConfigurationTable(ModifierConfiguration config)
    {
        var table = new Table();
        table.AddColumn("[bold]Setting[/]");
        table.AddColumn("[bold]Value[/]");

        // Global settings
        if (config.Global != null)
        {
            table.AddRow("[dim]Global Settings[/]", "");
            if (!string.IsNullOrEmpty(config.Global.Namespace))
                table.AddRow("  Namespace", config.Global.Namespace);
            if (config.Global.GenerateEnumTypes.HasValue)
                table.AddRow("  Generate Enum Types", Convert.ToString(config.Global.GenerateEnumTypes.Value, CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(config.Global.TypeNamePrefix))
                table.AddRow("  Type Name Prefix", config.Global.TypeNamePrefix);
            if (!string.IsNullOrEmpty(config.Global.TypeNameSuffix))
                table.AddRow("  Type Name Suffix", config.Global.TypeNameSuffix);
        }

        // Rules
        if (config.Rules.Any())
        {
            table.AddRow("", "");
            table.AddRow("[dim]Property Rules[/]", $"[dim]{config.Rules.Count} rules configured[/]");
            
            foreach (var rule in config.Rules.Take(5)) // Show first 5 rules
            {
                var status = rule.Value.Include == false ? "[red]Excluded[/]" : "[green]Included[/]";
                table.AddRow($"  {rule.Key}", status);
            }
            
            if (config.Rules.Count > 5)
            {
                table.AddRow("  ...", $"[dim]and {config.Rules.Count - 5} more rules[/]");
            }
        }

        return table;
    }

    private static async Task<SwaggerDocument> LoadSwaggerDocument(string filePath)
    {
        AnsiConsole.MarkupLine($"[cyan]📄 Loading Swagger document: {Path.GetFileName(filePath)}[/]");
        
        var document = await SwaggerParser.ParseFileAsync(filePath);
        
        // Display document summary
        DisplaySwaggerSummary(document);
        
        return document;
    }

    private static void DisplaySwaggerSummary(SwaggerDocument document)
    {
        var table = new Table();
        table.AddColumn("[bold]Property[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Title", document.Info.Title);
        table.AddRow("Version", document.Info.Version);
        table.AddRow("Swagger Version", document.Swagger);
        if (!string.IsNullOrEmpty(document.Host))
            table.AddRow("Host", document.Host);
        if (!string.IsNullOrEmpty(document.BasePath))
            table.AddRow("Base Path", document.BasePath);
        table.AddRow("Paths", Convert.ToString(document.Paths.Count, CultureInfo.InvariantCulture));
        table.AddRow("Definitions", Convert.ToString(document.Definitions.Count, CultureInfo.InvariantCulture));

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[bold]Swagger Document Summary[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan")
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void DisplayJsonSchemaSummary(SwaggerDocument document, string directoryPath)
    {
        var table = new Table();
        table.AddColumn("[bold]Property[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Source Directory", directoryPath);
        table.AddRow("Generated Title", document.Info.Title);
        table.AddRow("Generated Version", document.Info.Version);
        table.AddRow("Schema Count", Convert.ToString(document.Definitions.Count, CultureInfo.InvariantCulture));
        
        // Show some example schema names
        var schemaNames = document.Definitions.Keys.Take(5).ToList();
        if (schemaNames.Any())
        {
            table.AddRow("Sample Schemas", string.Join(", ", schemaNames) + (document.Definitions.Count > 5 ? "..." : ""));
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[bold]JSON Schema Directory Summary[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("green")
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void ValidateConfiguration(ModifierConfiguration config, SwaggerDocument document, bool skipConfirmation)
    {
        AnsiConsole.MarkupLine("[yellow]🔍 Validating configuration against Swagger schema...[/]");
        
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);
        SchemaValidator.DisplayValidationErrors(errors);
        
        if (errors.Any())
        {
            if (skipConfirmation)
            {
                AnsiConsole.MarkupLine("[dim]Skipping validation confirmation (--yes flag specified), continuing...[/]");
            }
            else
            {
                bool isInteractive = !Console.IsInputRedirected && !Console.IsOutputRedirected && Environment.UserInteractive;
                if (!isInteractive)
                {
                    AnsiConsole.MarkupLine("[dim]Running in non-interactive mode, continuing despite validation errors...[/]");
                }
                else if (!AnsiConsole.Confirm("[red]Configuration validation failed. Do you want to continue anyway?[/]"))
                {
                    throw new OperationCanceledException("Generation cancelled due to validation errors");
                }
            }
        }
        
        AnsiConsole.WriteLine();
    }

    private static async Task GenerateCode(SwaggerDocument document, ModifierConfiguration? config, string? namespaceOverride, DirectoryInfo? outputDir, DirectoryInfo? templateDir, bool useModern, bool generateRecords, bool useSystemTextJson)
    {
        AnsiConsole.MarkupLine("[green]⚙️  Generating C# code...[/]");
        
        // Create generation options
        var options = new CodeGenerationOptions
        {
            UseNullableReferenceTypes = useModern,
            UseRequiredKeyword = useModern,
            GenerateRecords = generateRecords,
            UseSystemTextJson = useSystemTextJson
        };
        
        if (config != null)
        {
            options.ModifierConfiguration = config;
        }
        
        // Set custom template directory if provided
        if (templateDir != null)
        {
            options.TemplateDirectory = templateDir.FullName;
        }
        
        // Apply namespace override if provided
        if (!string.IsNullOrEmpty(namespaceOverride))
        {
            if (options.ModifierConfiguration == null)
                options.ModifierConfiguration = new ModifierConfiguration();
            if (options.ModifierConfiguration.Global == null)
                options.ModifierConfiguration.Global = new GlobalSettings();
            options.ModifierConfiguration.Global.Namespace = namespaceOverride;
        }

        var codeGenerator = new CodeGenerator(options);
        var generationResult = codeGenerator.GenerateCode(document);

        // Output results
        if (outputDir != null)
        {
            await WriteFilesToDirectory(generationResult, outputDir);
        }
        else
        {
            DisplayGeneratedCode(generationResult);
        }
    }

    private static async Task WriteFilesToDirectory(CodeGenerationResult result, DirectoryInfo outputDir)
    {
        if (!outputDir.Exists)
            outputDir.Create();

        AnsiConsole.MarkupLine($"[green]📁 Writing generated files to: {outputDir.FullName}[/]");

        var progress = AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            });

        await progress.StartAsync(async ctx =>
        {
            var totalFiles = result.DtoClasses.Count + result.Validators.Count;
            var task = ctx.AddTask("[green]Writing files[/]", maxValue: totalFiles);

            // Write DTO classes
            foreach (var dto in result.DtoClasses)
            {
                var filePath = Path.Combine(outputDir.FullName, $"{dto.Key}.cs");
                await File.WriteAllTextAsync(filePath, dto.Value);
                task.Increment(1);
                await Task.Delay(10); // Small delay for visual effect
            }

            // Write validators
            foreach (var validator in result.Validators)
            {
                var filePath = Path.Combine(outputDir.FullName, $"{validator.Key}Validator.cs");
                await File.WriteAllTextAsync(filePath, validator.Value);
                task.Increment(1);
                await Task.Delay(10); // Small delay for visual effect
            }
        });

        AnsiConsole.MarkupLine($"[green]✓ Generated {result.DtoClasses.Count} DTO classes and {result.Validators.Count} validators[/]");
    }

    private static void DisplayGeneratedCode(CodeGenerationResult result)
    {
        // Display generated DTOs
        if (result.DtoClasses.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold green]Generated DTO Classes[/]"));
            AnsiConsole.WriteLine();

            foreach (var dto in result.DtoClasses)
            {
                var panel = new Panel(Markup.Escape(dto.Value))
                {
                    Header = new PanelHeader($"[bold]{dto.Key}.cs[/]"),
                    Border = BoxBorder.Rounded
                };
                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();
            }
        }

        // Display generated Validators
        if (result.Validators.Any())
        {
            AnsiConsole.Write(new Rule("[bold green]Generated FluentValidation Validators[/]"));
            AnsiConsole.WriteLine();

            foreach (var validator in result.Validators)
            {
                var panel = new Panel(Markup.Escape(validator.Value))
                {
                    Header = new PanelHeader($"[bold]{validator.Key}Validator.cs[/]"),
                    Border = BoxBorder.Rounded
                };
                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();
            }
        }
    }

    private static void DisplayDetailedHelp()
    {
        AnsiConsole.WriteLine();
        
        var helpTable = new Table();
        helpTable.AddColumn("[bold]Command[/]");
        helpTable.AddColumn("[bold]Description[/]");
        helpTable.AddColumn("[bold]Example[/]");

        helpTable.AddRow(
            "[cyan]swaggergen generate[/]",
            "Generate code from default sample",
            "[dim]swaggergen generate[/]"
        );
        
        helpTable.AddRow(
            "[cyan]swaggergen generate <file>[/]",
            "Generate code from specific Swagger file",
            "[dim]swaggergen generate petstore.json[/]"
        );
        
        helpTable.AddRow(
            "[cyan]swaggergen generate --config <file>[/]",
            "Generate with configuration file",
            "[dim]swaggergen generate --config config.yaml[/]"
        );
        
        helpTable.AddRow(
            "[cyan]swaggergen generate --output <dir>[/]",
            "Write generated files to directory",
            "[dim]swaggergen generate --output ./generated[/]"
        );
        
        helpTable.AddRow(
            "[cyan]swaggergen generate --template-dir <dir>[/]",
            "Use custom template directory",
            "[dim]swaggergen generate --template-dir ./my-templates[/]"
        );

        helpTable.AddRow(
            "[cyan]swaggergen generate --schema-dir <dir>[/]",
            "Generate from JSON Schema directory",
            "[dim]swaggergen generate --schema-dir ./schemas[/]"
        );

        var panel = new Panel(helpTable)
        {
            Header = new PanelHeader("[bold]Available Commands[/]"),
            Border = BoxBorder.Rounded
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Show configuration examples
        AnsiConsole.MarkupLine("[bold yellow]Configuration File Examples:[/]");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[dim]YAML configuration (config.yaml):[/]");
        AnsiConsole.MarkupLine("""
[dim]global:
  namespace: "MyApi.Generated"
  generateEnumTypes: true
  
rules:
  "User.Password":
    include: false
  "Order.Details.Product.Name":
    include: true
    description: "Product name"[/]
""");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]For more examples, see: src/SwaggerGen/Samples/Configuration/sample-config.yaml[/]");
    }

    /// <summary>
    /// Attempts to find the source directory by looking for the Samples folder
    /// </summary>
    /// <param name="currentDirectory">The current working directory</param>
    /// <returns>The source directory path if found, null otherwise</returns>
    private static string? FindSourceDirectory(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);
        
        // Walk up the directory tree looking for the source structure
        while (directory != null)
        {
            var srcDir = Path.Combine(directory.FullName, "src", "SwaggerGen");
            var samplesDir = Path.Combine(srcDir, "Samples");
            
            if (Directory.Exists(samplesDir))
            {
                return srcDir;
            }
            
            directory = directory.Parent;
        }
        
        return null;
    }
}
