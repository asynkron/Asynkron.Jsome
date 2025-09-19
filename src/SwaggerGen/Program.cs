using SwaggerGen;
using SwaggerGen.Models;

namespace SwaggerGen;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SwaggerGen - Swagger 2.0 Code Generator");
        Console.WriteLine("=======================================");
        Console.WriteLine();

        try
        {
            string? swaggerFilePath = null;
            string? outputPath = null;

            // Parse command line arguments
            if (args.Length >= 1)
            {
                swaggerFilePath = args[0];
                outputPath = args.Length >= 2 ? args[1] : Directory.GetCurrentDirectory();
            }
            else
            {
                // Use default sample file if no arguments provided
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

                if (File.Exists(sampleFilePath))
                {
                    swaggerFilePath = sampleFilePath;
                    outputPath = Path.Combine(currentDirectory, "Generated");
                    Console.WriteLine("No arguments provided. Using sample Swagger file for demonstration.");
                    Console.WriteLine("Usage: SwaggerGen <swagger-file> [output-directory]");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Usage: SwaggerGen <swagger-file> [output-directory]");
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  swagger-file      Path to the Swagger 2.0 JSON file");
                    Console.WriteLine("  output-directory  Output directory for generated code (optional, defaults to current directory)");
                    Console.WriteLine();
                    Console.WriteLine("Example:");
                    Console.WriteLine("  SwaggerGen petstore.json ./Generated");
                    return;
                }
            }

            if (!File.Exists(swaggerFilePath))
            {
                Console.WriteLine($"Error: Swagger file not found: {swaggerFilePath}");
                return;
            }

            Console.WriteLine($"Loading Swagger file: {swaggerFilePath}");
            Console.WriteLine($"Output directory: {outputPath}");
            Console.WriteLine();

            // Read JSON for code generation
            var swaggerJson = await File.ReadAllTextAsync(swaggerFilePath);

            // Parse the Swagger file
            SwaggerDocument document = await SwaggerParser.ParseFileAsync(swaggerFilePath);

            // Display the summary
            string summary = SwaggerParser.GetDocumentSummary(document);
            Console.WriteLine(summary);
            Console.WriteLine();

            // Generate code using Handlebars templates
            var currentDir = Directory.GetCurrentDirectory();
            var templatesPath = Path.Combine(currentDir, "Templates");
            
            // Check if we're running from the build output directory
            if (!Directory.Exists(templatesPath))
            {
                var sourceDirectory = FindSourceDirectory(currentDir);
                if (sourceDirectory != null)
                {
                    templatesPath = Path.Combine(sourceDirectory, "Templates");
                }
            }

            if (!Directory.Exists(templatesPath))
            {
                Console.WriteLine($"Error: Templates directory not found at: {templatesPath}");
                Console.WriteLine("Please ensure the Templates directory exists with DTO.hbs and Validator.hbs files.");
                return;
            }

            var codeGenerator = new CodeGenerator(templatesPath);
            await codeGenerator.GenerateCodeAsync(document, outputPath, swaggerJson);

            Console.WriteLine();
            Console.WriteLine("Code generation completed successfully!");
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
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
        if (Environment.UserInteractive && !Console.IsInputRedirected)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
