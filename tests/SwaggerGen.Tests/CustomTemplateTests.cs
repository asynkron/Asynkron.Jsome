using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class CustomTemplateTests
{
    [Fact]
    public void CustomTemplateFiles_WhenSpecified_OnlyGeneratesFromSpecifiedTemplates()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        var options = new CodeGenerationOptions
        {
            CustomTemplateFiles = new List<string> { "DTO.hbs" },
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        Assert.NotEmpty(result.DtoClasses);
        Assert.Empty(result.Validators); // Should not generate validators
        Assert.Empty(result.EnumTypes);
        Assert.Empty(result.ConstantClasses);
        Assert.Empty(result.CustomTemplateOutput);
    }

    [Fact]
    public void CustomTemplateFiles_WithCustomTemplate_GeneratesCustomOutput()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        
        // Create a simple custom template
        var customTemplatePath = Path.Combine(GetTemplatesPath(), "TestCustom.hbs");
        File.WriteAllText(customTemplatePath, "// Custom template for {{ClassName}}\nnamespace {{Namespace}};\npublic class Custom{{ClassName}} { }");
        
        try
        {
            var options = new CodeGenerationOptions
            {
                CustomTemplateFiles = new List<string> { "TestCustom.hbs" },
                TemplateDirectory = GetTemplatesPath()
            };

            // Act
            var generator = new CodeGenerator(options);
            var result = generator.GenerateCode(document);

            // Assert
            Assert.Empty(result.DtoClasses);
            Assert.Empty(result.Validators);
            Assert.NotEmpty(result.CustomTemplateOutput);
            
            var customOutput = result.CustomTemplateOutput.First().Value.Content;
            Assert.Contains("Custom template for TestClass", customOutput);
            Assert.Contains("public class CustomTestClass", customOutput);
        }
        finally
        {
            // Cleanup
            if (File.Exists(customTemplatePath))
                File.Delete(customTemplatePath);
        }
    }

    [Fact]
    public void CustomTemplateFiles_WithMixedTemplates_GeneratesBothStandardAndCustom()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        
        // Create a simple custom template
        var customTemplatePath = Path.Combine(GetTemplatesPath(), "TestMixed.hbs");
        File.WriteAllText(customTemplatePath, "// Mixed template\ntype {{ClassName}} = { }");
        
        try
        {
            var options = new CodeGenerationOptions
            {
                CustomTemplateFiles = new List<string> { "DTO.hbs", "TestMixed.hbs" },
                TemplateDirectory = GetTemplatesPath()
            };

            // Act
            var generator = new CodeGenerator(options);
            var result = generator.GenerateCode(document);

            // Assert
            Assert.NotEmpty(result.DtoClasses);
            Assert.Empty(result.Validators); // Validator template not specified
            Assert.NotEmpty(result.CustomTemplateOutput);
        }
        finally
        {
            // Cleanup
            if (File.Exists(customTemplatePath))
                File.Delete(customTemplatePath);
        }
    }

    [Fact]
    public void CodeGenerator_WithoutCustomTemplateFiles_UsesDefaultBehavior()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        var options = new CodeGenerationOptions
        {
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert - should generate all standard templates
        Assert.NotEmpty(result.DtoClasses);
        Assert.NotEmpty(result.Validators);
        Assert.Empty(result.CustomTemplateOutput); // No custom templates
    }

    [Fact]
    public void CustomTemplateFiles_WithFrontmatter_UsesCorrectFileExtension()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        
        // Create a template with frontmatter specifying file extension
        var customTemplatePath = Path.Combine(GetTemplatesPath(), "TestFSharp.hbs");
        var templateWithFrontmatter = @"---
extension: fs
description: F# test template
---
// F# template for {{ClassName}}
type {{ClassName}} = { }";
        
        File.WriteAllText(customTemplatePath, templateWithFrontmatter);
        
        try
        {
            var options = new CodeGenerationOptions
            {
                CustomTemplateFiles = new List<string> { "TestFSharp.hbs" },
                TemplateDirectory = GetTemplatesPath()
            };

            // Act
            var generator = new CodeGenerator(options);
            var result = generator.GenerateCode(document);

            // Assert
            Assert.NotEmpty(result.CustomTemplateOutput);
            
            var generatedFile = result.CustomTemplateOutput.First().Value;
            Assert.Equal("fs", generatedFile.Extension);
            Assert.Contains("// F# template for TestClass", generatedFile.Content);
            Assert.Contains("type TestClass = { }", generatedFile.Content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(customTemplatePath))
                File.Delete(customTemplatePath);
        }
    }

    [Fact]
    public void CustomTemplateFiles_WithoutFrontmatter_UsesDefaultExtension()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        
        // Create a template without frontmatter
        var customTemplatePath = Path.Combine(GetTemplatesPath(), "TestDefault.hbs");
        File.WriteAllText(customTemplatePath, "// Default template for {{ClassName}}\npublic class {{ClassName}} { }");
        
        try
        {
            var options = new CodeGenerationOptions
            {
                CustomTemplateFiles = new List<string> { "TestDefault.hbs" },
                TemplateDirectory = GetTemplatesPath()
            };

            // Act
            var generator = new CodeGenerator(options);
            var result = generator.GenerateCode(document);

            // Assert
            Assert.NotEmpty(result.CustomTemplateOutput);
            
            var generatedFile = result.CustomTemplateOutput.First().Value;
            Assert.Equal("cs", generatedFile.Extension); // Should use default "cs"
            Assert.Contains("// Default template for TestClass", generatedFile.Content);
        }
        finally
        {
            // Cleanup
            if (File.Exists(customTemplatePath))
                File.Delete(customTemplatePath);
        }
    }

    private SwaggerDocument CreateTestSwaggerDocument()
    {
        return new SwaggerDocument
        {
            Swagger = "2.0",
            Info = new Info { Title = "Test API", Version = "1.0" },
            Definitions = new Dictionary<string, Schema>
            {
                ["TestClass"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" },
                        ["name"] = new Schema { Type = "string" }
                    },
                    Required = new List<string> { "id", "name" }
                }
            }
        };
    }

    private string GetTemplatesPath()
    {
        // Use the same template discovery logic as CodeGenerator for consistency
        // Priority 1: Development-time fallback - walk up directory tree to find source templates
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null)
        {
            var srcPath = Path.Combine(currentDir.FullName, "src", "SwaggerGen", "Templates");
            if (Directory.Exists(srcPath))
            {
                return srcPath;
            }
            currentDir = currentDir.Parent;
        }

        // Priority 2: Check if templates are in the current directory (for different execution contexts)
        if (Directory.Exists("Templates"))
        {
            return Path.GetFullPath("Templates");
        }

        // Priority 3: Check relative to test assembly location
        var testAssemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (testAssemblyPath != null)
        {
            var testDir = new DirectoryInfo(testAssemblyPath);
            while (testDir != null)
            {
                var srcPath = Path.Combine(testDir.FullName, "src", "SwaggerGen", "Templates");
                if (Directory.Exists(srcPath))
                {
                    return srcPath;
                }
                testDir = testDir.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "Template files not found. Checked locations:\n" +
            $"  - Source directory search from: {Directory.GetCurrentDirectory()}\n" +
            $"  - Current directory: Templates\n" +
            $"  - Test assembly directory search from: {testAssemblyPath}\n\n" +
            "Ensure the test is running from a directory where src/SwaggerGen/Templates can be found by walking up the directory tree.");
    }
}