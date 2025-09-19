using SwaggerGen;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class CodeGenerationTests
{
    private const string TestSwaggerJson = @"{
        ""swagger"": ""2.0"",
        ""info"": {
            ""title"": ""Test API"",
            ""version"": ""1.0.0""
        },
        ""definitions"": {
            ""TestModel"": {
                ""type"": ""object"",
                ""required"": [""id"", ""name""],
                ""properties"": {
                    ""id"": {
                        ""type"": ""integer"",
                        ""format"": ""int64""
                    },
                    ""name"": {
                        ""type"": ""string"",
                        ""maxLength"": 100,
                        ""minLength"": 1
                    },
                    ""email"": {
                        ""type"": ""string""
                    }
                }
            }
        }
    }";

    [Fact]
    public void GenerateDTOs_ValidSwaggerDocument_GeneratesCorrectDTOClasses()
    {
        // Arrange
        var document = SwaggerParser.Parse(TestSwaggerJson);
        var generator = new CodeGenerator();

        // Act
        var dtoFiles = generator.GenerateDTOs(document, "Test.DTOs");

        // Assert
        Assert.Single(dtoFiles);
        var dtoFile = dtoFiles.First();
        
        Assert.Equal("TestModel.cs", dtoFile.FileName);
        Assert.Equal(GeneratedFileType.DTO, dtoFile.Type);
        Assert.Contains("namespace Test.DTOs", dtoFile.Content);
        Assert.Contains("public class TestModel", dtoFile.Content);
        Assert.Contains("[Required]", dtoFile.Content);
        Assert.Contains("public long Id { get; set; }", dtoFile.Content);
        Assert.Contains("public string Name { get; set; }", dtoFile.Content);
        Assert.Contains("[StringLength(100, MinimumLength = 1)]", dtoFile.Content);
        Assert.Contains("public string Email { get; set; }", dtoFile.Content);
    }

    [Fact]
    public void GenerateValidators_ValidSwaggerDocument_GeneratesCorrectValidatorClasses()
    {
        // Arrange
        var document = SwaggerParser.Parse(TestSwaggerJson);
        var generator = new CodeGenerator();

        // Act
        var validatorFiles = generator.GenerateValidators(document, "Test.DTOs");

        // Assert
        Assert.Single(validatorFiles);
        var validatorFile = validatorFiles.First();
        
        Assert.Equal("TestModelValidator.cs", validatorFile.FileName);
        Assert.Equal(GeneratedFileType.Validator, validatorFile.Type);
        Assert.Contains("namespace Test.DTOs.Validators", validatorFile.Content);
        Assert.Contains("public class TestModelValidator : AbstractValidator<TestModel>", validatorFile.Content);
        Assert.Contains("RuleFor(x => x.Id).NotEmpty()", validatorFile.Content);
        Assert.Contains("RuleFor(x => x.Name).NotEmpty()", validatorFile.Content);
        Assert.Contains("RuleFor(x => x.Name).MaximumLength(100).MinimumLength(1)", validatorFile.Content);
    }

    [Fact]
    public void GenerateDTOs_PetstoreExample_GeneratesExpectedClasses()
    {
        // Arrange
        var petstoreJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "SwaggerGen", "Samples", "petstore-swagger.json"));
        var document = SwaggerParser.Parse(petstoreJson);
        var generator = new CodeGenerator();

        // Act
        var dtoFiles = generator.GenerateDTOs(document, "Petstore.DTOs");

        // Assert
        Assert.Equal(3, dtoFiles.Count);
        
        var classNames = dtoFiles.Select(f => Path.GetFileNameWithoutExtension(f.FileName)).ToList();
        Assert.Contains("Pet", classNames);
        Assert.Contains("NewPet", classNames);
        Assert.Contains("Error", classNames);
        
        // Check that all files have the correct namespace
        foreach (var file in dtoFiles)
        {
            Assert.Contains("namespace Petstore.DTOs", file.Content);
        }
    }
}