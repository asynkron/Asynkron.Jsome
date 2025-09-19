using SwaggerGen.CodeGeneration;
using Xunit;

namespace SwaggerGen.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task ParseAndGenerateCode_PetstoreSwagger_GeneratesAllFiles()
    {
        // Arrange
        var sampleFilePath = Path.Combine(
            FindSourceDirectory() ?? throw new InvalidOperationException("Could not find source directory"),
            "Samples", 
            "petstore-swagger.json");

        // Act
        var document = await SwaggerParser.ParseFileAsync(sampleFilePath);
        var generator = new SwaggerCodeGenerator("Generated.DTOs");
        var generatedFiles = generator.GenerateCode(document);

        // Assert
        Assert.Equal(6, generatedFiles.Count); // 3 DTOs + 3 Validators

        // Check DTO files exist
        Assert.Contains("Pet.cs", generatedFiles.Keys);
        Assert.Contains("NewPet.cs", generatedFiles.Keys);
        Assert.Contains("Error.cs", generatedFiles.Keys);

        // Check Validator files exist
        Assert.Contains("PetValidator.cs", generatedFiles.Keys);
        Assert.Contains("NewPetValidator.cs", generatedFiles.Keys);
        Assert.Contains("ErrorValidator.cs", generatedFiles.Keys);

        // Validate NewPet content (basic properties)
        var newPetCode = generatedFiles["NewPet.cs"];
        Assert.Contains("public class NewPet", newPetCode);
        Assert.Contains("public string Name { get; set; }", newPetCode);
        Assert.Contains("public string? Tag { get; set; }", newPetCode);
        Assert.Contains("[Required]", newPetCode);

        // Validate NewPet validator
        var newPetValidatorCode = generatedFiles["NewPetValidator.cs"];
        Assert.Contains("public class NewPetValidator", newPetValidatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Name).NotEmpty();", newPetValidatorCode);

        // Validate Error content
        var errorCode = generatedFiles["Error.cs"];
        Assert.Contains("public class Error", errorCode);
        Assert.Contains("public int Code { get; set; }", errorCode);
        Assert.Contains("public string Message { get; set; }", errorCode);

        // Validate Pet content (should have Id property)
        var petCode = generatedFiles["Pet.cs"];
        Assert.Contains("public class Pet", petCode);
        Assert.Contains("public long Id { get; set; }", petCode);
        // Note: allOf properties from NewPet are not fully implemented yet
    }

    /// <summary>
    /// Attempts to find the source directory by looking for the Samples folder
    /// </summary>
    /// <returns>The source directory path if found, null otherwise</returns>
    private static string? FindSourceDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
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