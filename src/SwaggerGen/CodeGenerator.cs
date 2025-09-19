using HandlebarsDotNet;
using SwaggerGen.Models;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace SwaggerGen;

/// <summary>
/// Generates C# code from Swagger definitions using Handlebars templates
/// </summary>
public class CodeGenerator
{
    private readonly string _templatesPath;
    private readonly HandlebarsTemplate<object, string> _dtoTemplate;
    private readonly HandlebarsTemplate<object, string> _validatorTemplate;

    public CodeGenerator(string templatesPath)
    {
        _templatesPath = templatesPath;
        
        // Load and compile templates
        var dtoTemplateSource = File.ReadAllText(Path.Combine(_templatesPath, "DTO.hbs"));
        var validatorTemplateSource = File.ReadAllText(Path.Combine(_templatesPath, "Validator.hbs"));
        
        var handlebars = Handlebars.Create();
        _dtoTemplate = handlebars.Compile(dtoTemplateSource);
        _validatorTemplate = handlebars.Compile(validatorTemplateSource);
    }

    /// <summary>
    /// Generates C# DTO classes and FluentValidation validators from a Swagger document
    /// </summary>
    /// <param name="document">The Swagger document</param>
    /// <param name="outputPath">Base output directory path</param>
    /// <param name="swaggerJson">The original JSON string for parsing $ref correctly</param>
    public async Task GenerateCodeAsync(SwaggerDocument document, string outputPath, string? swaggerJson = null)
    {
        // Create output directories
        var modelsPath = Path.Combine(outputPath, "Models");
        var validatorsPath = Path.Combine(outputPath, "Validators");
        
        Directory.CreateDirectory(modelsPath);
        Directory.CreateDirectory(validatorsPath);

        // Parse JSON to handle $ref properly if provided
        JObject? jsonDocument = null;
        if (!string.IsNullOrEmpty(swaggerJson))
        {
            jsonDocument = JObject.Parse(swaggerJson);
        }

        // Generate DTOs and validators for each definition
        foreach (var definition in document.Definitions)
        {
            var className = definition.Key;
            var schema = definition.Value;

            // Generate DTO
            var dtoContext = CreateDtoContext(className, schema, document.Definitions, jsonDocument);
            var dtoCode = _dtoTemplate.Invoke(dtoContext);
            var dtoFilePath = Path.Combine(modelsPath, $"{className}.cs");
            await File.WriteAllTextAsync(dtoFilePath, dtoCode);

            // Generate validator (only for root objects, not referenced nested objects)
            var validatorContext = CreateValidatorContext(className, schema, document.Definitions, jsonDocument);
            var validatorCode = _validatorTemplate.Invoke(validatorContext);
            var validatorFilePath = Path.Combine(validatorsPath, $"{className}Validator.cs");
            await File.WriteAllTextAsync(validatorFilePath, validatorCode);
        }

        Console.WriteLine($"Generated {document.Definitions.Count} DTO classes in: {modelsPath}");
        Console.WriteLine($"Generated {document.Definitions.Count} validator classes in: {validatorsPath}");
    }

    private object CreateDtoContext(string className, Schema schema, Dictionary<string, Schema> allDefinitions, JObject? jsonDocument = null)
    {
        var properties = new List<object>();
        var allRequiredProperties = new List<string>();
        string? baseClass = null;

        // Collect all required properties from the schema and allOf schemas
        if (schema.Required?.Any() == true)
        {
            allRequiredProperties.AddRange(schema.Required);
        }
        
        // Handle allOf schemas (inheritance) - use JSON directly for $ref handling
        if (schema.AllOf?.Any() == true)
        {
            var allOfItems = GetAllOfItems(className, jsonDocument);
            if (allOfItems?.Any() == true)
            {
                foreach (var allOfItem in allOfItems)
                {
                    var refValue = allOfItem["$ref"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(refValue))
                    {
                        // Reference to another schema - this suggests inheritance
                        var refName = ExtractRefName(refValue);
                        if (allDefinitions.ContainsKey(refName))
                        {
                            // Set the first reference as the base class for inheritance
                            if (baseClass == null)
                            {
                                baseClass = refName;
                            }
                        }
                    }
                    else if (allOfItem["properties"] != null)
                    {
                        // Inline schema with properties
                        var requiredProps = allOfItem["required"]?.ToObject<List<string>>() ?? new List<string>();
                        allRequiredProperties.AddRange(requiredProps);
                        
                        var inlineProperties = allOfItem["properties"]!.ToObject<Dictionary<string, JObject>>();
                        if (inlineProperties != null)
                        {
                            AddPropertiesFromJsonSchema(properties, inlineProperties, allRequiredProperties, allDefinitions);
                        }
                    }
                }
            }
            else
            {
                // Fallback to original schema.AllOf handling
                foreach (var allOfSchema in schema.AllOf)
                {
                    if (allOfSchema?.Required?.Any() == true)
                    {
                        allRequiredProperties.AddRange(allOfSchema.Required);
                    }

                    if (allOfSchema != null && !string.IsNullOrEmpty(allOfSchema.Ref))
                    {
                        // Reference to another schema - this suggests inheritance
                        var refName = ExtractRefName(allOfSchema.Ref);
                        if (allDefinitions.ContainsKey(refName))
                        {
                            // Set the first reference as the base class for inheritance
                            if (baseClass == null)
                            {
                                baseClass = refName;
                            }
                        }
                    }
                    else if (allOfSchema != null)
                    {
                        // Inline schema - add these properties to the current class
                        AddPropertiesFromSchema(properties, allOfSchema, allRequiredProperties, allDefinitions);
                    }
                }
            }
        }
        else
        {
            // Regular schema properties
            AddPropertiesFromSchema(properties, schema, allRequiredProperties, allDefinitions);
        }

        return new
        {
            ClassName = className,
            Description = schema.Description,
            Properties = properties,
            BaseClass = baseClass
        };
    }

    private object CreateValidatorContext(string className, Schema schema, Dictionary<string, Schema> allDefinitions, JObject? jsonDocument = null)
    {
        var properties = new List<object>();
        var allRequiredProperties = new List<string>();

        // Collect all required properties from the schema and allOf schemas
        if (schema.Required?.Any() == true)
        {
            allRequiredProperties.AddRange(schema.Required);
        }

        // Handle allOf schemas (inheritance) - use JSON directly for $ref handling
        if (schema.AllOf?.Any() == true)
        {
            var allOfItems = GetAllOfItems(className, jsonDocument);
            if (allOfItems?.Any() == true)
            {
                foreach (var allOfItem in allOfItems)
                {
                    var refValue = allOfItem["$ref"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(refValue))
                    {
                        // Reference to another schema
                        var refName = ExtractRefName(refValue);
                        if (allDefinitions.ContainsKey(refName))
                        {
                            AddValidationRulesFromSchema(properties, allDefinitions[refName], allRequiredProperties, allDefinitions);
                        }
                    }
                    else if (allOfItem["properties"] != null)
                    {
                        // Inline schema with properties
                        var requiredProps = allOfItem["required"]?.ToObject<List<string>>() ?? new List<string>();
                        allRequiredProperties.AddRange(requiredProps);
                        
                        var inlineProperties = allOfItem["properties"]!.ToObject<Dictionary<string, JObject>>();
                        if (inlineProperties != null)
                        {
                            AddValidationRulesFromJsonSchema(properties, inlineProperties, allRequiredProperties, allDefinitions);
                        }
                    }
                }
            }
            else
            {
                // Fallback to original schema.AllOf handling
                foreach (var allOfSchema in schema.AllOf)
                {
                    if (allOfSchema?.Required?.Any() == true)
                    {
                        allRequiredProperties.AddRange(allOfSchema.Required);
                    }

                    if (allOfSchema != null && !string.IsNullOrEmpty(allOfSchema.Ref))
                    {
                        // Reference to another schema
                        var refName = ExtractRefName(allOfSchema.Ref);
                        if (allDefinitions.ContainsKey(refName))
                        {
                            AddValidationRulesFromSchema(properties, allDefinitions[refName], allRequiredProperties, allDefinitions);
                        }
                    }
                    else if (allOfSchema != null)
                    {
                        // Inline schema
                        AddValidationRulesFromSchema(properties, allOfSchema, allRequiredProperties, allDefinitions);
                    }
                }
            }
        }
        else
        {
            // Regular schema properties
            AddValidationRulesFromSchema(properties, schema, allRequiredProperties, allDefinitions);
        }

        return new
        {
            ClassName = className,
            Description = schema.Description,
            Properties = properties
        };
    }

    private void AddPropertiesFromSchema(List<object> properties, Schema schema, List<string> requiredProperties, Dictionary<string, Schema> allDefinitions)
    {
        foreach (var property in schema.Properties)
        {
            var propertyName = ToPascalCase(property.Key);
            var propertySchema = property.Value;
            var csharpType = GetCSharpType(propertySchema, allDefinitions);
            var isRequired = requiredProperties?.Contains(property.Key) == true;

            var validationAttributes = new List<string>();
            if (isRequired)
            {
                validationAttributes.Add("[Required]");
            }

            // Add validation attributes based on schema constraints
            if (propertySchema.MaxLength.HasValue)
            {
                validationAttributes.Add($"[StringLength({propertySchema.MaxLength})]");
            }

            if (!string.IsNullOrEmpty(propertySchema.Pattern))
            {
                validationAttributes.Add($"[RegularExpression(@\"{propertySchema.Pattern}\")]");
            }

            if (propertySchema.Minimum.HasValue || propertySchema.Maximum.HasValue)
            {
                var min = propertySchema.Minimum?.ToString() ?? "double.MinValue";
                var max = propertySchema.Maximum?.ToString() ?? "double.MaxValue";
                validationAttributes.Add($"[Range({min}, {max})]");
            }

            properties.Add(new
            {
                Name = propertyName,
                Type = isRequired ? csharpType : MakeNullable(csharpType),
                Description = propertySchema.Description,
                ValidationAttributes = validationAttributes,
                DefaultValue = GetDefaultValue(propertySchema, isRequired)
            });
        }
    }

    private void AddValidationRulesFromSchema(List<object> properties, Schema schema, List<string> requiredProperties, Dictionary<string, Schema> allDefinitions)
    {
        foreach (var property in schema.Properties)
        {
            var propertyName = ToPascalCase(property.Key);
            var propertySchema = property.Value;
            var isRequired = requiredProperties?.Contains(property.Key) == true;

            var validationRules = new List<string>();

            if (isRequired)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).NotEmpty();");
            }

            if (propertySchema.MaxLength.HasValue)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).MaximumLength({propertySchema.MaxLength});");
            }

            if (propertySchema.MinLength.HasValue)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).MinimumLength({propertySchema.MinLength});");
            }

            if (!string.IsNullOrEmpty(propertySchema.Pattern))
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).Matches(@\"{propertySchema.Pattern}\");");
            }

            if (propertySchema.Minimum.HasValue)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).GreaterThanOrEqualTo({propertySchema.Minimum});");
            }

            if (propertySchema.Maximum.HasValue)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).LessThanOrEqualTo({propertySchema.Maximum});");
            }

            if (validationRules.Any())
            {
                properties.Add(new
                {
                    Name = propertyName,
                    ValidationRules = validationRules
                });
            }
        }
    }

    private string GetCSharpType(Schema schema, Dictionary<string, Schema> allDefinitions)
    {
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            return ExtractRefName(schema.Ref);
        }

        return schema.Type?.ToLower() switch
        {
            "string" => schema.Format?.ToLower() switch
            {
                "date" => "DateTime",
                "date-time" => "DateTime",
                "byte" => "byte[]",
                _ => "string"
            },
            "integer" => schema.Format?.ToLower() switch
            {
                "int64" => "long",
                _ => "int"
            },
            "number" => schema.Format?.ToLower() switch
            {
                "float" => "float",
                _ => "double"
            },
            "boolean" => "bool",
            "array" => schema.Items != null ? $"List<{GetCSharpType(schema.Items, allDefinitions)}>" : "List<object>",
            "object" => "object",
            _ => "object"
        };
    }

    private string MakeNullable(string type)
    {
        if (type == "string" || type.StartsWith("List<") || type == "object" || type == "byte[]")
        {
            return $"{type}?";
        }
        
        // Value types need ? for nullable
        return type switch
        {
            "int" or "long" or "double" or "float" or "bool" or "DateTime" => $"{type}?",
            _ => $"{type}?" // For custom classes/DTOs
        };
    }

    private string? GetDefaultValue(Schema schema, bool isRequired)
    {
        if (isRequired)
        {
            return schema.Type?.ToLower() switch
            {
                "string" => "string.Empty",
                "array" => "new()",
                _ => null
            };
        }
        return null;
    }

    private string ExtractRefName(string reference)
    {
        // Extract name from "#/definitions/Pet" -> "Pet"
        return reference.Split('/').LastOrDefault() ?? reference;
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    private List<JObject>? GetAllOfItems(string className, JObject? jsonDocument)
    {
        if (jsonDocument == null) return null;
        
        var definitionsToken = jsonDocument["definitions"];
        if (definitionsToken == null) return null;
        
        var classDefinition = definitionsToken[className];
        if (classDefinition == null) return null;
        
        var allOfToken = classDefinition["allOf"];
        if (allOfToken == null) return null;
        
        return allOfToken.ToObject<List<JObject>>();
    }

    private void AddPropertiesFromJsonSchema(List<object> properties, Dictionary<string, JObject> jsonProperties, List<string> requiredProperties, Dictionary<string, Schema> allDefinitions)
    {
        foreach (var kvp in jsonProperties)
        {
            var propertyName = ToPascalCase(kvp.Key);
            var propertyJson = kvp.Value;
            var type = propertyJson["type"]?.ToString() ?? "object";
            var format = propertyJson["format"]?.ToString();
            var isRequired = requiredProperties?.Contains(kvp.Key) == true;

            var csharpType = GetCSharpTypeFromJson(type, format);
            
            var validationAttributes = new List<string>();
            if (isRequired)
            {
                validationAttributes.Add("[Required]");
            }

            // Add validation attributes based on schema constraints
            if (propertyJson["maxLength"] != null)
            {
                validationAttributes.Add($"[StringLength({propertyJson["maxLength"]})]");
            }

            if (propertyJson["pattern"] != null)
            {
                validationAttributes.Add($"[RegularExpression(@\"{propertyJson["pattern"]}\")]");
            }

            if (propertyJson["minimum"] != null || propertyJson["maximum"] != null)
            {
                var min = propertyJson["minimum"]?.ToString() ?? "double.MinValue";
                var max = propertyJson["maximum"]?.ToString() ?? "double.MaxValue";
                validationAttributes.Add($"[Range({min}, {max})]");
            }

            properties.Add(new
            {
                Name = propertyName,
                Type = isRequired ? csharpType : MakeNullable(csharpType),
                Description = propertyJson["description"]?.ToString(),
                ValidationAttributes = validationAttributes,
                DefaultValue = GetDefaultValueForType(type, isRequired)
            });
        }
    }

    private void AddValidationRulesFromJsonSchema(List<object> properties, Dictionary<string, JObject> jsonProperties, List<string> requiredProperties, Dictionary<string, Schema> allDefinitions)
    {
        foreach (var kvp in jsonProperties)
        {
            var propertyName = ToPascalCase(kvp.Key);
            var propertyJson = kvp.Value;
            var isRequired = requiredProperties?.Contains(kvp.Key) == true;

            var validationRules = new List<string>();

            if (isRequired)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).NotEmpty();");
            }

            if (propertyJson["maxLength"] != null)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).MaximumLength({propertyJson["maxLength"]});");
            }

            if (propertyJson["minLength"] != null)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).MinimumLength({propertyJson["minLength"]});");
            }

            if (propertyJson["pattern"] != null)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).Matches(@\"{propertyJson["pattern"]}\");");
            }

            if (propertyJson["minimum"] != null)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).GreaterThanOrEqualTo({propertyJson["minimum"]});");
            }

            if (propertyJson["maximum"] != null)
            {
                validationRules.Add($"RuleFor(x => x.{propertyName}).LessThanOrEqualTo({propertyJson["maximum"]});");
            }

            if (validationRules.Any())
            {
                properties.Add(new
                {
                    Name = propertyName,
                    ValidationRules = validationRules
                });
            }
        }
    }

    private string GetCSharpTypeFromJson(string type, string? format)
    {
        return type?.ToLower() switch
        {
            "string" => format?.ToLower() switch
            {
                "date" => "DateTime",
                "date-time" => "DateTime",
                "byte" => "byte[]",
                _ => "string"
            },
            "integer" => format?.ToLower() switch
            {
                "int64" => "long",
                _ => "int"
            },
            "number" => format?.ToLower() switch
            {
                "float" => "float",
                _ => "double"
            },
            "boolean" => "bool",
            "array" => "List<object>",
            "object" => "object",
            _ => "object"
        };
    }

    private string? GetDefaultValueForType(string type, bool isRequired)
    {
        if (isRequired)
        {
            return type?.ToLower() switch
            {
                "string" => "string.Empty",
                "array" => "new()",
                _ => null
            };
        }
        return null;
    }
}