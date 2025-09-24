using Asynkron.Jsome.CodeGeneration;
using Asynkron.Jsome.Models;
using Xunit;

namespace Asynkron.Jsome.Tests;

public class SystemTextJsonTests
{
    [Fact]
    public void CodeGenerator_GeneratesSystemTextJsonAttributes_WhenEnabled()
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
                        ["name"] = new Schema { Type = "string", MaxLength = 50, MinLength = 1 },
                        ["email"] = new Schema { Type = "string", MaxLength = 100 }
                    },
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        Assert.True(result.DtoClasses.ContainsKey("User"));
        
        var userDto = result.DtoClasses["User"];
        
        // Should use System.Text.Json imports
        Assert.Contains("using System.Text.Json.Serialization;", userDto);
        Assert.DoesNotContain("using Newtonsoft.Json;", userDto);
        
        // Should use JsonPropertyName instead of JsonProperty
        Assert.Contains("[JsonPropertyName(\"name\")]", userDto);
        Assert.DoesNotContain("[JsonProperty(", userDto);
        
        // Should use JsonIgnore with conditions
        Assert.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.Never)]", userDto); // For required field
        Assert.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]", userDto); // For optional field
        
        // Should use enhanced Required attribute for required fields
        Assert.Contains("[Required(AllowEmptyStrings = false)]", userDto);
        
        // Should use StringLength instead of separate MaxLength/MinLength
        Assert.Contains("[StringLength(50, MinimumLength = 1)]", userDto);
        Assert.Contains("[StringLength(100)]", userDto); // Only max length
        
        // Should not contain old-style attributes
        Assert.DoesNotContain("[MaxLength(", userDto);
        Assert.DoesNotContain("[MinLength(", userDto);
        Assert.DoesNotContain("[Required]", userDto); // Should be enhanced version
    }

    [Fact]
    public void CodeGenerator_GeneratesRecordsWithPropertyAttributes_WhenSystemTextJsonEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user record",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string", MaxLength = 50, MinLength = 1 },
                        ["email"] = new Schema { Type = "string", MaxLength = 100 }
                    },
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true,
            GenerateRecords = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Should be a record
        Assert.Contains("public partial record User(", userDto);
        
        // Should have JsonConstructor attribute
        Assert.Contains("[method: JsonConstructor]", userDto);
        
        // Should use property: prefix for attributes
        Assert.Contains("[property: JsonPropertyName(\"name\")]", userDto);
        Assert.Contains("[property: JsonIgnore(Condition = JsonIgnoreCondition.Never)]", userDto);
        Assert.Contains("[property: Required(AllowEmptyStrings = false)]", userDto);
        Assert.Contains("[property: StringLength(50, MinimumLength = 1)]", userDto);
        Assert.Contains("[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]", userDto);
    }

    [Fact]
    public void CodeGenerator_UsesNewtonSoftJsonByDefault()
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
                        ["name"] = new Schema { Type = "string", MaxLength = 50, MinLength = 1 }
                    },
                    Required = ["name"]
                }
            }
        };

        var generator = new CodeGenerator(); // Default options

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Should use Newtonsoft.Json by default
        Assert.Contains("using Newtonsoft.Json;", userDto);
        Assert.DoesNotContain("using System.Text.Json.Serialization;", userDto);
        
        // Should use old-style attributes
        Assert.Contains("[JsonProperty(\"name\")]", userDto);
        Assert.Contains("[Required]", userDto); // Not enhanced
        Assert.Contains("[MaxLength(50)]", userDto);
        Assert.Contains("[MinLength(1)]", userDto);
        
        // Should not contain System.Text.Json attributes
        Assert.DoesNotContain("[JsonPropertyName(", userDto);
        Assert.DoesNotContain("[JsonIgnore(Condition", userDto);
        Assert.DoesNotContain("[StringLength(", userDto);
        Assert.DoesNotContain("AllowEmptyStrings = false", userDto);
    }

    [Fact]
    public void CodeGenerator_HandlesRequiredAndOptionalPropertiesCorrectly()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BootNotificationRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["chargePointVendor"] = new Schema { Type = "string", MaxLength = 20, MinLength = 1 },
                        ["chargePointModel"] = new Schema { Type = "string", MaxLength = 20, MinLength = 1 },
                        ["firmwareVersion"] = new Schema { Type = "string", MaxLength = 50, MinLength = 1 }
                    },
                    Required = ["chargePointVendor", "chargePointModel"]
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var dto = result.DtoClasses["BootNotificationRequest"];
        
        // Required properties should have JsonIgnore Never condition
        var requiredPropertyLines = dto.Split('\n')
            .Where(line => line.Contains("ChargePointVendor") || line.Contains("ChargePointModel"))
            .ToArray();
            
        // Check that required properties have the Never condition
        Assert.True(dto.Contains("ChargePointVendor") && dto.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.Never)]"));
        Assert.True(dto.Contains("ChargePointModel") && dto.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.Never)]"));
        
        // Optional properties should have WhenWritingDefault condition
        Assert.True(dto.Contains("FirmwareVersion") && dto.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]"));
        
        // Required properties should have enhanced Required attribute
        Assert.Contains("[Required(AllowEmptyStrings = false)]", dto);
        
        // All properties should use StringLength
        Assert.Contains("[StringLength(20, MinimumLength = 1)]", dto);
        Assert.Contains("[StringLength(50, MinimumLength = 1)]", dto);
    }

    [Fact]
    public void CodeGenerator_HandlesModernCSharpFeaturesWithSystemTextJson()
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
                        ["name"] = new Schema { Type = "string", MaxLength = 50 },
                        ["email"] = new Schema { Type = "string", MaxLength = 100 }
                    },
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true,
            UseNullableReferenceTypes = true,
            UseRequiredKeyword = true,
            GenerateRecords = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Should use required keyword instead of Required attribute for required properties
        Assert.Contains("required string Name", userDto);
        Assert.DoesNotContain("[property: Required(AllowEmptyStrings = false)]", userDto);
        
        // Should use nullable types for optional properties
        Assert.Contains("string? Email", userDto);
        
        // Should still have JsonIgnore conditions
        Assert.Contains("[property: JsonIgnore(Condition = JsonIgnoreCondition.Never)]", userDto);
        Assert.Contains("[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]", userDto);
    }

    [Fact]
    public void CodeGenerator_HandlesOnlyMaxLengthCorrectly()
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
                        ["description"] = new Schema { Type = "string", MaxLength = 255 } // Only max, no min
                    }
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Should use StringLength with only max length (no MinimumLength parameter)
        Assert.Contains("[StringLength(255)]", userDto);
        Assert.DoesNotContain("MinimumLength", userDto);
    }

    [Fact]
    public void CodeGenerator_HandlesOnlyMinLengthCorrectly()
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
                        ["password"] = new Schema { Type = "string", MinLength = 8 } // Only min, no max
                    }
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Should use StringLength with int.MaxValue for max and MinimumLength parameter
        Assert.Contains("[StringLength(int.MaxValue, MinimumLength = 8)]", userDto);
    }
}