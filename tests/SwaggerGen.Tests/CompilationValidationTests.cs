using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

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