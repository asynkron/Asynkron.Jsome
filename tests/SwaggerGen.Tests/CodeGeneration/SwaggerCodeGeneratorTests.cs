using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests.CodeGeneration;

public class SwaggerCodeGeneratorTests
{
    [Fact]
    public void GenerateCode_SimpleSchema_GeneratesDto()
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
                        ["id"] = new Schema { Type = "integer", Format = "int32" },
                        ["name"] = new Schema { Type = "string" }
                    },
                    Required = ["name"]
                }
            }
        };

        var generator = new SwaggerCodeGenerator("Test.DTOs");

        // Act
        var result = generator.GenerateCode(document);

        // Assert
        Assert.Contains("TestModel.cs", result.Keys);
        Assert.Contains("TestModelValidator.cs", result.Keys);

        var dtoCode = result["TestModel.cs"];
        Assert.Contains("public class TestModel", dtoCode);
        Assert.Contains("public int? Id { get; set; }", dtoCode);
        Assert.Contains("[Required]", dtoCode);
        Assert.Contains("public string Name { get; set; }", dtoCode);

        var validatorCode = result["TestModelValidator.cs"];
        Assert.Contains("public class TestModelValidator", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Name).NotEmpty();", validatorCode);
    }

    [Fact]
    public void GenerateCode_AllOfSchema_MergesProperties()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BaseModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["name"] = new Schema { Type = "string" }
                    },
                    Required = ["name"]
                },
                ["ExtendedModel"] = new Schema
                {
                    Type = "object",
                    AllOf = new List<Schema>
                    {
                        new Schema { Ref = "#/definitions/BaseModel" },
                        new Schema
                        {
                            Properties = new Dictionary<string, Schema>
                            {
                                ["id"] = new Schema { Type = "integer", Format = "int64" }
                            },
                            Required = ["id"]
                        }
                    }
                }
            }
        };

        var generator = new SwaggerCodeGenerator("Test.DTOs");

        // Act
        var result = generator.GenerateCode(document);

        // Assert
        var extendedDtoCode = result["ExtendedModel.cs"];
        Assert.Contains("public class ExtendedModel", extendedDtoCode);
        Assert.Contains("public long Id { get; set; }", extendedDtoCode);
        // Should also contain properties from BaseModel reference
        // This test will help verify if the allOf reference resolution is working
    }

    [Fact]
    public void GenerateCode_StringWithValidation_AddsValidationAttributes()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["ValidationModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["email"] = new Schema 
                        { 
                            Type = "string",
                            MinLength = 5,
                            MaxLength = 100,
                            Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
                        },
                        ["age"] = new Schema
                        {
                            Type = "integer",
                            Minimum = 0,
                            Maximum = 120
                        }
                    },
                    Required = ["email"]
                }
            }
        };

        var generator = new SwaggerCodeGenerator("Test.DTOs");

        // Act
        var result = generator.GenerateCode(document);

        // Assert
        var dtoCode = result["ValidationModel.cs"];
        Assert.Contains("[Required]", dtoCode);
        Assert.Contains("[StringLength(100, MinimumLength = 5)]", dtoCode);
        Assert.Contains("[Range(0, 120)]", dtoCode);

        var validatorCode = result["ValidationModelValidator.cs"];
        Assert.Contains("RuleFor(x =&gt; x.Email).NotEmpty();", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Email).MinimumLength(5);", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Email).MaximumLength(100);", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Email).Matches(@&quot;^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$&quot;);", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Age).GreaterThanOrEqualTo(0);", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Age).LessThanOrEqualTo(120);", validatorCode);
    }

    [Fact]
    public void GenerateCode_ArrayProperty_GeneratesListType()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["ArrayModel"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["tags"] = new Schema
                        {
                            Type = "array",
                            Items = new Schema { Type = "string" },
                            MinItems = 1,
                            MaxItems = 10
                        }
                    }
                }
            }
        };

        var generator = new SwaggerCodeGenerator("Test.DTOs");

        // Act
        var result = generator.GenerateCode(document);

        // Assert
        var dtoCode = result["ArrayModel.cs"];
        Assert.Contains("public List&lt;string&gt;? Tags { get; set; }", dtoCode);

        var validatorCode = result["ArrayModelValidator.cs"];
        Assert.Contains("RuleFor(x =&gt; x.Tags).Must(x =&gt; x == null || x.Count &gt;= 1);", validatorCode);
        Assert.Contains("RuleFor(x =&gt; x.Tags).Must(x =&gt; x == null || x.Count &lt;= 10);", validatorCode);
    }
}