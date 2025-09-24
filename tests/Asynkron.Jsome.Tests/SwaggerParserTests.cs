using Asynkron.Jsome;
using Asynkron.Jsome.Models;
using Xunit;

namespace Asynkron.Jsome.Tests;

public class SwaggerParserTests
{
    private const string ValidSwaggerJson = @"{
        ""swagger"": ""2.0"",
        ""info"": {
            ""title"": ""Test API"",
            ""version"": ""1.0.0"",
            ""description"": ""A test API""
        },
        ""host"": ""api.test.com"",
        ""basePath"": ""/v1"",
        ""schemes"": [""https""],
        ""paths"": {
            ""/test"": {
                ""get"": {
                    ""summary"": ""Test endpoint"",
                    ""responses"": {
                        ""200"": {
                            ""description"": ""Success""
                        }
                    }
                }
            }
        },
        ""definitions"": {
            ""TestModel"": {
                ""type"": ""object"",
                ""properties"": {
                    ""id"": {
                        ""type"": ""integer""
                    },
                    ""name"": {
                        ""type"": ""string""
                    }
                }
            }
        }
    }";

    [Fact]
    public void Parse_ValidSwaggerJson_ReturnsSwaggerDocument()
    {
        // Act
        var result = SwaggerParser.Parse(ValidSwaggerJson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2.0", result.Swagger);
        Assert.Equal("Test API", result.Info.Title);
        Assert.Equal("1.0.0", result.Info.Version);
        Assert.Equal("A test API", result.Info.Description);
        Assert.Equal("api.test.com", result.Host);
        Assert.Equal("/v1", result.BasePath);
        Assert.Single(result.Schemes);
        Assert.Equal("https", result.Schemes[0]);
        Assert.Single(result.Paths);
        Assert.True(result.Paths.ContainsKey("/test"));
        Assert.Single(result.Definitions);
        Assert.True(result.Definitions.ContainsKey("TestModel"));
    }

    [Fact]
    public void Parse_NullJson_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwaggerParser.Parse(null!));
        Assert.Contains("JSON string cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Parse_EmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => SwaggerParser.Parse(""));
        Assert.Contains("JSON string cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<Newtonsoft.Json.JsonReaderException>(() => SwaggerParser.Parse(invalidJson));
    }

    [Fact]
    public void Parse_MissingSwaggerField_ThrowsJsonException()
    {
        // Arrange
        const string jsonWithoutSwagger = @"{
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            }
        }";

        // Act & Assert
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => SwaggerParser.Parse(jsonWithoutSwagger));
        Assert.Contains("Document must have a 'swagger' field", exception.Message);
    }

    [Fact]
    public void Parse_UnsupportedSwaggerVersion_ThrowsJsonException()
    {
        // Arrange
        const string swagger3Json = @"{
            ""swagger"": ""3.0.0"",
            ""info"": {
                ""title"": ""Test API"",
                ""version"": ""1.0.0""
            }
        }";

        // Act & Assert
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => SwaggerParser.Parse(swagger3Json));
        Assert.Contains("Only Swagger 2.0 is supported", exception.Message);
    }

    [Fact]
    public void Parse_MissingInfoField_ThrowsJsonException()
    {
        // Arrange - when info is missing, it will create default Info object but without title/version
        const string jsonWithoutInfo = @"{
            ""swagger"": ""2.0""
        }";

        // Act & Assert - The validation will fail on missing title
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => SwaggerParser.Parse(jsonWithoutInfo));
        Assert.Contains("Document info must have a 'title' field", exception.Message);
    }

    [Fact]
    public void Parse_MissingInfoTitle_ThrowsJsonException()
    {
        // Arrange
        const string jsonWithoutTitle = @"{
            ""swagger"": ""2.0"",
            ""info"": {
                ""version"": ""1.0.0""
            }
        }";

        // Act & Assert
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => SwaggerParser.Parse(jsonWithoutTitle));
        Assert.Contains("Document info must have a 'title' field", exception.Message);
    }

    [Fact]
    public void Parse_MissingInfoVersion_ThrowsJsonException()
    {
        // Arrange
        const string jsonWithoutVersion = @"{
            ""swagger"": ""2.0"",
            ""info"": {
                ""title"": ""Test API""
            }
        }";

        // Act & Assert
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => SwaggerParser.Parse(jsonWithoutVersion));
        Assert.Contains("Document info must have a 'version' field", exception.Message);
    }

    [Fact]
    public async Task ParseFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        const string nonExistentFile = "/path/to/nonexistent/file.json";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => SwaggerParser.ParseFileAsync(nonExistentFile));
    }

    [Fact]
    public void ParseFileAsync_NullFilePath_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(() => SwaggerParser.ParseFileAsync(null!));
        Assert.Contains("File path cannot be null or empty", exception.Result.Message);
    }

    [Fact]
    public void GetDocumentSummary_ValidDocument_ReturnsFormattedSummary()
    {
        // Arrange
        var document = SwaggerParser.Parse(ValidSwaggerJson);

        // Act
        var summary = SwaggerParser.GetDocumentSummary(document);

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("Test API", summary);
        Assert.Contains("1.0.0", summary);
        Assert.Contains("A test API", summary);
        Assert.Contains("api.test.com", summary);
        Assert.Contains("/v1", summary);
        Assert.Contains("https", summary);
        Assert.Contains("Paths: 1", summary);
        Assert.Contains("Definitions: 1", summary);
        Assert.Contains("/test", summary);
        Assert.Contains("TestModel", summary);
    }

    [Fact]
    public void GetDocumentSummary_NullDocument_ReturnsNoDocumentMessage()
    {
        // Act
        var summary = SwaggerParser.GetDocumentSummary(null!);

        // Assert
        Assert.Equal("No document provided", summary);
    }
}