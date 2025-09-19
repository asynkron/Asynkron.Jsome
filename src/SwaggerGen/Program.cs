using SwaggerGen;
using SwaggerGen.Models;
using SwaggerGen.CodeGeneration;

namespace SwaggerGen;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SwaggerGen - Swagger 2.0 Parser & Code Generator");
        Console.WriteLine("=================================================");
        Console.WriteLine();

        try
        {
            // Parse command line arguments
            var options = ParseArguments(args);
            
            // Determine the Swagger file path
            var swaggerFilePath = GetSwaggerFilePath(options.InputFile);
            
            if (!File.Exists(swaggerFilePath))
            {
                Console.WriteLine($"Error: Swagger file not found at: {swaggerFilePath}");
                PrintUsage();
                return;
            }

            Console.WriteLine($"Loading Swagger file: {swaggerFilePath}");
            Console.WriteLine();

            // Parse the Swagger file
            SwaggerDocument document = await SwaggerParser.ParseFileAsync(swaggerFilePath);

            // Display the summary
            string summary = SwaggerParser.GetDocumentSummary(document);
            Console.WriteLine(summary);

            // Generate code if requested
            if (options.GenerateCode)
            {
                Console.WriteLine();
                Console.WriteLine("Generating code...");
                
                try
                {
                    var generator = new CodeGenerator();

                    // Generate DTOs
                    var dtoFiles = generator.GenerateDTOs(document, options.Namespace);
                    await WriteGeneratedFiles(dtoFiles, options.OutputPath, "DTOs");

                    // Generate Validators
                    var validatorFiles = generator.GenerateValidators(document, options.Namespace);
                    await WriteGeneratedFiles(validatorFiles, options.OutputPath, "Validators");

                    Console.WriteLine($"Code generation completed! Files written to: {options.OutputPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during code generation: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Processing completed successfully!");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: File not found - {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: Invalid Swagger document - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        if (args.Length == 0 && Environment.UserInteractive)
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    private static CommandLineOptions ParseArguments(string[] args)
    {
        var options = new CommandLineOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--input":
                case "-i":
                    if (i + 1 < args.Length)
                        options.InputFile = args[++i];
                    break;
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        options.OutputPath = args[++i];
                    break;
                case "--namespace":
                case "-n":
                    if (i + 1 < args.Length)
                        options.Namespace = args[++i];
                    break;
                case "--generate":
                case "-g":
                    options.GenerateCode = true;
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
            }
        }

        return options;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: SwaggerGen [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -i, --input <file>       Path to Swagger JSON file (default: sample file)");
        Console.WriteLine("  -o, --output <path>      Output directory for generated files (default: ./Generated)");
        Console.WriteLine("  -n, --namespace <name>   Namespace for generated classes (default: Generated.DTOs)");
        Console.WriteLine("  -g, --generate           Generate DTO classes and validators");
        Console.WriteLine("  -h, --help               Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  SwaggerGen -i petstore.json -g -o ./Output -n MyApi.Models");
        Console.WriteLine("  SwaggerGen --generate --input api.json --namespace Company.Api.DTOs");
    }

    private static string GetSwaggerFilePath(string? inputFile)
    {
        if (!string.IsNullOrEmpty(inputFile))
        {
            return inputFile;
        }

        // Default to sample file
        var currentDirectory = Directory.GetCurrentDirectory();
        var sampleFilePath = Path.Combine(currentDirectory, "Samples", "petstore-swagger.json");
        
        // Check if we're running from the build output directory
        if (!File.Exists(sampleFilePath))
        {
            // Try to find the file relative to the source directory
            var sourceDirectory = FindSourceDirectory(currentDirectory);
            if (sourceDirectory != null)
            {
                sampleFilePath = Path.Combine(sourceDirectory, "Samples", "petstore-swagger.json");
            }
        }

        return sampleFilePath;
    }

    private static async Task WriteGeneratedFiles(List<GeneratedFile> files, string outputPath, string subfolder)
    {
        var fullOutputPath = Path.Combine(outputPath, subfolder);
        Directory.CreateDirectory(fullOutputPath);

        foreach (var file in files)
        {
            var filePath = Path.Combine(fullOutputPath, file.FileName);
            await File.WriteAllTextAsync(filePath, file.Content);
            Console.WriteLine($"  Generated: {filePath}");
        }
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

/// <summary>
/// Command line options
/// </summary>
public class CommandLineOptions
{
    public string? InputFile { get; set; }
    public string OutputPath { get; set; } = "./Generated";
    public string Namespace { get; set; } = "Generated.DTOs";
    public bool GenerateCode { get; set; } = false;
}
