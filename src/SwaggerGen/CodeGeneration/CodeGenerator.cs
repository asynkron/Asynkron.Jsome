using HandlebarsDotNet;
using SwaggerGen.Models;
using System.Text;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Generates C# code from Swagger definitions using Handlebars templates
/// </summary>
public class CodeGenerator
{
    private readonly IHandlebars _handlebars;
    private readonly HandlebarsTemplate<object, string> _dtoTemplate;
    private readonly HandlebarsTemplate<object, string> _validatorTemplate;

    public CodeGenerator()
    {
        _handlebars = Handlebars.Create();
        
        // Load templates
        var dtoTemplateSource = LoadTemplate("DTO.hbs");
        var validatorTemplateSource = LoadTemplate("Validator.hbs");
        
        _dtoTemplate = _handlebars.Compile(dtoTemplateSource);
        _validatorTemplate = _handlebars.Compile(validatorTemplateSource);
    }

    /// <summary>
    /// Generates DTO classes from Swagger definitions
    /// </summary>
    public List<GeneratedFile> GenerateDTOs(SwaggerDocument document, string namespaceName = "Generated.DTOs")
    {
        var files = new List<GeneratedFile>();
        
        foreach (var definition in document.Definitions)
        {
            var dtoClass = CreateDTOClass(definition.Key, definition.Value, namespaceName, document);
            var code = _dtoTemplate(dtoClass);
            
            files.Add(new GeneratedFile
            {
                FileName = $"{dtoClass.ClassName}.cs",
                Content = code,
                Type = GeneratedFileType.DTO
            });
        }
        
        return files;
    }

    /// <summary>
    /// Generates FluentValidation validators from Swagger definitions
    /// </summary>
    public List<GeneratedFile> GenerateValidators(SwaggerDocument document, string namespaceName = "Generated.DTOs")
    {
        var files = new List<GeneratedFile>();
        
        foreach (var definition in document.Definitions)
        {
            var dtoClass = CreateDTOClass(definition.Key, definition.Value, namespaceName, document);
            var code = _validatorTemplate(dtoClass);
            
            files.Add(new GeneratedFile
            {
                FileName = $"{dtoClass.ClassName}Validator.cs",
                Content = code,
                Type = GeneratedFileType.Validator
            });
        }
        
        return files;
    }

    private DTOClass CreateDTOClass(string name, Schema schema, string namespaceName, SwaggerDocument document)
    {
        var dtoClass = new DTOClass
        {
            ClassName = ToPascalCase(name),
            Namespace = namespaceName,
            Description = schema.Description ?? $"DTO for {name}"
        };

        // Handle allOf schemas (like Pet in the sample)
        if (schema.AllOf != null && schema.AllOf.Count > 0)
        {
            for (int i = 0; i < schema.AllOf.Count; i++)
            {
                var allOfSchema = schema.AllOf[i];
                
                if (allOfSchema == null)
                {
                    // This can happen with $ref items that aren't parsed correctly
                    // For now, we'll skip these and handle them later if needed
                    continue;
                }
                    
                if (!string.IsNullOrEmpty(allOfSchema.Ref))
                {
                    // Reference to another definition
                    var refName = ExtractRefName(allOfSchema.Ref);
                    if (document.Definitions.TryGetValue(refName, out var refSchema))
                    {
                        AddPropertiesFromSchema(dtoClass, refSchema, document);
                    }
                }
                else
                {
                    // Inline schema
                    AddPropertiesFromSchema(dtoClass, allOfSchema, document);
                }
            }
        }
        else
        {
            AddPropertiesFromSchema(dtoClass, schema, document);
        }

        return dtoClass;
    }

    private void AddPropertiesFromSchema(DTOClass dtoClass, Schema schema, SwaggerDocument document)
    {
        if (schema.Properties == null)
            return;

        foreach (var property in schema.Properties)
        {
            var prop = new DTOProperty
            {
                Name = ToPascalCase(property.Key),
                JsonName = property.Key,
                Type = GetCSharpType(property.Value, document),
                Description = property.Value.Description,
                IsRequired = schema.Required?.Contains(property.Key) ?? false,
                MaxLength = property.Value.MaxLength,
                MinLength = property.Value.MinLength,
                Pattern = property.Value.Pattern,
                Minimum = property.Value.Minimum,
                Maximum = property.Value.Maximum,
                DefaultValue = GetDefaultValue(property.Value),
                IsNestedObject = IsNestedObject(property.Value, document),
                IsArray = property.Value.Type == "array"
            };

            if (prop.IsArray && property.Value.Items != null)
            {
                prop.ArrayItemValidator = GetArrayItemValidator(property.Value.Items, document);
            }

            dtoClass.Properties.Add(prop);
        }
    }

    private string GetCSharpType(Schema schema, SwaggerDocument document)
    {
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            return ToPascalCase(ExtractRefName(schema.Ref));
        }

        return schema.Type switch
        {
            "string" => schema.Format switch
            {
                "date-time" => "DateTime",
                "date" => "DateOnly",
                "uuid" => "Guid",
                _ => "string"
            },
            "integer" => schema.Format switch
            {
                "int64" => "long",
                _ => "int"
            },
            "number" => schema.Format switch
            {
                "float" => "float",
                "double" => "double",
                _ => "decimal"
            },
            "boolean" => "bool",
            "array" => schema.Items != null ? $"List<{GetCSharpType(schema.Items, document)}>" : "List<object>",
            "object" => "object",
            _ => "object"
        };
    }

    private bool IsNestedObject(Schema schema, SwaggerDocument document)
    {
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var refName = ExtractRefName(schema.Ref);
            return document.Definitions.ContainsKey(refName);
        }

        return schema.Type == "object" && schema.Properties != null && schema.Properties.Any();
    }

    private string? GetArrayItemValidator(Schema itemSchema, SwaggerDocument document)
    {
        if (!string.IsNullOrEmpty(itemSchema.Ref))
        {
            return $"{ToPascalCase(ExtractRefName(itemSchema.Ref))}Validator";
        }

        if (itemSchema.Type == "object" && itemSchema.Properties != null && itemSchema.Properties.Any())
        {
            // For inline object definitions in arrays, we'd need to generate a separate class
            // For now, return null to keep it simple
            return null;
        }

        return null;
    }

    private string? GetDefaultValue(Schema schema)
    {
        if (schema.Default == null)
            return null;

        return schema.Type switch
        {
            "string" => $"\"{schema.Default}\"",
            "boolean" => schema.Default.ToString()?.ToLower(),
            _ => schema.Default.ToString()
        };
    }

    private string ExtractRefName(string reference)
    {
        // Extract name from "#/definitions/Pet" -> "Pet"
        return reference.Split('/').Last();
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle common cases like "NewPet" that should stay as "NewPet"
        if (char.IsUpper(input[0]))
        {
            // If it's already in PascalCase or has mixed case, return as is
            return input;
        }

        var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                {
                    result.Append(word.Substring(1));
                }
            }
        }

        return result.ToString();
    }

    private string LoadTemplate(string templateName)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateName);
        
        // Try relative path from current directory if not found
        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateName);
        }
        
        // Try finding it in the source directory
        if (!File.Exists(templatePath))
        {
            var currentDir = Directory.GetCurrentDirectory();
            var possiblePaths = new[]
            {
                Path.Combine(currentDir, "src", "SwaggerGen", "Templates", templateName),
                Path.Combine(currentDir, "..", "..", "..", "Templates", templateName),
                Path.Combine(currentDir, "Templates", templateName)
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    templatePath = path;
                    break;
                }
            }
        }

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templateName}. Searched in: {templatePath}");
        }

        return File.ReadAllText(templatePath);
    }
}

/// <summary>
/// Represents a generated code file
/// </summary>
public class GeneratedFile
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public GeneratedFileType Type { get; set; }
}

/// <summary>
/// Types of generated files
/// </summary>
public enum GeneratedFileType
{
    DTO,
    Validator
}