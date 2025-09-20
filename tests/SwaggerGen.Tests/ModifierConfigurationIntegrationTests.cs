using SwaggerGen.CodeGeneration;
using SwaggerGen.Configuration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class ModifierConfigurationIntegrationTests
{
    [Fact]
    public void CodeGenerator_AppliesModifierConfiguration_ExcludesSpecifiedProperties()
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
                        ["id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string" },
                        ["password"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" }
                    },
                    Required = new List<string> { "id", "name" }
                }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["User.password"] = new PropertyRule { Include = false };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        Assert.Contains("public string Id { get; set; }", userDto);
        Assert.Contains("public string Name { get; set; }", userDto);
        Assert.Contains("public string Email { get; set; }", userDto);
        Assert.DoesNotContain("password", userDto.ToLowerInvariant());
    }

    [Fact]
    public void CodeGenerator_AppliesModifierConfiguration_CustomValidation()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Product"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string", MaxLength = 50 },
                        ["price"] = new Schema { Type = "number", Minimum = 0 }
                    },
                    Required = new List<string> { "name" }
                }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["Product.name"] = new PropertyRule
        {
            Include = true,
            Description = "Custom product name description",
            Validation = new PropertyValidation
            {
                Required = true,
                MaxLength = 100,
                Pattern = "^[A-Za-z0-9\\s]+$",
                Message = "Product name must contain only alphanumeric characters and spaces"
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var productValidator = result.Validators["Product"];
        Assert.Contains("MaximumLength(100)", productValidator);
        Assert.Contains("&quot;^[A-Za-z0-9\\s]+$&quot;", productValidator); // HTML encoded in template output
        Assert.Contains("Product name must contain only alphanumeric characters and spaces", productValidator);
    }

    [Fact]
    public void CodeGenerator_AppliesModifierConfiguration_CustomTypeMapping()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Order"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" },
                        ["createdDate"] = new Schema { Type = "string", Format = "date-time" }
                    }
                }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["Order.createdDate"] = new PropertyRule
        {
            Include = true,
            Type = "DateTime",
            Description = "Order creation timestamp"
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var orderDto = result.DtoClasses["Order"];
        Assert.Contains("public DateTime CreatedDate { get; set; }", orderDto);
        Assert.Contains("Order creation timestamp", orderDto);
    }

    [Fact]
    public void CodeGenerator_AppliesGlobalConfiguration()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                Namespace = "CustomNamespace.Generated"
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var dto = result.DtoClasses["TestModel"];
        Assert.Contains("namespace CustomNamespace.Generated", dto);
    }

    [Fact]
    public void CodeGenerator_ExcludesEntireClass_WhenClassPathExcluded()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["PublicModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" }
                    }
                },
                ["InternalModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["secret"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["InternalModel"] = new PropertyRule { Include = false };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Contains("PublicModel", result.DtoClasses.Keys);
        Assert.DoesNotContain("InternalModel", result.DtoClasses.Keys);
        Assert.Contains("PublicModel", result.Validators.Keys);
        Assert.DoesNotContain("InternalModel", result.Validators.Keys);
    }

    [Fact]
    public void CodeGenerator_BackwardCompatible_WhenNoConfigurationProvided()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["items"] = new Schema 
                        { 
                            Type = "array",
                            MinItems = 1,
                            MaxItems = 5,
                            UniqueItems = true
                        }
                    },
                    Required = new List<string> { "items" }
                }
            }
        };

        var generator = new CodeGenerator(); // No configuration

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Should use original validation format
        var validator = result.Validators["TestModel"];
        Assert.Contains(".Must(x =&gt; x.Count &gt;= 1)", validator); // HTML encoded in template output
        Assert.Contains(".Must(x =&gt; x.Count &lt;= 5)", validator);
        Assert.Contains("x.Distinct().Count() == x.Count", validator);
    }
}