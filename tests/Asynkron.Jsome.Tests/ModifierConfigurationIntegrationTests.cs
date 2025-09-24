using Asynkron.Jsome.CodeGeneration;
using Asynkron.Jsome.Configuration;
using Asynkron.Jsome.Models;
using Xunit;

namespace Asynkron.Jsome.Tests;

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
                    Required = ["id", "name"]
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
                    Required = ["name"]
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
        Assert.Contains(@"@""^[A-Za-z0-9\s]+$""", productValidator); // Properly formatted pattern string
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
                    Required = ["items"]
                }
            }
        };

        var generator = new CodeGenerator(); // No configuration

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Should use original validation format
        var validator = result.Validators["TestModel"];
        Assert.Contains(".Must(x => x.Count >= 1)", validator); // Properly formatted output
        Assert.Contains(".Must(x => x.Count <= 5)", validator);
        Assert.Contains("x.Distinct().Count() == x.Count", validator);
    }

    [Fact]
    public void CodeGenerator_AppliesTypeNamePrefix_WhenConfigured()
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
                        ["status"] = new Schema 
                        { 
                            Type = "integer",
                            Enum = [1, 2, 3]
                        },
                        ["category"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["premium", "basic"]
                        },
                        ["address"] = new Schema { Ref = "#/definitions/Address" }
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

        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                TypeNamePrefix = "Api",
                GenerateEnumTypes = true
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config,
            GenerateEnumTypes = true
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Check DTO class names have prefix
        Assert.True(result.DtoClasses.ContainsKey("User"));
        var userDto = result.DtoClasses["User"];
        Assert.Contains("public partial class ApiUser", userDto);
        
        Assert.True(result.DtoClasses.ContainsKey("Address"));
        var addressDto = result.DtoClasses["Address"];
        Assert.Contains("public partial class ApiAddress", addressDto);

        // Assert - Check enum names have prefix
        Assert.True(result.EnumTypes.Count > 0);
        var enumKey = result.EnumTypes.Keys.First(k => k.Contains("Status"));
        Assert.Contains("ApiUserStatus", enumKey);
        var enumCode = result.EnumTypes[enumKey];
        Assert.Contains("public enum ApiUserStatus", enumCode);

        // Assert - Check constants class names have prefix  
        Assert.True(result.ConstantClasses.Count > 0);
        var constantKey = result.ConstantClasses.Keys.First(k => k.Contains("Category"));
        Assert.Contains("ApiUserCategoryConstants", constantKey);
        var constantCode = result.ConstantClasses[constantKey];
        Assert.Contains("public static class ApiUserCategoryConstants", constantCode);

        // Assert - Check type references in DTO use prefix
        Assert.Contains("public ApiAddress Address { get; set; }", userDto);
        
        // Assert - Check validator class names still use original names
        var userValidator = result.Validators["User"];
        Assert.Contains("UserValidator : AbstractValidator<ApiUser>", userValidator);
    }

    [Fact]
    public void CodeGenerator_AppliesTypeNameSuffix_WhenConfigured()
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
                        ["name"] = new Schema { Type = "string" },
                        ["type"] = new Schema 
                        { 
                            Type = "integer",
                            Enum = [1, 2]
                        }
                    }
                }
            }
        };

        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                TypeNameSuffix = "DTO",
                GenerateEnumTypes = true
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config,
            GenerateEnumTypes = true
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Check DTO class names have suffix
        var productDto = result.DtoClasses["Product"];
        Assert.Contains("public partial class ProductDTO", productDto);

        // Assert - Check enum names have suffix
        var enumKey = result.EnumTypes.Keys.First();
        Assert.Contains("ProductTypeDTO", enumKey);
        var enumCode = result.EnumTypes[enumKey];
        Assert.Contains("public enum ProductTypeDTO", enumCode);
    }

    [Fact]
    public void CodeGenerator_AppliesBothPrefixAndSuffix_WhenConfigured()
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
                        ["priority"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["high", "low"]
                        }
                    }
                }
            }
        };

        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                TypeNamePrefix = "Api",
                TypeNameSuffix = "Model",
                GenerateEnumTypes = true
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config,                 
            GenerateEnumTypes = true
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Check DTO class names have both prefix and suffix
        var orderDto = result.DtoClasses["Order"];
        Assert.Contains("public partial class ApiOrderModel", orderDto);

        // Assert - Check constants class names have both prefix and suffix
        var constantKey = result.ConstantClasses.Keys.First();
        Assert.Contains("ApiOrderPriorityConstantsModel", constantKey);
        var constantCode = result.ConstantClasses[constantKey];
        Assert.Contains("public static class ApiOrderPriorityConstantsModel", constantCode);
    }

    [Fact]
    public void CodeGenerator_HandlesEmptyPrefixSuffix_WhenConfigured()
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
                        ["name"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                TypeNamePrefix = "",
                TypeNameSuffix = ""
            }
        };

        var options = new CodeGenerationOptions
        {
            ModifierConfiguration = config
        };

        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert - Should behave same as no configuration
        var userDto = result.DtoClasses["User"];
        Assert.Contains("public partial class User", userDto);
    }

    [Fact]
    public void CodeGenerator_ParsesYamlConfiguration_WithPrefixAndSuffix()
    {
        // Arrange - Create YAML config with prefix and suffix
        var yamlContent = @"
global:
  namespace: Example.Generated
  typeNamePrefix: Api
  typeNameSuffix: DTO
  generateEnumTypes: true

rules:
  User.password:
    include: false
";

        // Write YAML to temp file
        var tempFile = Path.GetTempFileName();
        var yamlFile = Path.ChangeExtension(tempFile, ".yaml");
        File.WriteAllText(yamlFile, yamlContent);

        try
        {
            // Load configuration from YAML
            var config = ConfigurationLoader.Load(yamlFile);
            
            // Verify configuration was parsed correctly
            Assert.NotNull(config.Global);
            Assert.Equal("Api", config.Global.TypeNamePrefix);
            Assert.Equal("DTO", config.Global.TypeNameSuffix);
            Assert.Equal("Example.Generated", config.Global.Namespace);
            Assert.True(config.Global.GenerateEnumTypes);
            
            // Test with a simple schema
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
                            ["password"] = new Schema { Type = "string" }
                        }
                    }
                }
            };

            var options = new CodeGenerationOptions
            {
                ModifierConfiguration = config
            };

            var generator = new CodeGenerator(options);

            // Act
            var result = generator.GenerateCode(document);

            // Assert - Type names should have prefix and suffix
            var userDto = result.DtoClasses["User"];
            Assert.Contains("namespace Example.Generated", userDto);
            Assert.Contains("public partial class ApiUserDTO", userDto);
            Assert.Contains("public string Id { get; set; }", userDto);
            // Password should be excluded
            Assert.DoesNotContain("password", userDto.ToLowerInvariant());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            if (File.Exists(yamlFile))
                File.Delete(yamlFile);
        }
    }
}