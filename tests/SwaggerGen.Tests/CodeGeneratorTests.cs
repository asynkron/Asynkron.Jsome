using SwaggerGen;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class CodeGeneratorTests
{
    [Fact]
    public async Task GenerateCodeAsync_WithSimpleSchema_GeneratesCorrectDto()
    {
        // Arrange
        var templatesPath = GetTemplatesPath();
        var codeGenerator = new CodeGenerator(templatesPath);
        var outputPath = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestModel"] = new Schema
                {
                    Type = "object",
                    Required = new List<string> { "name" },
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string" },
                        ["age"] = new Schema { Type = "integer" }
                    }
                }
            }
        };

        try
        {
            // Act
            await codeGenerator.GenerateCodeAsync(document, outputPath);

            // Assert
            var dtoPath = Path.Combine(outputPath, "Models", "TestModel.cs");
            var validatorPath = Path.Combine(outputPath, "Validators", "TestModelValidator.cs");
            
            Assert.True(File.Exists(dtoPath));
            Assert.True(File.Exists(validatorPath));

            var dtoContent = await File.ReadAllTextAsync(dtoPath);
            var validatorContent = await File.ReadAllTextAsync(validatorPath);

            // Check DTO content
            Assert.Contains("public class TestModel", dtoContent);
            Assert.Contains("public string Name { get; set; } = string.Empty;", dtoContent);
            Assert.Contains("public int? Age { get; set; }", dtoContent);
            Assert.Contains("[Required]", dtoContent);

            // Check Validator content
            Assert.Contains("public class TestModelValidator : AbstractValidator<TestModel>", validatorContent);
            Assert.Contains("RuleFor(x => x.Name).NotEmpty();", validatorContent);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    [Fact]
    public async Task GenerateCodeAsync_WithInheritanceSchema_GeneratesCorrectInheritance()
    {
        // Arrange
        var templatesPath = GetTemplatesPath();
        var codeGenerator = new CodeGenerator(templatesPath);
        var outputPath = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BaseModel"] = new Schema
                {
                    Type = "object",
                    Required = new List<string> { "name" },
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string" }
                    }
                },
                ["DerivedModel"] = new Schema
                {
                    Type = "object",
                    AllOf = new List<Schema>
                    {
                        new Schema { Ref = "#/definitions/BaseModel" },
                        new Schema
                        {
                            Required = new List<string> { "id" },
                            Properties = new Dictionary<string, Schema>
                            {
                                ["id"] = new Schema { Type = "integer", Format = "int64" }
                            }
                        }
                    }
                }
            }
        };

        // Create mock JSON for $ref handling
        var mockJson = """
        {
            "definitions": {
                "BaseModel": {
                    "type": "object",
                    "required": ["name"],
                    "properties": {
                        "name": { "type": "string" }
                    }
                },
                "DerivedModel": {
                    "type": "object",
                    "allOf": [
                        { "$ref": "#/definitions/BaseModel" },
                        {
                            "required": ["id"],
                            "properties": {
                                "id": { "type": "integer", "format": "int64" }
                            }
                        }
                    ]
                }
            }
        }
        """;

        try
        {
            // Act
            await codeGenerator.GenerateCodeAsync(document, outputPath, mockJson);

            // Assert
            var baseDtoPath = Path.Combine(outputPath, "Models", "BaseModel.cs");
            var derivedDtoPath = Path.Combine(outputPath, "Models", "DerivedModel.cs");
            
            Assert.True(File.Exists(baseDtoPath));
            Assert.True(File.Exists(derivedDtoPath));

            var baseDtoContent = await File.ReadAllTextAsync(baseDtoPath);
            var derivedDtoContent = await File.ReadAllTextAsync(derivedDtoPath);

            // Check base DTO content
            Assert.Contains("public class BaseModel", baseDtoContent);
            Assert.Contains("public string Name { get; set; } = string.Empty;", baseDtoContent);

            // Check derived DTO content has inheritance
            Assert.Contains("public class DerivedModel : BaseModel", derivedDtoContent);
            Assert.Contains("public long Id { get; set; }", derivedDtoContent);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
    }

    private string GetTemplatesPath()
    {
        // Find the templates directory relative to the test project
        var currentDir = Directory.GetCurrentDirectory();
        
        // Look for the src directory in parent directories
        var dir = new DirectoryInfo(currentDir);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        
        if (dir == null)
        {
            throw new DirectoryNotFoundException($"Could not find src directory from: {currentDir}");
        }
        
        var templatesPath = Path.Combine(dir.FullName, "src", "SwaggerGen", "Templates");
        
        if (!Directory.Exists(templatesPath))
        {
            throw new DirectoryNotFoundException($"Templates directory not found at: {templatesPath}");
        }
        
        return templatesPath;
    }
}