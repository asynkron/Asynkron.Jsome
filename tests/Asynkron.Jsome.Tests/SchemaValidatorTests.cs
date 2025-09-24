using Asynkron.Jsome.Configuration;
using Asynkron.Jsome.Models;
using Xunit;

namespace Asynkron.Jsome.Tests;

public class SchemaValidatorTests
{
    [Fact]
    public void ValidatePropertyPaths_ValidPath_ReturnsNoErrors()
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
                        ["name"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" }
                    }
                }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["User.name"] = new PropertyRule { Include = true };

        // Act
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatePropertyPaths_InvalidPath_ReturnsErrors()
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

        var config = new ModifierConfiguration();
        config.Rules["User.nonexistent"] = new PropertyRule { Include = false };

        // Act
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);

        // Assert
        Assert.Single(errors);
        Assert.Equal("User.nonexistent", errors[0].PropertyPath);
        Assert.Contains("was not found in the Swagger definition", errors[0].Message);
    }

    [Fact]
    public void ValidatePropertyPaths_RootDefinitionOnly_IsValid()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema { Type = "object" }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["User"] = new PropertyRule { Include = true };

        // Act
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatePropertyPaths_NonexistentRootDefinition_ReturnsError()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema { Type = "object" }
            }
        };

        var config = new ModifierConfiguration();
        config.Rules["Product.name"] = new PropertyRule { Include = true };

        // Act
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Product.name", errors[0].PropertyPath);
    }

    [Fact]
    public void ValidatePropertyPaths_WildcardPattern_IsSkipped()
    {
        // Arrange
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>()
        };

        var config = new ModifierConfiguration();
        config.Rules["*.Id"] = new PropertyRule { Include = true };

        // Act
        var errors = SchemaValidator.ValidatePropertyPaths(config, document);

        // Assert
        Assert.Empty(errors); // Wildcard patterns should be skipped
    }
}