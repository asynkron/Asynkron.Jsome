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
        Assert.Contains(".Must(x =&gt; x.Count &gt;= 1)", validator);
        Assert.Contains(".Must(x =&gt; x.Count &lt;= 5)", validator);
        Assert.Contains(".Must(x =&gt; x.Distinct().Count() == x.Count)", validator);
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
        Assert.Contains(".Must(x =&gt; x % 0.5 == 0)", validator);
        Assert.Contains(".Must(x =&gt; x % 5 == 0)", validator);
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
        Assert.Contains(".Must(x =&gt; new[] { &quot;active&quot;, &quot;inactive&quot;, &quot;pending&quot; }.Contains(x.ToString()))", validator);
        Assert.Contains(".Must(x =&gt; new[] { &quot;1&quot;, &quot;2&quot;, &quot;3&quot; }.Contains(x.ToString()))", validator);
        Assert.Contains("Must be one of: &quot;active&quot;, &quot;inactive&quot;, &quot;pending&quot;", validator);
        Assert.Contains("Must be one of: &quot;1&quot;, &quot;2&quot;, &quot;3&quot;", validator);
        
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
        Assert.Contains(".Must(x =&gt; new[] { &quot;active&quot;, &quot;inactive&quot; }.Contains(x))", validator);
        
        // Integer enum validation uses Enum.IsDefined
        Assert.Contains(".Must(x =&gt; Enum.IsDefined(typeof(ValidationTestPriority), x))", validator);
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
        Assert.Contains(".Must(x =&gt; new[] { &quot;active&quot;, &quot;inactive&quot; }.Contains(x.ToString()))", validator);
        Assert.Contains(".Must(x =&gt; new[] { &quot;1&quot;, &quot;2&quot;, &quot;3&quot; }.Contains(x.ToString()))", validator);
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
}