using SwaggerGen.CodeGeneration;
using Xunit;

namespace SwaggerGen.Tests.CodeGeneration;

public class TemplateEngineTests
{
    [Fact]
    public void RenderDtoClass_ValidModel_RendersCorrectly()
    {
        // Arrange
        var engine = new TemplateEngine();
        var model = new DtoGenerationModel
        {
            Namespace = "Test.DTOs",
            ClassName = "TestModel",
            Description = "A test model",
            Properties = new List<PropertyModel>
            {
                new PropertyModel
                {
                    Name = "Id",
                    Type = "int",
                    IsNullable = false,
                    Description = "The identifier",
                    ValidationAttributes = new List<string> { "[Required]" }
                },
                new PropertyModel
                {
                    Name = "Name",
                    Type = "string",
                    IsNullable = true,
                    Description = "The name"
                }
            }
        };

        // Act
        var result = engine.RenderDtoClass(model);

        // Assert
        Assert.Contains("namespace Test.DTOs;", result);
        Assert.Contains("public class TestModel", result);
        Assert.Contains("A test model", result);
        Assert.Contains("[Required]", result);
        Assert.Contains("public int Id { get; set; }", result);
        Assert.Contains("public string? Name { get; set; }", result);
    }

    [Fact]
    public void RenderValidator_ValidModel_RendersCorrectly()
    {
        // Arrange
        var engine = new TemplateEngine();
        var model = new ValidatorGenerationModel
        {
            Namespace = "Test.DTOs",
            ClassName = "TestModel",
            ValidationRules = new List<string>
            {
                "RuleFor(x => x.Id).NotEmpty();",
                "RuleFor(x => x.Name).MaximumLength(100);"
            }
        };

        // Act
        var result = engine.RenderValidator(model);

        // Assert
        Assert.Contains("namespace Test.DTOs.Validators;", result);
        Assert.Contains("public class TestModelValidator : AbstractValidator<TestModel>", result);
        Assert.Contains("RuleFor(x =&gt; x.Id).NotEmpty();", result);
        Assert.Contains("RuleFor(x =&gt; x.Name).MaximumLength(100);", result);
    }
}