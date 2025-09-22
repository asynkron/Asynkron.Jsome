using SwaggerGen;
using SwaggerGen.Models;
using Xunit;
using System.IO;

namespace SwaggerGen.Tests;

public class JsonSchemaParserTests
{
    [Fact]
    public void ParseDirectory_WithValidSchemas_ShouldMergeSuccessfully()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test JSON Schema files
            var schema1 = """
            {
                "$schema": "http://json-schema.org/draft-04/schema#",
                "title": "TestRequest",
                "type": "object",
                "properties": {
                    "id": {
                        "type": "string",
                        "maxLength": 20
                    }
                },
                "additionalProperties": false,
                "required": ["id"]
            }
            """;

            var schema2 = """
            {
                "$schema": "http://json-schema.org/draft-04/schema#",
                "title": "TestResponse",
                "type": "object",
                "properties": {
                    "status": {
                        "type": "string",
                        "enum": ["success", "failure"]
                    }
                },
                "additionalProperties": false,
                "required": ["status"]
            }
            """;

            File.WriteAllText(Path.Combine(tempDir, "test1.json"), schema1);
            File.WriteAllText(Path.Combine(tempDir, "test2.json"), schema2);

            // Act
            var document = JsonSchemaParser.ParseDirectory(tempDir);

            // Assert
            Assert.NotNull(document);
            Assert.Equal("2.0", document.Swagger);
            Assert.Equal("Generated from JSON Schema Directory", document.Info.Title);
            Assert.Equal(2, document.Definitions.Count);
            Assert.True(document.Definitions.ContainsKey("TestRequest"));
            Assert.True(document.Definitions.ContainsKey("TestResponse"));
            
            var testRequest = document.Definitions["TestRequest"];
            Assert.Equal("object", testRequest.Type);
            Assert.Single(testRequest.Properties);
            Assert.True(testRequest.Properties.ContainsKey("id"));
            Assert.Contains("id", testRequest.Required);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ParseDirectory_WithInternalDefinitions_ShouldExtractThem()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test JSON Schema with internal definitions
            var schemaWithDefinitions = """
            {
                "$schema": "http://json-schema.org/draft-04/schema#",
                "title": "ComplexRequest",
                "definitions": {
                    "StatusEnum": {
                        "type": "string",
                        "enum": ["active", "inactive"]
                    }
                },
                "type": "object",
                "properties": {
                    "status": {
                        "$ref": "#/definitions/StatusEnum"
                    }
                },
                "required": ["status"]
            }
            """;

            File.WriteAllText(Path.Combine(tempDir, "complex.json"), schemaWithDefinitions);

            // Act
            var document = JsonSchemaParser.ParseDirectory(tempDir);

            // Assert
            Assert.NotNull(document);
            Assert.Equal(2, document.Definitions.Count); // ComplexRequest + StatusEnum
            Assert.True(document.Definitions.ContainsKey("ComplexRequest"));
            Assert.True(document.Definitions.ContainsKey("StatusEnum"));
            
            var statusEnum = document.Definitions["StatusEnum"];
            Assert.Equal("string", statusEnum.Type);
            Assert.Equal(2, statusEnum.Enum.Count);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ParseDirectory_WithConflictingDefinitions_ShouldThrowException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create two schemas with conflicting definitions for the same name
            var schema1 = """
            {
                "title": "TestSchema",
                "type": "object",
                "properties": {
                    "value": {"type": "string"}
                }
            }
            """;

            var schema2 = """
            {
                "title": "TestSchema", 
                "type": "object",
                "properties": {
                    "value": {"type": "integer"}
                }
            }
            """;

            File.WriteAllText(Path.Combine(tempDir, "test1.json"), schema1);
            File.WriteAllText(Path.Combine(tempDir, "test2.json"), schema2);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                JsonSchemaParser.ParseDirectory(tempDir));
            
            Assert.Contains("Conflicting schema definitions", exception.Message);
            Assert.Contains("TestSchema", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ParseDirectory_WithNonExistentDirectory_ShouldThrowException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), "NonExistent", Guid.NewGuid().ToString());

        // Act & Assert
        var exception = Assert.Throws<DirectoryNotFoundException>(() => 
            JsonSchemaParser.ParseDirectory(nonExistentDir));
        
        Assert.Contains("Schema directory not found", exception.Message);
    }

    [Fact]
    public void ParseDirectory_WithEmptyDirectory_ShouldThrowException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "SwaggerGenTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                JsonSchemaParser.ParseDirectory(tempDir));
            
            Assert.Contains("No .json files found", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}