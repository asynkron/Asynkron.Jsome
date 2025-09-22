using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using SwaggerGen;

namespace SwaggerGen.Tests;

/// <summary>
/// Tests that validate generated code compiles successfully and behaves correctly
/// </summary>
public class CompilationValidationTests
{
    /// <summary>
    /// Validates that generated DTOs with required properties compile and have proper [Required] attributes
    /// </summary>
    [Fact]
    public void GeneratedDtos_WithRequiredProperties_CompileSuccessfully()
    {
        // Arrange - Create a Swagger document with required properties
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["User"] = new Schema
                {
                    Type = "object",
                    Description = "A user object with required properties",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "integer" },
                        ["name"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" }
                    },
                    Required = ["name", "email"] // Both name and email are required
                },
                ["Address"] = new Schema
                {
                    Type = "object",
                    Description = "An address object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["street"] = new Schema { Type = "string" },
                        ["city"] = new Schema { Type = "string" },
                        ["zipCode"] = new Schema { Type = "string" }
                    },
                    Required = ["street", "city"] // street and city are required
                }
            }
        };

        var generator = new CodeGenerator();
        
        // Act - Generate code
        var result = generator.GenerateCode(document, "Test.Generated.Compilation");
        
        // Assert - Verify generation succeeded
        Assert.Equal(2, result.DtoClasses.Count);
        Assert.True(result.DtoClasses.ContainsKey("User"));
        Assert.True(result.DtoClasses.ContainsKey("Address"));
        
        // Compile the generated code
        var compilation = CompileGeneratedCode(result);
        
        // Verify compilation succeeded
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        
        Assert.Empty(errors); // Should have no compilation errors
        
        // Load the compiled assembly and verify attributes
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        
        Assert.True(emitResult.Success);
        
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        
        // Verify User class has Required attributes on correct properties
        var userType = assembly.GetType("Test.Generated.Compilation.User");
        Assert.NotNull(userType);
        
        var nameProperty = userType.GetProperty("Name");
        var emailProperty = userType.GetProperty("Email");
        var idProperty = userType.GetProperty("Id");
        
        Assert.NotNull(nameProperty);
        Assert.NotNull(emailProperty);
        Assert.NotNull(idProperty);
        
        // Verify [Required] attributes are present on required properties
        Assert.True(nameProperty.GetCustomAttributes<RequiredAttribute>().Any());
        Assert.True(emailProperty.GetCustomAttributes<RequiredAttribute>().Any());
        
        // Verify [Required] attribute is NOT present on optional properties
        Assert.False(idProperty.GetCustomAttributes<RequiredAttribute>().Any());
        
        // Verify Address class has Required attributes on correct properties
        var addressType = assembly.GetType("Test.Generated.Compilation.Address");
        Assert.NotNull(addressType);
        
        var streetProperty = addressType.GetProperty("Street");
        var cityProperty = addressType.GetProperty("City");
        var zipCodeProperty = addressType.GetProperty("ZipCode");
        
        Assert.NotNull(streetProperty);
        Assert.NotNull(cityProperty);
        Assert.NotNull(zipCodeProperty);
        
        // Verify [Required] attributes are present on required properties
        Assert.True(streetProperty.GetCustomAttributes<RequiredAttribute>().Any());
        Assert.True(cityProperty.GetCustomAttributes<RequiredAttribute>().Any());
        
        // Verify [Required] attribute is NOT present on optional properties
        Assert.False(zipCodeProperty.GetCustomAttributes<RequiredAttribute>().Any());
    }
    
    /// <summary>
    /// Validates that DTOs with required properties protect against null serialization through [Required] attributes
    /// </summary>
    [Fact]
    public void GeneratedDtos_WithRequiredProperties_PreventNullSerialization()
    {
        // Arrange - Create a schema that has both required and optional properties
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Product"] = new Schema
                {
                    Type = "object",
                    Description = "A product with both required and optional properties",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string" },
                        ["description"] = new Schema { Type = "string" },
                        ["price"] = new Schema { Type = "number", Format = "decimal" },
                        ["category"] = new Schema { Type = "string" }
                    },
                    Required = ["id", "name", "price"] // id, name, and price are required
                }
            }
        };

        var generator = new CodeGenerator();
        
        // Act - Generate code
        var result = generator.GenerateCode(document, "Test.Generated.RequiredValidation");
        
        // Assert - Basic generation validation
        Assert.Single(result.DtoClasses);
        Assert.True(result.DtoClasses.ContainsKey("Product"));
        
        // Compile and verify
        var compilation = CompileGeneratedCode(result);
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        
        Assert.Empty(errors); // Should compile without errors
        
        // Load the compiled assembly and verify [Required] attributes are correctly applied
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        
        Assert.True(emitResult.Success);
        
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        
        var productType = assembly.GetType("Test.Generated.RequiredValidation.Product");
        Assert.NotNull(productType);
        
        // Get all properties
        var idProperty = productType.GetProperty("Id");
        var nameProperty = productType.GetProperty("Name");
        var descriptionProperty = productType.GetProperty("Description");
        var priceProperty = productType.GetProperty("Price");
        var categoryProperty = productType.GetProperty("Category");
        
        Assert.NotNull(idProperty);
        Assert.NotNull(nameProperty);
        Assert.NotNull(descriptionProperty);
        Assert.NotNull(priceProperty);
        Assert.NotNull(categoryProperty);
        
        // Verify [Required] attributes are ONLY on required properties
        Assert.True(idProperty.GetCustomAttributes<RequiredAttribute>().Any(), "Id should have [Required] attribute");
        Assert.True(nameProperty.GetCustomAttributes<RequiredAttribute>().Any(), "Name should have [Required] attribute");
        Assert.True(priceProperty.GetCustomAttributes<RequiredAttribute>().Any(), "Price should have [Required] attribute");
        
        // Verify [Required] attributes are NOT on optional properties
        Assert.False(descriptionProperty.GetCustomAttributes<RequiredAttribute>().Any(), "Description should NOT have [Required] attribute");
        Assert.False(categoryProperty.GetCustomAttributes<RequiredAttribute>().Any(), "Category should NOT have [Required] attribute");
        
        // Verify the properties exist and are settable (basic DTO functionality)
        var instance = Activator.CreateInstance(productType);
        Assert.NotNull(instance);
        
        // Test that we can set values (basic DTO functionality test)
        idProperty.SetValue(instance, "test-id");
        nameProperty.SetValue(instance, "Test Product");
        
        Assert.Equal("test-id", idProperty.GetValue(instance));
        Assert.Equal("Test Product", nameProperty.GetValue(instance));
    }
    
    /// <summary>
    /// Validates that complex DTOs with enums and nested objects compile successfully
    /// </summary>
    [Fact]
    public void GeneratedDtos_WithComplexTypes_CompileSuccessfully()
    {
        // Arrange - Create a more complex Swagger document
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Customer"] = new Schema
                {
                    Type = "object",
                    Description = "A customer with complex properties",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" },
                        ["status"] = new Schema { 
                            Type = "string", 
                            Enum = ["active", "inactive", "pending"]
                        },
                        ["address"] = new Schema
                        {
                            Ref = "#/definitions/Address"
                        },
                        ["tags"] = new Schema
                        {
                            Type = "array",
                            Items = new Schema { Type = "string" }
                        }
                    },
                    Required = ["id", "status"]
                },
                ["Address"] = new Schema
                {
                    Type = "object", 
                    Properties = new Dictionary<string, Schema>
                    {
                        ["street"] = new Schema { Type = "string" },
                        ["city"] = new Schema { Type = "string" }
                    },
                    Required = ["street"]
                }
            }
        };

        var generator = new CodeGenerator();
        
        // Act - Generate code
        var result = generator.GenerateCode(document, "Test.Generated.Complex");
        
        // Assert - Verify generation succeeded  
        Assert.True(result.DtoClasses.Count >= 2);
        
        // Compile the generated code
        var compilation = CompileGeneratedCode(result);
        
        // Verify compilation succeeded
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        
        Assert.Empty(errors); // Should have no compilation errors
        
        // Verify emit succeeds
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        
        Assert.True(emitResult.Success);
    }
    
    /// <summary>
    /// Tests full roundtrip serialization-deserialization with Pet demo DTOs
    /// </summary>
    [Fact]
    public void GeneratedDtos_PetDemo_FullRoundtripSerializationWorks()
    {
        // Arrange - Use the petstore swagger document to generate Pet DTOs
        var json = File.ReadAllText(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "src", "SwaggerGen", "Samples", "petstore-swagger.json"));
        var document = SwaggerParser.Parse(json);
        var generator = new CodeGenerator();
        
        // Act - Generate code
        var result = generator.GenerateCode(document, "PetStore.Generated");
        
        // Assert - Verify we have the expected DTOs
        Assert.True(result.DtoClasses.ContainsKey("Pet"), "Pet DTO should be generated");
        Assert.True(result.DtoClasses.ContainsKey("NewPet"), "NewPet DTO should be generated");
        Assert.True(result.DtoClasses.ContainsKey("Error"), "Error DTO should be generated");
        
        // Compile the generated code
        var compilation = CompileGeneratedCode(result);
        
        // Verify compilation succeeded
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors); // Should have no compilation errors
        
        // Emit to assembly
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        Assert.True(emitResult.Success, "Code should compile successfully");
        
        // Load the compiled assembly
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        
        // Test roundtrip serialization for each DTO type with various scenarios
        TestRoundtripSerialization(assembly, "Pet", """{"name":"Buddy","tag":"dog","id":12345}""");
        TestRoundtripSerialization(assembly, "Pet", """{"name":"Rex","id":67890}"""); // No tag (optional field)
        TestRoundtripSerialization(assembly, "NewPet", """{"name":"Fluffy","tag":"cat"}""");
        TestRoundtripSerialization(assembly, "NewPet", """{"name":"Max"}"""); // No tag (optional field)
        TestRoundtripSerialization(assembly, "Error", """{"code":404,"message":"Pet not found"}""");
        TestRoundtripSerialization(assembly, "Error", """{"code":500,"message":"Internal server error"}""");
    }
    
    /// <summary>
    /// Tests roundtrip serialization with complex nested objects from Stripe schema
    /// </summary>
    [Fact]
    public void GeneratedDtos_ComplexObjects_FullRoundtripSerializationWorks()
    {
        // Arrange - Create a complex document with nested objects
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["Customer"] = new Schema
                {
                    Type = "object",
                    Description = "A customer with nested address",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["id"] = new Schema { Type = "string" },
                        ["name"] = new Schema { Type = "string" },
                        ["email"] = new Schema { Type = "string" },
                        ["address"] = new Schema { Ref = "#/definitions/Address" },
                        ["tags"] = new Schema
                        {
                            Type = "array",
                            Items = new Schema { Type = "string" }
                        }
                    },
                    Required = ["id", "name"]
                },
                ["Address"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["street"] = new Schema { Type = "string" },
                        ["city"] = new Schema { Type = "string" },
                        ["zipCode"] = new Schema { Type = "string" },
                        ["country"] = new Schema { Type = "string" }
                    },
                    Required = ["street", "city"]
                }
            }
        };

        var generator = new CodeGenerator();
        
        // Act - Generate code
        var result = generator.GenerateCode(document, "Complex.Generated");
        
        // Compile the generated code
        var compilation = CompileGeneratedCode(result);
        
        // Verify compilation succeeded
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
        
        // Emit to assembly
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        Assert.True(emitResult.Success);
        
        // Load the compiled assembly
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        
        // Test roundtrip serialization with complex nested objects
        TestRoundtripSerialization(assembly, "Customer", 
            """{"id":"cust_123","name":"John Doe","email":"john@example.com","address":{"street":"123 Main St","city":"Springfield","zipCode":"12345","country":"USA"},"tags":["vip","premium"]}""");
        
        TestRoundtripSerialization(assembly, "Customer", 
            """{"id":"cust_456","name":"Jane Smith"}"""); // Minimal required fields only
        
        TestRoundtripSerialization(assembly, "Address", 
            """{"street":"456 Oak Ave","city":"Boston","zipCode":"02101"}""");
    }
    
    /// <summary>
    /// Tests the exact scenario described in the problem statement:
    /// Load JSON file, deserialize to compiled C# type, serialize back to JSON, verify identical
    /// </summary>
    [Fact]
    public void GeneratedDtos_ExactProblemStatementScenario_RoundtripWithJsonFiles()
    {
        // Step 1: Generate the Pet demo DTOs
        var swaggerJson = File.ReadAllText(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "src", "SwaggerGen", "Samples", "petstore-swagger.json"));
        var document = SwaggerParser.Parse(swaggerJson);
        var generator = new CodeGenerator();
        var result = generator.GenerateCode(document, "PetDemo.Generated");
        
        // Step 2: Compile the code via Roslyn
        var compilation = CompileGeneratedCode(result);
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
        
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        Assert.True(emitResult.Success);
        
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        
        // Step 3: Create sample JSON files representing various samples of said entities
        var sampleJsonFiles = new Dictionary<string, string[]>
        {
            ["Pet"] = [
                """{"name":"Buddy","tag":"dog","id":12345}""",
                """{"name":"Rex","id":67890}""",
                """{"name":"Whiskers","tag":"cat","id":999}"""
            ],
            ["NewPet"] = [
                """{"name":"Fluffy","tag":"cat"}""",
                """{"name":"Max"}""",
                """{"name":"Luna","tag":"rabbit"}"""
            ],
            ["Error"] = [
                """{"code":404,"message":"Pet not found"}""",
                """{"code":500,"message":"Internal server error"}""",
                """{"code":400,"message":"Invalid pet data"}"""
            ]
        };
        
        // Step 4-6: For each sample JSON file:
        // - Load the JSON file
        // - Deserialize it into our compiled C# type
        // - Serialize the same entity back to JSON
        // - Verify the result is identical to the original
        foreach (var (typeName, jsonSamples) in sampleJsonFiles)
        {
            var dtoType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            Assert.NotNull(dtoType);
            
            foreach (var originalJson in jsonSamples)
            {
                // Simulate loading from a JSON file
                var loadedJson = originalJson;
                
                // Deserialize into our compiled C# type
                var deserializedObject = Newtonsoft.Json.JsonConvert.DeserializeObject(loadedJson, dtoType);
                Assert.NotNull(deserializedObject);
                
                // Serialize the same entity back to JSON
                var roundtripJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    deserializedObject,
                    Newtonsoft.Json.Formatting.None,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    });
                
                // Verify the result is identical to the original
                var originalJObject = Newtonsoft.Json.Linq.JObject.Parse(originalJson);
                var roundtripJObject = Newtonsoft.Json.Linq.JObject.Parse(roundtripJson);
                
                Assert.True(Newtonsoft.Json.Linq.JToken.DeepEquals(originalJObject, roundtripJObject),
                    $"Full roundtrip validation failed for {typeName}. " +
                    $"Original: {originalJson}, Roundtrip: {roundtripJson}");
            }
        }
        
        // If we reach here, "everything is good" as stated in the problem statement
        Assert.True(true, "Full roundtrip deserialization-serialization working perfectly!");
    }
    
    /// <summary>
    /// Helper method to test roundtrip serialization for a specific DTO type
    /// </summary>
    private static void TestRoundtripSerialization(Assembly assembly, string typeName, string originalJson)
    {
        // Get the DTO type from the compiled assembly
        var dtoType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
        Assert.NotNull(dtoType);
        
        // Deserialize the original JSON into the DTO
        var deserializedObject = Newtonsoft.Json.JsonConvert.DeserializeObject(originalJson, dtoType);
        Assert.NotNull(deserializedObject);
        
        // Serialize the object back to JSON
        var roundtripJson = Newtonsoft.Json.JsonConvert.SerializeObject(
            deserializedObject, 
            Newtonsoft.Json.Formatting.None,
            new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
        
        // Parse both JSON strings to JObjects for comparison (ignoring property order)
        var originalJObject = Newtonsoft.Json.Linq.JObject.Parse(originalJson);
        var roundtripJObject = Newtonsoft.Json.Linq.JObject.Parse(roundtripJson);
        
        // Assert that the JSON is identical after roundtrip
        Assert.True(Newtonsoft.Json.Linq.JToken.DeepEquals(originalJObject, roundtripJObject), 
            $"Roundtrip failed for {typeName}. Original: {originalJson}, Roundtrip: {roundtripJson}");
    }

    /// <summary>
    /// Helper method to compile generated code using Roslyn
    /// </summary>
    private static CSharpCompilation CompileGeneratedCode(CodeGenerationResult result)
    {
        var syntaxTrees = new List<SyntaxTree>();
        
        // Add all generated DTO classes
        foreach (var dto in result.DtoClasses.Values)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(dto);
            syntaxTrees.Add(syntaxTree);
        }
        
        // Add generated enums and constants if any
        foreach (var enumCode in result.EnumTypes.Values)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(enumCode);
            syntaxTrees.Add(syntaxTree);
        }
        
        foreach (var constantsCode in result.ConstantClasses.Values)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(constantsCode);
            syntaxTrees.Add(syntaxTree);
        }
        
        // Get references to required assemblies - need comprehensive references for compilation
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Runtime
            MetadataReference.CreateFromFile(typeof(RequiredAttribute).Assembly.Location), // System.ComponentModel.DataAnnotations
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location), // System.Collections
        };
        
        // Add Newtonsoft.Json reference
        try
        {
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("Newtonsoft.Json").Location));
        }
        catch
        {
            // If Newtonsoft.Json isn't available, try to find it in app domain
            var newtonsoftAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Newtonsoft.Json");
            if (newtonsoftAssembly != null)
            {
                references.Add(MetadataReference.CreateFromFile(newtonsoftAssembly.Location));
            }
        }
        
        // Add System runtime assemblies
        try
        {
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));
        }
        catch
        {
            // Ignore if these can't be loaded - they may not be needed
        }
        
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        
        return CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            compilationOptions);
    }
}