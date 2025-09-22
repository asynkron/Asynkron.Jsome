using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;
using System.IO;

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
                    Required = ["name"]
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
        Assert.Contains("public partial class User", userDto);
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
                    Required = ["email"]
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
                    Required = ["score"]
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
        Assert.Contains(@".Matches(@""^\d{3}-\d{2}-\d{4}$"")", validator);
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

    [Fact]
    public void CodeGenerator_ProcessesStripeSwaggerSuccessfully()
    {
        // Arrange - Load the actual Stripe Swagger file
        var currentDir = Directory.GetCurrentDirectory();
        var stripeFilePath = Path.Combine(currentDir, "..", "..", "..", "..", "..", "testdata", "stripe-swagger.json");
        
        // Skip test if file doesn't exist (for different test environments)
        if (!File.Exists(stripeFilePath))
        {
            // Try alternative path
            stripeFilePath = Path.Combine(currentDir, "testdata", "stripe-swagger.json");
            if (!File.Exists(stripeFilePath))
            {
                return; // Skip test if file not found
            }
        }

        var json = File.ReadAllText(stripeFilePath);
        var document = SwaggerParser.Parse(json);
        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Stripe.Generated");

        // Assert
        Assert.True(result.DtoClasses.Count > 0, "Should generate DTO classes");
        Assert.True(result.Validators.Count > 0, "Should generate validators");
        
        // Check that Customer class exists and has Address property correctly typed
        Assert.True(result.DtoClasses.ContainsKey("Customer"), "Should contain Customer class");
        var customerDto = result.DtoClasses["Customer"];
        
        // Verify that $ref properties are properly resolved
        Assert.Contains("public Address Address { get; set; }", customerDto);
        Assert.Contains("namespace Stripe.Generated", customerDto);
        
        // Check that validators are generated correctly
        Assert.True(result.Validators.ContainsKey("Customer"), "Should contain Customer validator");
        var validator = result.Validators["Customer"];
        Assert.Contains("CustomerValidator : AbstractValidator<Customer>", validator);
    }

    [Fact]
    public void CodeGenerator_HandlesArrayValidationConstraints()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["ArrayTest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["tags"] = new Schema 
                        { 
                            Type = "array",
                            Items = new Schema { Type = "string" },
                            MinItems = 1,
                            MaxItems = 5,
                            UniqueItems = true
                        },
                        ["numbers"] = new Schema
                        {
                            Type = "array",
                            Items = new Schema { Type = "integer" },
                            MinItems = 2
                        }
                    },
                    Required = ["tags"]
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var validator = result.Validators["ArrayTest"];
        Assert.Contains(".Must(x => x.Count >= 1)", validator);
        Assert.Contains(".Must(x => x.Count <= 5)", validator);
        Assert.Contains(".Must(x => x.Distinct().Count() == x.Count)", validator);
        Assert.Contains("Must contain at least 1 items", validator);
        Assert.Contains("Must contain at most 5 items", validator);
        Assert.Contains("All items must be unique", validator);
    }

    [Fact]
    public void CodeGenerator_HandlesNumberValidationConstraints()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["NumberTest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["percentage"] = new Schema 
                        { 
                            Type = "number",
                            MultipleOf = 0.5m,
                            Minimum = 0,
                            Maximum = 100
                        },
                        ["count"] = new Schema
                        {
                            Type = "integer",
                            MultipleOf = 5
                        }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var validator = result.Validators["NumberTest"];
        Assert.Contains(".Must(x => x % 0.5 == 0)", validator);
        Assert.Contains(".Must(x => x % 5 == 0)", validator);
        Assert.Contains("Must be a multiple of 0.5", validator);
        Assert.Contains("Must be a multiple of 5", validator);
    }

    [Fact]
    public void CodeGenerator_HandlesEnumValidationConstraints()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["EnumTest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["active", "inactive", "pending"]
                        },
                        ["priority"] = new Schema
                        {
                            Type = "integer",
                            Enum = [1, 2, 3]
                        }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var validator = result.Validators["EnumTest"];
        Assert.Contains(@".Must(x => new[] { ""active"", ""inactive"", ""pending"" }.Contains(x.ToString()))", validator);
        Assert.Contains(@".Must(x => new[] { ""1"", ""2"", ""3"" }.Contains(x.ToString()))", validator);
        Assert.Contains(@"Must be one of: ""active"", ""inactive"", ""pending""", validator);
        Assert.Contains(@"Must be one of: ""1"", ""2"", ""3""", validator);
        
        // Check that enum values are documented in DTO
        var dto = result.DtoClasses["EnumTest"];
        Assert.Contains("Allowed values: active, inactive, pending", dto);
        Assert.Contains("Allowed values: 1, 2, 3", dto);
    }

    [Fact]
    public void CodeGenerator_GeneratesEnumsAndConstants_WhenGenerateEnumTypesEnabled()
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
                        ["stringEnum"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["option1", "option2", "option3"]
                        },
                        ["integerEnum"] = new Schema
                        {
                            Type = "integer",
                            Enum = [10, 20, 30]
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { GenerateEnumTypes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        Assert.Single(result.EnumTypes);
        Assert.Single(result.ConstantClasses);
        
        // Check enum generation
        var enumCode = result.EnumTypes.Values.First();
        Assert.Contains("public enum TestModelIntegerEnum", enumCode);
        Assert.Contains("Value10 = 10", enumCode);
        Assert.Contains("Value20 = 20", enumCode);
        Assert.Contains("Value30 = 30", enumCode);
        
        // Check constants generation
        var constantsCode = result.ConstantClasses.Values.First();
        Assert.Contains("public static class TestModelStringEnumConstants", constantsCode);
        Assert.Contains("public const string OPTION1 = \"option1\"", constantsCode);
        Assert.Contains("public const string OPTION2 = \"option2\"", constantsCode);
        Assert.Contains("public const string OPTION3 = \"option3\"", constantsCode);
        
        // Check DTO uses correct types
        var dto = result.DtoClasses["TestModel"];
        Assert.Contains("public string StringEnum { get; set; }", dto);
        Assert.Contains("public TestModelIntegerEnum IntegerEnum { get; set; }", dto);
        
        // Check documentation references
        Assert.Contains("Uses enum type: <see cref=\"TestModelIntegerEnum\"/>", dto);
        Assert.Contains("Allowed values defined in: <see cref=\"TestModelStringEnumConstants\"/>", dto);
    }

    [Fact]
    public void CodeGenerator_GeneratesImprovedValidation_WhenGenerateEnumTypesEnabled()
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
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["active", "inactive"]
                        },
                        ["priority"] = new Schema
                        {
                            Type = "integer",
                            Enum = [1, 2, 3]
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { GenerateEnumTypes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var validator = result.Validators["ValidationTest"];
        
        // String enum validation uses direct string comparison
        Assert.Contains(@".Must(x => new[] { ""active"", ""inactive"" }.Contains(x))", validator);
        
        // Integer enum validation uses Enum.IsDefined
        Assert.Contains(".Must(x => Enum.IsDefined(typeof(ValidationTestPriority), x))", validator);
        Assert.Contains("Must be a valid ValidationTestPriority value", validator);
    }

    [Fact]
    public void CodeGenerator_BackwardCompatible_WhenGenerateEnumTypesDisabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BackwardTest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = ["active", "inactive"]
                        },
                        ["priority"] = new Schema
                        {
                            Type = "integer",
                            Enum = [1, 2, 3]
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { GenerateEnumTypes = false };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Empty(result.EnumTypes);
        Assert.Empty(result.ConstantClasses);
        
        var dto = result.DtoClasses["BackwardTest"];
        Assert.Contains("public string Status { get; set; }", dto);
        Assert.Contains("public int Priority { get; set; }", dto);
        
        // Should use legacy documentation
        Assert.Contains("Allowed values: active, inactive", dto);
        Assert.Contains("Allowed values: 1, 2, 3", dto);
        
        // Should use legacy validation
        var validator = result.Validators["BackwardTest"];
        Assert.Contains(@".Must(x => new[] { ""active"", ""inactive"" }.Contains(x.ToString()))", validator);
        Assert.Contains(@".Must(x => new[] { ""1"", ""2"", ""3"" }.Contains(x.ToString()))", validator);
    }

    [Fact]
    public void CodeGenerator_HandlesComplexEnumNames()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["ComplexNaming"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["kebab-case-field"] = new Schema
                        {
                            Type = "integer",
                            Enum = [1, 2]
                        },
                        ["snake_case_field"] = new Schema
                        {
                            Type = "string",
                            Enum = ["value-with-dash", "value_with_underscore", "123-numeric"]
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { GenerateEnumTypes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        // Check that enum names are properly converted to PascalCase
        Assert.Contains("public enum ComplexNamingKebabCaseField", result.EnumTypes.Values.First());
        Assert.Contains("public static class ComplexNamingSnakeCaseFieldConstants", result.ConstantClasses.Values.First());
        
        // Check that enum value names handle special characters
        var enumCode = result.EnumTypes.Values.First();
        Assert.Contains("Value1 = 1", enumCode);
        Assert.Contains("Value2 = 2", enumCode);
        
        // Check that constant names handle special characters  
        var constantsCode = result.ConstantClasses.Values.First();
        Assert.Contains("VALUE_WITH_DASH", constantsCode);
        Assert.Contains("VALUE_WITH_UNDERSCORE", constantsCode);
        Assert.Contains("VALUE_123_NUMERIC", constantsCode);
    }

    [Fact]
    public void CodeGenerator_GeneratesPartialClasses_ForExtensibility()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Customer"] = new Schema
                {
                    Type = "object",
                    Description = "A customer object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string" },
                    },
                    Required = ["name"]
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var customerDto = result.DtoClasses["Customer"];
        
        // Verify it's a partial class to allow user extensions
        Assert.Contains("public partial class Customer", customerDto);
        Assert.DoesNotContain("public class Customer", customerDto.Replace("public partial class Customer", ""));
    }

    [Fact]
    public void CodeGenerator_GeneratesPartialRecords_ForExtensibility()
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
                        ["id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string" },
                    },
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions { GenerateRecords = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var productDto = result.DtoClasses["Product"];
        
        // Verify it's a partial record to allow user extensions
        Assert.Contains("public partial record Product(", productDto);
        Assert.DoesNotContain("public record Product(", productDto.Replace("public partial record Product(", ""));
    }

    [Fact]
    public void CodeGenerator_GeneratesSwashbuckleAttributes_WhenEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user entity",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema 
                        { 
                            Type = "integer",
                            Format = "int64", 
                            Description = "The unique identifier",
                            Example = 12345
                        },
                        ["name"] = new Schema 
                        { 
                            Type = "string",
                            Description = "The user's full name",
                            Example = "John Doe"
                        },
                        ["email"] = new Schema 
                        { 
                            Type = "string",
                            Format = "email",
                            Description = "The user's email address",
                            Example = "john.doe@example.com"
                        },
                        ["isActive"] = new Schema
                        {
                            Type = "boolean",
                            Description = "Whether the user is active",
                            Example = true
                        }
                    },
                    Required = ["id", "name"]
                }
            }
        };

        var options = new CodeGenerationOptions { UseSwashbuckleAttributes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Verify Swashbuckle using statement
        Assert.Contains("using Swashbuckle.AspNetCore.Annotations;", userDto);
        
        // Verify SwaggerSchema attributes with description and format
        Assert.Contains("[SwaggerSchema(Description = \"The unique identifier\", Format = \"int64\")]", userDto);
        Assert.Contains("[SwaggerSchema(Description = \"The user's full name\")]", userDto);
        Assert.Contains("[SwaggerSchema(Description = \"The user's email address\", Format = \"email\")]", userDto);
        Assert.Contains("[SwaggerSchema(Description = \"Whether the user is active\")]", userDto);
        
        // Verify SwaggerExampleValue attributes
        Assert.Contains("[SwaggerExampleValue(12345)]", userDto);
        Assert.Contains("[SwaggerExampleValue(\"John Doe\")]", userDto);
        Assert.Contains("[SwaggerExampleValue(\"john.doe@example.com\")]", userDto);
        Assert.Contains("[SwaggerExampleValue(true)]", userDto);
        
        // Verify DataAnnotations are still present
        Assert.Contains("[Required]", userDto);
        Assert.Contains("[JsonProperty(\"id\")]", userDto);
        Assert.Contains("[JsonProperty(\"name\")]", userDto);
    }

    [Fact]
    public void CodeGenerator_DoesNotGenerateSwashbuckleAttributes_WhenDisabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user entity",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema 
                        { 
                            Type = "integer",
                            Format = "int64", 
                            Description = "The unique identifier",
                            Example = 12345
                        },
                        ["name"] = new Schema 
                        { 
                            Type = "string",
                            Description = "The user's full name",
                            Example = "John Doe"
                        }
                    },
                    Required = ["id", "name"]
                }
            }
        };

        var options = new CodeGenerationOptions { UseSwashbuckleAttributes = false };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Verify Swashbuckle using statement is NOT present
        Assert.DoesNotContain("using Swashbuckle.AspNetCore.Annotations;", userDto);
        
        // Verify SwaggerSchema attributes are NOT present
        Assert.DoesNotContain("SwaggerSchema", userDto);
        Assert.DoesNotContain("SwaggerExampleValue", userDto);
        
        // Verify DataAnnotations are still present
        Assert.Contains("[Required]", userDto);
        Assert.Contains("[JsonProperty(\"id\")]", userDto);
        Assert.Contains("[JsonProperty(\"name\")]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesSwashbuckleAttributes_WithFormatOnly()
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
                        ["price"] = new Schema 
                        { 
                            Type = "number",
                            Format = "decimal"
                        },
                        ["code"] = new Schema 
                        { 
                            Type = "string",
                            Format = "uuid"
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { UseSwashbuckleAttributes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var productDto = result.DtoClasses["Product"];
        
        // Verify SwaggerSchema attributes with format only (no description)
        Assert.Contains("[SwaggerSchema(Format = \"decimal\")]", productDto);
        Assert.Contains("[SwaggerSchema(Format = \"uuid\")]", productDto);
    }

    [Fact]  
    public void CodeGenerator_GeneratesSwashbuckleAttributes_WithExampleOnly()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestData"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["score"] = new Schema 
                        { 
                            Type = "number",
                            Example = 95.5
                        },
                        ["category"] = new Schema 
                        { 
                            Type = "string",
                            Example = "premium"
                        },
                        ["enabled"] = new Schema
                        {
                            Type = "boolean",
                            Example = false
                        }
                    }
                }
            }
        };

        var options = new CodeGenerationOptions { UseSwashbuckleAttributes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var testDataDto = result.DtoClasses["TestData"];
        
        // Verify SwaggerExampleValue attributes with different types
        Assert.Contains("[SwaggerExampleValue(95.5)]", testDataDto);
        Assert.Contains("[SwaggerExampleValue(\"premium\")]", testDataDto);
        Assert.Contains("[SwaggerExampleValue(false)]", testDataDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesSwashbuckleAttributes_WithSystemTextJson()
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
                        ["id"] = new Schema 
                        { 
                            Type = "integer",
                            Description = "User ID",
                            Example = 123
                        },
                        ["name"] = new Schema 
                        { 
                            Type = "string",
                            Description = "User name",
                            Example = "John"
                        }
                    },
                    Required = ["id"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            UseSwashbuckleAttributes = true,
            UseSystemTextJson = true 
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        var userDto = result.DtoClasses["User"];
        
        // Verify both System.Text.Json and Swashbuckle using statements
        Assert.Contains("using System.Text.Json.Serialization;", userDto);
        Assert.Contains("using Swashbuckle.AspNetCore.Annotations;", userDto);
        
        // Verify System.Text.Json attributes
        Assert.Contains("[JsonPropertyName(\"id\")]", userDto);
        Assert.DoesNotContain("[JsonProperty(\"id\")]", userDto);
        
        // Verify Swashbuckle attributes are still present
        Assert.Contains("[SwaggerSchema(Description = \"User ID\")]", userDto);
        Assert.Contains("[SwaggerExampleValue(123)]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesRecordsWithSwashbuckleAttributes_WhenEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user entity",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema 
                        { 
                            Type = "integer",
                            Format = "int64", 
                            Description = "The unique identifier",
                            Example = 12345
                        },
                        ["name"] = new Schema 
                        { 
                            Type = "string",
                            Description = "The user's full name",
                            Example = "John Doe"
                        },
                        ["email"] = new Schema 
                        { 
                            Type = "string",
                            Format = "email",
                            Description = "The user's email address",
                            Example = "john.doe@example.com"
                        }
                    },
                    Required = ["id", "name"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            UseSwashbuckleAttributes = true,
            GenerateRecords = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Should be a record
        Assert.Contains("public partial record User(", userDto);
        
        // Verify Swashbuckle using statement
        Assert.Contains("using Swashbuckle.AspNetCore.Annotations;", userDto);
        
        // Verify SwaggerSchema attributes with description and format
        Assert.Contains("[SwaggerSchema(Description = \"The unique identifier\", Format = \"int64\")]", userDto);
        Assert.Contains("[SwaggerSchema(Description = \"The user's full name\")]", userDto);
        Assert.Contains("[SwaggerSchema(Description = \"The user's email address\", Format = \"email\")]", userDto);
        
        // Verify SwaggerExampleValue attributes
        Assert.Contains("[SwaggerExampleValue(12345)]", userDto);
        Assert.Contains("[SwaggerExampleValue(\"John Doe\")]", userDto);
        Assert.Contains("[SwaggerExampleValue(\"john.doe@example.com\")]", userDto);
        
        // Verify JSON property mapping for records
        Assert.Contains("[JsonProperty(\"id\")]", userDto);
        Assert.Contains("[JsonProperty(\"name\")]", userDto);
        Assert.Contains("[JsonProperty(\"email\")]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesRecordsWithSwashbuckleAttributes_AndSystemTextJson_WhenEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user entity",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema 
                        { 
                            Type = "integer",
                            Format = "int64", 
                            Description = "The unique identifier",
                            Example = 12345
                        },
                        ["name"] = new Schema 
                        { 
                            Type = "string",
                            Description = "The user's full name",
                            Example = "John Doe"
                        },
                        ["email"] = new Schema 
                        { 
                            Type = "string",
                            Format = "email",
                            Description = "The user's email address",
                            Example = "john.doe@example.com"
                        }
                    },
                    Required = ["id", "name"]
                }
            }
        };

        var options = new CodeGenerationOptions 
        { 
            UseSwashbuckleAttributes = true,
            GenerateRecords = true,
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var userDto = result.DtoClasses["User"];
        
        // Should be a record
        Assert.Contains("public partial record User(", userDto);
        
        // Should have JsonConstructor attribute
        Assert.Contains("[method: JsonConstructor]", userDto);
        
        // Verify both System.Text.Json and Swashbuckle using statements
        Assert.Contains("using System.Text.Json.Serialization;", userDto);
        Assert.Contains("using Swashbuckle.AspNetCore.Annotations;", userDto);
        
        // Verify SwaggerSchema attributes with property: prefix for SystemTextJson
        Assert.Contains("[property: SwaggerSchema(Description = \"The unique identifier\", Format = \"int64\")]", userDto);
        Assert.Contains("[property: SwaggerSchema(Description = \"The user's full name\")]", userDto);
        Assert.Contains("[property: SwaggerSchema(Description = \"The user's email address\", Format = \"email\")]", userDto);
        
        // Verify SwaggerExampleValue attributes with property: prefix
        Assert.Contains("[property: SwaggerExampleValue(12345)]", userDto);
        Assert.Contains("[property: SwaggerExampleValue(\"John Doe\")]", userDto);
        Assert.Contains("[property: SwaggerExampleValue(\"john.doe@example.com\")]", userDto);
        
        // Verify System.Text.Json property mapping for records
        Assert.Contains("[property: JsonPropertyName(\"id\")]", userDto);
        Assert.Contains("[property: JsonPropertyName(\"name\")]", userDto);
        Assert.Contains("[property: JsonPropertyName(\"email\")]", userDto);
    }

    [Fact]
    public void CodeGenerator_GeneratesClassLevelSwaggerSchema_WhenSwashbuckleEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Product"] = new Schema
                {
                    Type = "object",
                    Description = "Represents a product in the catalog",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" },
                        ["name"] = new Schema { Type = "string" }
                    },
                    Required = ["name"]
                }
            }
        };

        var options = new CodeGenerationOptions { UseSwashbuckleAttributes = true };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert
        Assert.Single(result.DtoClasses);
        var productDto = result.DtoClasses["Product"];
        
        // Should have class-level SwaggerSchema attribute with description
        Assert.Contains("[SwaggerSchema(Description = \"Represents a product in the catalog\")]", productDto);
        
        // Verify it's placed before the class declaration
        var lines = productDto.Split('\n');
        var swaggerSchemaLine = lines.FirstOrDefault(l => l.Contains("[SwaggerSchema(Description"));
        var classDeclarationLine = lines.FirstOrDefault(l => l.Contains("public partial class Product"));
        
        Assert.NotNull(swaggerSchemaLine);
        Assert.NotNull(classDeclarationLine);
        
        var swaggerSchemaIndex = Array.IndexOf(lines, swaggerSchemaLine);
        var classDeclarationIndex = Array.IndexOf(lines, classDeclarationLine);
        
        Assert.True(swaggerSchemaIndex < classDeclarationIndex, "SwaggerSchema attribute should appear before class declaration");
    }

    [Fact]
    public void CodeGenerator_GeneratesRecordLevelSwaggerSchema_WhenSwashbuckleEnabled()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Product"] = new Schema
                {
                    Type = "object",
                    Description = "Represents a product in the catalog",
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
            UseSwashbuckleAttributes = true,
            GenerateRecords = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "Test.Generated");

        // Assert  
        Assert.Single(result.DtoClasses);
        var productDto = result.DtoClasses["Product"];
        
        // Should have record-level SwaggerSchema attribute with description  
        Assert.Contains("[SwaggerSchema(Description = \"Represents a product in the catalog\")]", productDto);
        
        // Verify it's placed before the record declaration
        var lines = productDto.Split('\n');
        var swaggerSchemaLine = lines.FirstOrDefault(l => l.Contains("[SwaggerSchema(Description"));
        var recordDeclarationLine = lines.FirstOrDefault(l => l.Contains("public partial record Product("));
        
        Assert.NotNull(swaggerSchemaLine);
        Assert.NotNull(recordDeclarationLine);
        
        var swaggerSchemaIndex = Array.IndexOf(lines, swaggerSchemaLine);
        var recordDeclarationIndex = Array.IndexOf(lines, recordDeclarationLine);
        
        Assert.True(swaggerSchemaIndex < recordDeclarationIndex, "SwaggerSchema attribute should appear before record declaration");
    }
}