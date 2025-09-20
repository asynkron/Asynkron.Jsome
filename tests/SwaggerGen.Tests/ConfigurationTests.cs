using SwaggerGen.Configuration;
using Xunit;

namespace SwaggerGen.Tests;

public class ConfigurationTests
{
    [Fact]
    public void PropertyRule_IsIncluded_DefaultsToTrue()
    {
        // Arrange
        var rule = new PropertyRule();

        // Act & Assert
        Assert.True(rule.IsIncluded);
    }

    [Fact]
    public void PropertyRule_IsIncluded_RespectsExplicitValue()
    {
        // Arrange
        var rule = new PropertyRule { Include = false };

        // Act & Assert
        Assert.False(rule.IsIncluded);
    }

    [Fact]
    public void ModifierConfiguration_GetRule_ReturnsCorrectRule()
    {
        // Arrange
        var config = new ModifierConfiguration();
        var rule = new PropertyRule { Include = false };
        config.Rules["Order.Product.Name"] = rule;

        // Act
        var result = config.GetRule("Order.Product.Name");

        // Assert
        Assert.Same(rule, result);
    }

    [Fact]
    public void ModifierConfiguration_GetRule_ReturnsNullForNonExistentRule()
    {
        // Arrange
        var config = new ModifierConfiguration();

        // Act
        var result = config.GetRule("NonExistent.Path");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ModifierConfiguration_IsIncluded_DefaultsToTrue()
    {
        // Arrange
        var config = new ModifierConfiguration();

        // Act
        var result = config.IsIncluded("Any.Property.Path");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ModifierConfiguration_IsIncluded_RespectsRuleConfiguration()
    {
        // Arrange
        var config = new ModifierConfiguration();
        config.Rules["User.Password"] = new PropertyRule { Include = false };

        // Act
        var result = config.IsIncluded("User.Password");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ModifierConfiguration_GetChildRules_ReturnsMatchingRules()
    {
        // Arrange
        var config = new ModifierConfiguration();
        config.Rules["Order.Product.Name"] = new PropertyRule { Include = true };
        config.Rules["Order.Product.Price"] = new PropertyRule { Include = false };
        config.Rules["Order.Customer.Name"] = new PropertyRule { Include = true };
        config.Rules["User.Name"] = new PropertyRule { Include = true };

        // Act
        var result = config.GetChildRules("Order.Product");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Order.Product.Name", result.Keys);
        Assert.Contains("Order.Product.Price", result.Keys);
        Assert.DoesNotContain("Order.Customer.Name", result.Keys);
        Assert.DoesNotContain("User.Name", result.Keys);
    }

    [Fact]
    public void ConfigurationLoader_LoadFromYaml_ParsesValidYaml()
    {
        // Arrange
        var yaml = @"
global:
  namespace: Test.Generated
  generateEnumTypes: true
rules:
  'Order.Product.Name':
    include: true
    description: 'Product name'
    validation:
      required: true
      maxLength: 100
  'User.Password':
    include: false
";

        // Act
        var result = ConfigurationLoader.LoadFromYaml(yaml);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Global);
        Assert.Equal("Test.Generated", result.Global.Namespace);
        Assert.True(result.Global.GenerateEnumTypes);
        Assert.Equal(2, result.Rules.Count);
        
        var productRule = result.Rules["Order.Product.Name"];
        Assert.True(productRule.Include);
        Assert.Equal("Product name", productRule.Description);
        Assert.NotNull(productRule.Validation);
        Assert.True(productRule.Validation.Required);
        Assert.Equal(100, productRule.Validation.MaxLength);
        
        var passwordRule = result.Rules["User.Password"];
        Assert.False(passwordRule.Include);
    }

    [Fact]
    public void ConfigurationLoader_LoadFromJson_ParsesValidJson()
    {
        // Arrange
        var json = @"{
  ""global"": {
    ""namespace"": ""Test.Generated"",
    ""generateEnumTypes"": true
  },
  ""rules"": {
    ""Order.Product.Name"": {
      ""include"": true,
      ""description"": ""Product name"",
      ""validation"": {
        ""required"": true,
        ""maxLength"": 100
      }
    },
    ""User.Password"": {
      ""include"": false
    }
  }
}";

        // Act
        var result = ConfigurationLoader.LoadFromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Global);
        Assert.Equal("Test.Generated", result.Global.Namespace);
        Assert.True(result.Global.GenerateEnumTypes);
        Assert.Equal(2, result.Rules.Count);
        
        var productRule = result.Rules["Order.Product.Name"];
        Assert.True(productRule.Include);
        Assert.Equal("Product name", productRule.Description);
        Assert.NotNull(productRule.Validation);
        Assert.True(productRule.Validation.Required);
        Assert.Equal(100, productRule.Validation.MaxLength);
        
        var passwordRule = result.Rules["User.Password"];
        Assert.False(passwordRule.Include);
    }

    [Fact]
    public void ConfigurationLoader_LoadFromYaml_ReturnsEmptyConfigForEmptyString()
    {
        // Act
        var result = ConfigurationLoader.LoadFromYaml("");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Rules);
    }

    [Fact]
    public void ConfigurationLoader_LoadFromJson_ReturnsEmptyConfigForEmptyString()
    {
        // Act
        var result = ConfigurationLoader.LoadFromJson("");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Rules);
    }

    [Fact]
    public void ConfigurationLoader_ToYaml_GeneratesValidYaml()
    {
        // Arrange
        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                Namespace = "Test.Generated",
                GenerateEnumTypes = true
            }
        };
        config.Rules["Order.Product.Name"] = new PropertyRule
        {
            Include = true,
            Description = "Product name",
            Validation = new PropertyValidation
            {
                Required = true,
                MaxLength = 100
            }
        };

        // Act
        var yaml = ConfigurationLoader.ToYaml(config);

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("namespace: Test.Generated", yaml);
        Assert.Contains("generateEnumTypes: true", yaml);
        Assert.Contains("Order.Product.Name:", yaml);
        Assert.Contains("include: true", yaml);
        Assert.Contains("description: Product name", yaml);
        Assert.Contains("maxLength: 100", yaml);
    }

    [Fact]
    public void ConfigurationLoader_ToJson_GeneratesValidJson()
    {
        // Arrange
        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings
            {
                Namespace = "Test.Generated",
                GenerateEnumTypes = true
            }
        };
        config.Rules["Order.Product.Name"] = new PropertyRule
        {
            Include = true,
            Description = "Product name",
            Validation = new PropertyValidation
            {
                Required = true,
                MaxLength = 100
            }
        };

        // Act
        var json = ConfigurationLoader.ToJson(config);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"namespace\": \"Test.Generated\"", json);
        Assert.Contains("\"generateEnumTypes\": true", json);
        Assert.Contains("\"Order.Product.Name\":", json);
        Assert.Contains("\"include\": true", json);
        Assert.Contains("\"description\": \"Product name\"", json);
        Assert.Contains("\"maxLength\": 100", json);
    }

    [Fact]
    public async Task ConfigurationLoader_SaveAsync_CreatesValidFile()
    {
        // Arrange
        var config = new ModifierConfiguration
        {
            Global = new GlobalSettings { Namespace = "Test.Generated" }
        };
        config.Rules["Test.Property"] = new PropertyRule { Include = false };

        var tempFile = Path.GetTempFileName();
        var yamlFile = Path.ChangeExtension(tempFile, ".yaml");

        try
        {
            // Act
            await ConfigurationLoader.SaveAsync(config, yamlFile);

            // Assert
            Assert.True(File.Exists(yamlFile));
            var content = await File.ReadAllTextAsync(yamlFile);
            Assert.Contains("namespace: Test.Generated", content);
            Assert.Contains("Test.Property:", content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(yamlFile)) File.Delete(yamlFile);
        }
    }
}