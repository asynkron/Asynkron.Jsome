using SwaggerGen;
using SwaggerGen.Models;
using SwaggerGen.CodeGeneration;

namespace SwaggerGen;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SwaggerGen - Swagger 2.0 Parser");
        Console.WriteLine("================================");
        Console.WriteLine();

        try
        {
            // Get the path to the sample swagger file
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

            if (!File.Exists(sampleFilePath))
            {
                Console.WriteLine($"Sample file not found at: {sampleFilePath}");
                Console.WriteLine("Please ensure the petstore-swagger.json file exists in the Samples directory.");
                return;
            }

            Console.WriteLine($"Loading Swagger file: {sampleFilePath}");
            Console.WriteLine();

            // Parse the Swagger file
            SwaggerDocument document = await SwaggerParser.ParseFileAsync(sampleFilePath);

            // Display the summary
            string summary = SwaggerParser.GetDocumentSummary(document);
            Console.WriteLine(summary);

            Console.WriteLine();
            Console.WriteLine("Generating C# DTO classes and FluentValidation validators...");
            Console.WriteLine();

            try
            {
                // Generate code
                var codeGenerator = new CodeGenerator();
                var generationResult = codeGenerator.GenerateCode(document, "SwaggerGen.Generated");

                // Display generated DTOs
                Console.WriteLine("Generated DTO Classes:");
                Console.WriteLine("=====================");
                foreach (var dto in generationResult.DtoClasses)
                {
                    Console.WriteLine($"--- {dto.Key}.cs ---");
                    Console.WriteLine(dto.Value);
                    Console.WriteLine();
                }

                // Display generated Validators
                Console.WriteLine("Generated FluentValidation Validators:");
                Console.WriteLine("=====================================");
                foreach (var validator in generationResult.Validators)
                {
                    Console.WriteLine($"--- {validator.Key}Validator.cs ---");
                    Console.WriteLine(validator.Value);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Code generation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Parsing completed successfully!");
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

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
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
