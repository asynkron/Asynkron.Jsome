using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class ModernCSharpFeaturesTests
{
    [Fact]
    public void CodeGenerator_GeneratesNullableReferenceTypes_WhenEnabled()
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
                        ["name"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" }
                    },
                    Required = ["name"] // Only name is required
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            UseNullableReferenceTypes = true 
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Required field should not be nullable
        Assert.Contains("public string Name { get; set; }", userDto);
        
        // Non-required fields should be nullable
        Assert.Contains("public int? Id { get; set; }", userDto);
        Assert.Contains("public string? Email { get; set; }", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesRequiredKeyword_WhenEnabled()
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
                        ["name"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" }
                    },
                    Required = ["name", "id"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            UseRequiredKeyword = true,
            UseNullableReferenceTypes = true 
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Required fields should use 'required' keyword instead of [Required] attribute
        Assert.Contains("public required int Id { get; set; }", userDto);
        Assert.Contains("public required string Name { get; set; }", userDto);
        
        // Non-required field should be nullable
        Assert.Contains("public string? Email { get; set; }", userDto);
        
        // Should not contain [Required] attributes
        Assert.DoesNotContain("[Required]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesRecords_WhenEnabled()
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
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            GenerateRecords = true,
            UseNullableReferenceTypes = true,
            UseRequiredKeyword = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Should generate a record instead of a class
        Assert.Contains("public partial record User(", userDto);
        Assert.DoesNotContain("public partial class User", userDto);
        
        // Should use required keyword for required fields
        Assert.Contains("required string Name", userDto);
        
        // Should use nullable types for non-required fields
        Assert.Contains("int? Id", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesComplexRecordWithAttributes_WhenEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Product"] = new Schema
                {
                    Type = "object",
                    Description = "A product object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["product_id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string", MaxLength = 100 },
                        ["description"] = new Schema { Type = "string", MinLength = 10 }
                    },
                    Required = ["product_id", "name"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            GenerateRecords = true,
            UseNullableReferenceTypes = true,
            UseRequiredKeyword = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var productDto = result.DtoClasses["Product"];
        
        // Should be a record
        Assert.Contains("public partial record Product(", productDto);
        
        // Should have JSON property mapping
        Assert.Contains("[JsonProperty(\"product_id\")]", productDto);
        Assert.Contains("[JsonProperty(\"name\")]", productDto);
        Assert.Contains("[JsonProperty(\"description\")]", productDto);
        
        // Should have validation attributes
        Assert.Contains("[MaxLength(100)]", productDto);
        Assert.Contains("[MinLength(10)]", productDto);
        
        // Should use required keyword appropriately
        Assert.Contains("required string ProductId", productDto);
        Assert.Contains("required string Name", productDto);
        Assert.Contains("string? Description", productDto);
    }

    [Fact]
    public void CodeGenerator_BackwardCompatibility_WhenFeaturesDisabled()
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
                    Required = ["name"]
                }
            }
        };

        // Use default options (all features disabled)
        var generator = new CodeGenerator(); 

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Should generate traditional class
        Assert.Contains("public partial class User", userDto);
        
        // Should use [Required] attribute
        Assert.Contains("[Required]", userDto);
        
        // Should not use nullable reference types
        Assert.Contains("public int Id { get; set; }", userDto);
        Assert.Contains("public string Name { get; set; }", userDto);
        
        // Should not use required keyword
        Assert.DoesNotContain("required", userDto);
    }
}