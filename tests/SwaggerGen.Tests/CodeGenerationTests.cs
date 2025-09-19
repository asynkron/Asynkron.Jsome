using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class CodeGenerationTests
{
    [Fact]
    public void CodeGenerator_GeneratesDto_FromSimpleSchema()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" },
                        ["name"] = new Schema { Type = "string" }
                    },
                    Required = new List<string> { "name" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        Assert.True(result.DtoClasses.ContainsKey("User"));
        
        var userDto = result.DtoClasses["User"];
        Assert.Contains("public class User", userDto);
        Assert.Contains("namespace Test.Generated", userDto);
        Assert.Contains("public int Id { get; set; }", userDto);
        Assert.Contains("public string Name { get; set; }", userDto);
        Assert.Contains("[Required]", userDto);
        Assert.Contains("[JsonProperty(\"name\")]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesValidator_FromSimpleSchema()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["email"] = new Schema 
                        { 
                            Type = "string",
                            MaxLength = 100,
                            MinLength = 5
                        }
                    },
                    Required = new List<string> { "email" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.Validators);
        Assert.True(result.Validators.ContainsKey("User"));
        
        var userValidator = result.Validators["User"];
        Assert.Contains("public class UserValidator", userValidator);
        Assert.Contains("AbstractValidator<User>", userValidator);
        Assert.Contains("RuleFor(x => x.Email)", userValidator);
        Assert.Contains(".NotEmpty", userValidator);
        Assert.Contains(".MinimumLength(5)", userValidator);
        Assert.Contains(".MaximumLength(100)", userValidator);
    }

    [Fact]
    public void CodeGenerator_HandlesMultipleDefinitions()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" }
                    }
                },
                ["Product"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Equal(2, result.DtoClasses.Count);
        Assert.Equal(2, result.Validators.Count);
        
        Assert.True(result.DtoClasses.ContainsKey("User"));
        Assert.True(result.DtoClasses.ContainsKey("Product"));
        Assert.True(result.Validators.ContainsKey("User"));
        Assert.True(result.Validators.ContainsKey("Product"));
    }

    [Fact]
    public void CodeGenerator_HandlesValidationConstraints()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["ValidationTest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["score"] = new Schema 
                        { 
                            Type = "number",
                            Minimum = 0,
                            Maximum = 100
                        },
                        ["pattern"] = new Schema
                        {
                            Type = "string",
                            Pattern = @"^\d{3}-\d{2}-\d{4}$"
                        }
                    },
                    Required = new List<string> { "score" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var validator = result.Validators["ValidationTest"];
        Assert.Contains(".GreaterThanOrEqualTo(0)", validator);
        Assert.Contains(".LessThanOrEqualTo(100)", validator);
        Assert.Contains(@".Matches(&quot;^\d{3}-\d{2}-\d{4}$&quot;)", validator);
    }

    [Fact]
    public void CodeGenerator_HandlesRefProperties()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["address"] = new Schema { Ref = "#/definitions/Address" },
                        ["name"] = new Schema { Type = "string" }
                    }
                },
                ["Address"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["street"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Equal(2, result.DtoClasses.Count);
        var userDto = result.DtoClasses["User"];
        
        // Check that the address property uses the correct type
        Assert.Contains("public Address Address { get; set; }", userDto);
        Assert.Contains("public string Name { get; set; }", userDto);
    }
}