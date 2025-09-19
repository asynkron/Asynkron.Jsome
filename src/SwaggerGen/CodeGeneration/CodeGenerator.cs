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
    private readonly string _dtoTemplate;
    private readonly string _validatorTemplate;

    public CodeGenerator()
    {
        _handlebars = Handlebars.Create();
        
        // Load templates
        var basePath = GetTemplatesPath();
        _dtoTemplate = File.ReadAllText(Path.Combine(basePath, "DTO.hbs"));
        _validatorTemplate = File.ReadAllText(Path.Combine(basePath, "Validator.hbs"));
    }

    /// <summary>
    /// Generates DTO classes and validators from Swagger document
    /// </summary>
    /// <param name="document">The Swagger document</param>
    /// <param name="targetNamespace">The target namespace for generated classes</param>
    /// <returns>Generated code results</returns>
    public CodeGenerationResult GenerateCode(SwaggerDocument document, string targetNamespace = "Generated")
    {
        var result = new CodeGenerationResult();
        
        foreach (var definition in document.Definitions)
        {
            var classInfo = ConvertSchemaToClassInfo(definition.Key, definition.Value, targetNamespace, document.Definitions);
            
            // Generate DTO
            var dtoCode = GenerateDto(classInfo);
            result.DtoClasses.Add(definition.Key, dtoCode);
            
            // Generate Validator
            var validatorCode = GenerateValidator(classInfo);
            result.Validators.Add(definition.Key, validatorCode);
        }
        
        return result;
    }

    private string GenerateDto(ClassInfo classInfo)
    {
        var template = _handlebars.Compile(_dtoTemplate);
        return template(classInfo);
    }

    private string GenerateValidator(ClassInfo classInfo)
    {
        var template = _handlebars.Compile(_validatorTemplate);
        return template(classInfo);
    }

    private ClassInfo ConvertSchemaToClassInfo(string name, Schema schema, string targetNamespace, Dictionary<string, Schema> allDefinitions)
    {
        var classInfo = new ClassInfo
        {
            ClassName = ToPascalCase(name),
            Namespace = targetNamespace,
            Description = schema.Description ?? ""
        };

        // Handle allOf inheritance (Note: $ref resolution needs enhancement for full inheritance support)
        if (schema.AllOf?.Any() == true)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                // Handle $ref references
                if (!string.IsNullOrEmpty(allOfSchema?.Ref))
                {
                    var refName = allOfSchema.Ref.Replace("#/definitions/", "");
                    if (allDefinitions.TryGetValue(refName, out var refSchema))
                    {
                        if (refSchema.Properties != null)
                        {
                            foreach (var property in refSchema.Properties)
                            {
                                var propertyInfo = ConvertPropertyToPropertyInfo(property.Key, property.Value, refSchema.Required ?? new List<string>());
                                classInfo.Properties.Add(propertyInfo);
                            }
                        }
                    }
                }
                
                // Always also add properties from the allOf schema itself
                if (allOfSchema?.Properties != null)
                {
                    foreach (var property in allOfSchema.Properties)
                    {
                        var propertyInfo = ConvertPropertyToPropertyInfo(property.Key, property.Value, allOfSchema.Required ?? new List<string>());
                        classInfo.Properties.Add(propertyInfo);
                    }
                }
            }
        }
        else
        {
            // Handle regular properties
            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    var propertyInfo = ConvertPropertyToPropertyInfo(property.Key, property.Value, schema.Required ?? new List<string>());
                    classInfo.Properties.Add(propertyInfo);
                }
            }
        }

        return classInfo;
    }

    private PropertyInfo ConvertPropertyToPropertyInfo(string name, Schema schema, List<string> requiredFields)
    {
        var propertyInfo = new PropertyInfo
        {
            Name = ToPascalCase(name),
            JsonPropertyName = name,
            Type = MapSwaggerTypeToCSharpType(schema),
            Description = schema.Description ?? "",
            IsRequired = requiredFields?.Contains(name) ?? false,
            MaxLength = schema.MaxLength,
            MinLength = schema.MinLength
        };

        // Generate validation rules
        propertyInfo.ValidationRules = GenerateValidationRules(schema, requiredFields?.Contains(name) ?? false);

        // Set default value if available
        if (schema.Default != null)
        {
            propertyInfo.DefaultValue = FormatDefaultValue(schema.Default, propertyInfo.Type);
        }

        return propertyInfo;
    }

    private List<ValidationRule> GenerateValidationRules(Schema schema, bool isRequired)
    {
        var rules = new List<ValidationRule>();

        if (isRequired)
        {
            rules.Add(new ValidationRule { Rule = "NotEmpty" });
        }

        if (schema.MinLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MinimumLength", 
                Parameters = new List<string> { schema.MinLength.Value.ToString() }
            });
        }

        if (schema.MaxLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MaximumLength", 
                Parameters = new List<string> { schema.MaxLength.Value.ToString() }
            });
        }

        if (schema.Minimum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "GreaterThanOrEqualTo", 
                Parameters = new List<string> { schema.Minimum.Value.ToString() }
            });
        }

        if (schema.Maximum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "LessThanOrEqualTo", 
                Parameters = new List<string> { schema.Maximum.Value.ToString() }
            });
        }

        if (!string.IsNullOrEmpty(schema.Pattern))
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "Matches", 
                Parameters = new List<string> { $"\"{schema.Pattern}\"" }
            });
        }

        return rules;
    }

    private string MapSwaggerTypeToCSharpType(Schema schema)
    {
        return schema.Type.ToLower() switch
        {
            "integer" => schema.Format == "int64" ? "long" : "int",
            "number" => schema.Format == "float" ? "float" : "decimal",
            "string" => schema.Format == "date-time" ? "DateTime" : "string",
            "boolean" => "bool",
            "array" => $"List<{MapSwaggerTypeToCSharpType(schema.Items ?? new Schema { Type = "object" })}>",
            "object" => "object",
            _ => "object"
        };
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle cases like "NewPet" - don't split on capital letters, just ensure first letter is uppercase
        if (char.IsUpper(input[0]))
            return input;

        var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length == 1)
        {
            // Single word - just capitalize first letter
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        var result = new StringBuilder();
        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLower());
            }
        }

        return result.ToString();
    }

    private string FormatDefaultValue(object value, string type)
    {
        return type.ToLower() switch
        {
            "string" => $"\"{value}\"",
            "bool" => value.ToString()?.ToLower() ?? "false",
            _ => value.ToString() ?? ""
        };
    }

    private string GetTemplatesPath()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var templatesPath = Path.Combine(basePath, "Templates");
        
        // If running from build output, try to find the source templates
        if (!Directory.Exists(templatesPath))
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (currentDir != null)
            {
                var srcPath = Path.Combine(currentDir.FullName, "src", "SwaggerGen", "Templates");
                if (Directory.Exists(srcPath))
                {
                    templatesPath = srcPath;
                    break;
                }
                currentDir = currentDir.Parent;
            }
        }

        return templatesPath;
    }
}