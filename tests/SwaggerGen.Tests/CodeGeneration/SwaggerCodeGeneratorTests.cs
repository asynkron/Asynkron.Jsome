using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests.CodeGeneration;

public class SwaggerCodeGeneratorTests
{
    [Fact]
    public void GenerateDto_ValidSchema_ReturnsValidCSharpCode()
    {
        // Arrange
        var generator = new SwaggerCodeGenerator("Test.Generated");
        var schema = new Schema
        {
            Type = "object",
            Description = "A test model",
            Properties = new Dictionary<string, Schema>
            {
                ["id"] = new Schema { Type = "integer", Description = "The ID" },
                ["name"] = new Schema { Type = "string", Description = "The name" }
            },
            Required = new List<string> { "id", "name" }
        };

        // Act
        var result = generator.GenerateDto("TestModel", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public class TestModel", result);
        Assert.Contains("public int Id { get; set; }", result);
        Assert.Contains("public string Name { get; set; }", result);
        Assert.Contains("[Required]", result);
        Assert.Contains("namespace Test.Generated.DTOs", result);
    }

    [Fact]
    public void GenerateValidator_ValidSchema_ReturnsValidValidatorCode()
    {
        // Arrange
        var generator = new SwaggerCodeGenerator("Test.Generated");
        var schema = new Schema
        {
            Type = "object",
            Properties = new Dictionary<string, Schema>
            {
                ["name"] = new Schema 
                { 
                    Type = "string", 
                    MinLength = 1, 
                    MaxLength = 100 
                },
                ["age"] = new Schema 
                { 
                    Type = "integer", 
                    Minimum = 0, 
                    Maximum = 150 
                }
            },
            Required = new List<string> { "name" }
        };

        // Act
        var result = generator.GenerateValidator("Person", schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("public class PersonValidator : AbstractValidator<Person>", result);
        Assert.Contains("RuleFor(x => x.Name)", result);
        Assert.Contains(".NotNull()", result);
        Assert.Contains(".NotEmpty()", result);
        Assert.Contains(".MinimumLength(1)", result);
        Assert.Contains(".MaximumLength(100)", result);
        Assert.Contains("namespace Test.Generated.Validators", result);
    }

    [Fact]
    public async Task GenerateAsync_ValidDocument_CreatesFilesSuccessfully()
    {
        // Arrange
        var generator = new SwaggerCodeGenerator("Test.Generated");
        var document = new SwaggerDocument
        {
            Swagger = "2.0",
            Info = new Info { Title = "Test API", Version = "1.0.0" },
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" },
                        ["username"] = new Schema { Type = "string" }
                    },
                    Required = new List<string> { "id", "username" }
                }
            }
        };

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await generator.GenerateAsync(document, tempDir);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.TotalFilesGenerated); // DTO + Validator
            Assert.Contains("User", result.GeneratedDtos);
            Assert.All(result.GeneratedFiles, file => Assert.True(File.Exists(file)));

            // Check DTO file content
            var dtoFile = result.GeneratedFiles.First(f => f.Contains("User.cs") && f.Contains("DTOs"));
            var dtoContent = await File.ReadAllTextAsync(dtoFile);
            Assert.Contains("public class User", dtoContent);

            // Check Validator file content
            var validatorFile = result.GeneratedFiles.First(f => f.Contains("UserValidator.cs"));
            var validatorContent = await File.ReadAllTextAsync(validatorFile);
            Assert.Contains("public class UserValidator", validatorContent);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}