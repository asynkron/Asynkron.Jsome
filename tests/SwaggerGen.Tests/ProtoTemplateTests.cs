using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class ProtoTemplateTests
{
    [Fact]
    public void GenerateProtoFiles_WhenEnabled_GeneratesProtoOutput()
    {
        // Arrange
        var document = CreateTestSwaggerDocument();
        var options = new CodeGenerationOptions
        {
            GenerateProtoFiles = true,
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        Assert.NotEmpty(result.CustomTemplateOutput);
        
        // Check that proto templates generated output
        var protoOutputs = result.CustomTemplateOutput.Where(kv => kv.Value.Extension == "proto");
        Assert.NotEmpty(protoOutputs);
        
        // Check specific proto message template output
        var protoMessage = protoOutputs.FirstOrDefault(kv => kv.Key.Contains("proto"));
        Assert.NotNull(protoMessage.Value);
        Assert.Contains("syntax = \"proto3\";", protoMessage.Value.Content);
        Assert.Contains("message TestClass", protoMessage.Value.Content);
    }

    [Fact]
    public void ProtoTemplate_GeneratesCorrectFieldNumbering()
    {
        // Arrange
        var document = CreateTestSwaggerDocumentWithMultipleProperties();
        var options = new CodeGenerationOptions
        {
            CustomTemplateFiles = new List<string> { "proto.hbs" },
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        var protoOutput = result.CustomTemplateOutput.First().Value.Content;
        
        Assert.Contains("= 1;", protoOutput);
        Assert.Contains("= 2;", protoOutput);
        Assert.Contains("= 3;", protoOutput);
    }

    [Fact]
    public void ProtoEnumTemplate_GeneratesValidProtoEnum()
    {
        // Arrange
        var document = CreateTestSwaggerDocumentWithEnum();
        var options = new CodeGenerationOptions
        {
            GenerateEnumTypes = true,
            CustomTemplateFiles = new List<string> { "proto.enum.hbs" },
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        var protoEnumOutput = result.CustomTemplateOutput.First().Value.Content;
        
        Assert.Contains("syntax = \"proto3\";", protoEnumOutput);
        Assert.Contains("enum ", protoEnumOutput);
        Assert.Contains("_UNSPECIFIED = 0;", protoEnumOutput); // Required in proto3
    }

    [Fact]
    public void ProtoStringEnumTemplate_GeneratesValidProtoEnum()
    {
        // Arrange
        var document = CreateTestSwaggerDocumentWithStringEnum();
        var options = new CodeGenerationOptions
        {
            GenerateEnumTypes = true,
            GenerateProtoFiles = true,
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        Assert.NotEmpty(result.ConstantClasses); // Should generate constants
        Assert.NotEmpty(result.CustomTemplateOutput); // Should generate proto templates
        
        // Find the proto string enum output
        var protoStringEnumOutput = result.CustomTemplateOutput
            .Where(kv => kv.Key.Contains("proto.string_enum"))
            .FirstOrDefault().Value?.Content;
        
        Assert.NotNull(protoStringEnumOutput);
        Assert.Contains("syntax = \"proto3\";", protoStringEnumOutput);
        Assert.Contains("enum ", protoStringEnumOutput);
        Assert.Contains("_UNSPECIFIED = 0;", protoStringEnumOutput);
        Assert.Contains("original value:", protoStringEnumOutput); // Comment about original string value
    }

    [Fact]
    public void HandlebarsHelpers_ConvertTypesCorrectly()
    {
        // Arrange
        var document = CreateTestSwaggerDocumentWithVariousTypes();
        var options = new CodeGenerationOptions
        {
            CustomTemplateFiles = new List<string> { "proto.hbs" },
            TemplateDirectory = GetTemplatesPath()
        };

        // Act
        var generator = new CodeGenerator(options);
        var result = generator.GenerateCode(document);

        // Assert
        var protoOutput = result.CustomTemplateOutput.First().Value.Content;
        
        // Check type conversions
        Assert.Contains("string ", protoOutput); // string -> string
        Assert.Contains("int32 ", protoOutput);  // int -> int32
        Assert.Contains("bool ", protoOutput);   // bool -> bool
    }

    private static SwaggerDocument CreateTestSwaggerDocument()
    {
        return new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestClass"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["Name"] = new Schema { Type = "string", Description = "Name property" },
                        ["Id"] = new Schema { Type = "integer", Format = "int32", Description = "ID property" }
                    },
                    Required = new List<string> { "Name", "Id" }
                }
            }
        };
    }

    private static SwaggerDocument CreateTestSwaggerDocumentWithMultipleProperties()
    {
        return new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestClass"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["Property1"] = new Schema { Type = "string" },
                        ["Property2"] = new Schema { Type = "integer" },
                        ["Property3"] = new Schema { Type = "boolean" }
                    }
                }
            }
        };
    }

    private static SwaggerDocument CreateTestSwaggerDocumentWithEnum()
    {
        return new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestEnum"] = new Schema
                {
                    Type = "integer",
                    Enum = new List<object> { 0, 1, 2 },
                    Description = "Test enum"
                }
            }
        };
    }

    private static SwaggerDocument CreateTestSwaggerDocumentWithStringEnum()
    {
        return new SwaggerDocument
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
                            Enum = new List<object> { "Value1", "Value2", "Value3" },
                            Description = "Test string enum property"
                        }
                    }
                }
            }
        };
    }

    private static SwaggerDocument CreateTestSwaggerDocumentWithVariousTypes()
    {
        return new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["TestClass"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["StringProp"] = new Schema { Type = "string" },
                        ["IntProp"] = new Schema { Type = "integer", Format = "int32" },
                        ["BoolProp"] = new Schema { Type = "boolean" }
                    }
                }
            }
        };
    }

    private static string GetTemplatesPath()
    {
        // Use the same logic as CodeGenerator to find templates directory
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

        // Priority 2: Check if templates are in the current directory
        if (Directory.Exists("Templates"))
        {
            return Path.GetFullPath("Templates");
        }

        // Priority 3: Check tool installation directory
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var templatesPath = Path.Combine(basePath, "Templates");
        
        if (Directory.Exists(templatesPath))
        {
            return templatesPath;
        }

        // Priority 4: Check contentFiles location for NuGet package
        var contentFilesPath = Path.Combine(basePath, "contentFiles", "any", "any", "Templates");
        if (Directory.Exists(contentFilesPath))
        {
            return contentFilesPath;
        }

        throw new DirectoryNotFoundException(
            "Template files not found. Checked locations:\n" +
            $"  - Source directory search from: {Directory.GetCurrentDirectory()}\n" +
            $"  - Current directory: Templates\n" +
            $"  - Tool directory: {templatesPath}\n" +
            $"  - ContentFiles directory: {contentFilesPath}\n\n" +
            "Unable to locate template files for testing.");
    }
}