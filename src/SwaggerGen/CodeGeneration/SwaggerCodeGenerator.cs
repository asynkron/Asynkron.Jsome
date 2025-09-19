using SwaggerGen.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Generates C# DTO classes and FluentValidation validators from Swagger definitions
/// </summary>
public class SwaggerCodeGenerator
{
    private readonly TemplateEngine _templateEngine;
    private readonly string _defaultNamespace;
    private SwaggerDocument? _currentDocument;

    public SwaggerCodeGenerator(string defaultNamespace = "Generated")
    {
        _templateEngine = new TemplateEngine();
        _defaultNamespace = defaultNamespace;
    }

    /// <summary>
    /// Generates DTO classes and validators from a Swagger document
    /// </summary>
    /// <param name="document">The Swagger document</param>
    /// <returns>A dictionary with file names as keys and generated code as values</returns>
    public Dictionary<string, string> GenerateCode(SwaggerDocument document)
    {
        _currentDocument = document;
        var generatedFiles = new Dictionary<string, string>();

        // Generate DTO classes for each definition
        foreach (var definition in document.Definitions)
        {
            var dtoModel = CreateDtoModel(definition.Key, definition.Value);
            var dtoCode = _templateEngine.RenderDtoClass(dtoModel);
            generatedFiles[$"{definition.Key}.cs"] = dtoCode;

            // Generate validator for the DTO
            var validatorModel = CreateValidatorModel(definition.Key, definition.Value);
            var validatorCode = _templateEngine.RenderValidator(validatorModel);
            generatedFiles[$"{definition.Key}Validator.cs"] = validatorCode;
        }

        return generatedFiles;
    }

    /// <summary>
    /// Creates a DTO generation model from a Swagger schema
    /// </summary>
    /// <param name="className">The class name</param>
    /// <param name="schema">The Swagger schema</param>
    /// <returns>A DTO generation model</returns>
    private DtoGenerationModel CreateDtoModel(string className, Schema schema)
    {
        var model = new DtoGenerationModel
        {
            Namespace = _defaultNamespace,
            ClassName = className,
            Description = !string.IsNullOrEmpty(schema.Description) ? schema.Description : $"DTO class for {className}"
        };

        // Process properties from the schema
        AddPropertiesFromSchema(model, schema, new HashSet<string>());

        return model;
    }

    /// <summary>
    /// Recursively adds properties from a schema, handling allOf compositions
    /// </summary>
    /// <param name="model">The DTO model to add properties to</param>
    /// <param name="schema">The schema to process</param>
    /// <param name="processedRefs">Set of already processed references to avoid infinite loops</param>
    private void AddPropertiesFromSchema(DtoGenerationModel model, Schema schema, HashSet<string> processedRefs)
    {
        if (schema == null) return;

        // Handle allOf - merge all schemas
        if (schema.AllOf != null && schema.AllOf.Any())
        {
            var allRequired = new List<string>();
            
            // First, collect all required fields from all schemas
            foreach (var allOfSchema in schema.AllOf)
            {
                if (allOfSchema == null) continue;

                if (!string.IsNullOrEmpty(allOfSchema.Ref))
                {
                    var referencedSchema = ResolveReference(allOfSchema.Ref);
                    if (referencedSchema?.Required != null)
                    {
                        allRequired.AddRange(referencedSchema.Required);
                    }
                }
                else if (allOfSchema.Required != null)
                {
                    allRequired.AddRange(allOfSchema.Required);
                }
            }
            
            // Add direct required fields
            if (schema.Required != null)
            {
                allRequired.AddRange(schema.Required);
            }
            
            // Now process properties from all schemas
            foreach (var allOfSchema in schema.AllOf)
            {
                if (allOfSchema == null) continue;

                // If it's a reference, resolve it
                if (!string.IsNullOrEmpty(allOfSchema.Ref))
                {
                    if (processedRefs.Contains(allOfSchema.Ref))
                        continue; // Avoid infinite loops
                    
                    processedRefs.Add(allOfSchema.Ref);
                    
                    var referencedSchema = ResolveReference(allOfSchema.Ref);
                    if (referencedSchema != null)
                    {
                        // Add properties directly from referenced schema with proper required status
                        if (referencedSchema.Properties != null)
                        {
                            foreach (var property in referencedSchema.Properties)
                            {
                                var isRequired = allRequired.Contains(property.Key) || 
                                                (referencedSchema.Required?.Contains(property.Key) == true);
                                var propertyModel = CreatePropertyModel(property.Key, property.Value, isRequired);
                                model.Properties.Add(propertyModel);
                            }
                        }
                    }
                }
                else
                {
                    // Process properties from this inline allOf item
                    if (allOfSchema.Properties != null)
                    {
                        foreach (var property in allOfSchema.Properties)
                        {
                            var isRequired = allRequired.Contains(property.Key);
                            var propertyModel = CreatePropertyModel(property.Key, property.Value, isRequired);
                            model.Properties.Add(propertyModel);
                        }
                    }
                }
            }
        }
        
        // Process any direct properties on the schema
        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                var isRequired = (schema.Required?.Contains(property.Key) == true);
                var propertyModel = CreatePropertyModel(property.Key, property.Value, isRequired);
                model.Properties.Add(propertyModel);
            }
        }
    }

    /// <summary>
    /// Resolves a JSON Schema reference to a Schema object
    /// </summary>
    /// <param name="reference">The reference string (e.g., "#/definitions/Pet")</param>
    /// <returns>The resolved schema, or null if not found</returns>
    private Schema? ResolveReference(string reference)
    {
        if (_currentDocument == null || string.IsNullOrEmpty(reference))
            return null;

        // Handle #/definitions/SchemaName format
        if (reference.StartsWith("#/definitions/"))
        {
            var schemaName = reference.Substring("#/definitions/".Length);
            return _currentDocument.Definitions.TryGetValue(schemaName, out var schema) ? schema : null;
        }

        return null;
    }

    /// <summary>
    /// Creates a property model from a Swagger schema property
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="propertySchema">The property schema</param>
    /// <param name="isRequired">Whether the property is required</param>
    /// <returns>A property model</returns>
    private PropertyModel CreatePropertyModel(string propertyName, Schema propertySchema, bool isRequired)
    {
        if (propertySchema == null)
        {
            propertySchema = new Schema { Type = "object" };
        }

        var property = new PropertyModel
        {
            Name = ToPascalCase(propertyName),
            Description = !string.IsNullOrEmpty(propertySchema.Description) ? propertySchema.Description : $"Gets or sets the {propertyName}",
            IsNullable = !isRequired
        };

        // Determine C# type
        property.Type = GetCSharpType(propertySchema);

        // Add validation attributes
        AddValidationAttributes(property, propertySchema, isRequired);

        // Handle default values
        if (propertySchema.Default != null)
        {
            property.HasDefaultValue = true;
            property.DefaultValue = GetDefaultValue(propertySchema);
        }

        return property;
    }

    /// <summary>
    /// Creates a validator generation model from a Swagger schema
    /// </summary>
    /// <param name="className">The class name</param>
    /// <param name="schema">The Swagger schema</param>
    /// <returns>A validator generation model</returns>
    private ValidatorGenerationModel CreateValidatorModel(string className, Schema schema)
    {
        var model = new ValidatorGenerationModel
        {
            Namespace = _defaultNamespace,
            ClassName = className
        };

        // Process validation rules for properties from the schema
        AddValidationRulesFromSchema(model, schema, new HashSet<string>());

        return model;
    }

    /// <summary>
    /// Recursively adds validation rules from a schema, handling allOf compositions
    /// </summary>
    /// <param name="model">The validator model to add rules to</param>
    /// <param name="schema">The schema to process</param>
    /// <param name="processedRefs">Set of already processed references to avoid infinite loops</param>
    private void AddValidationRulesFromSchema(ValidatorGenerationModel model, Schema schema, HashSet<string> processedRefs)
    {
        if (schema == null) return;

        // Handle allOf - merge all schemas
        if (schema.AllOf != null && schema.AllOf.Any())
        {
            var allRequired = new List<string>();
            var allProperties = new Dictionary<string, Schema>();
            
            // First, collect all required fields and properties from all schemas
            foreach (var allOfSchema in schema.AllOf)
            {
                if (allOfSchema == null) continue;

                if (!string.IsNullOrEmpty(allOfSchema.Ref))
                {
                    if (processedRefs.Contains(allOfSchema.Ref))
                        continue;
                    
                    processedRefs.Add(allOfSchema.Ref);
                    
                    var referencedSchema = ResolveReference(allOfSchema.Ref);
                    if (referencedSchema != null)
                    {
                        if (referencedSchema.Required != null)
                        {
                            allRequired.AddRange(referencedSchema.Required);
                        }
                        if (referencedSchema.Properties != null)
                        {
                            foreach (var prop in referencedSchema.Properties)
                            {
                                allProperties[prop.Key] = prop.Value;
                            }
                        }
                    }
                }
                else
                {
                    if (allOfSchema.Required != null)
                    {
                        allRequired.AddRange(allOfSchema.Required);
                    }
                    if (allOfSchema.Properties != null)
                    {
                        foreach (var prop in allOfSchema.Properties)
                        {
                            allProperties[prop.Key] = prop.Value;
                        }
                    }
                }
            }
            
            // Add direct required fields and properties
            if (schema.Required != null)
            {
                allRequired.AddRange(schema.Required);
            }
            if (schema.Properties != null)
            {
                foreach (var prop in schema.Properties)
                {
                    allProperties[prop.Key] = prop.Value;
                }
            }
            
            // Create validation rules for all properties
            foreach (var property in allProperties)
            {
                var isRequired = allRequired.Contains(property.Key);
                var validationRules = CreateValidationRules(property.Key, property.Value, isRequired);
                model.ValidationRules.AddRange(validationRules);
            }
        }
        else
        {
            // Process regular validation rules for properties
            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    var isRequired = (schema.Required?.Contains(property.Key) == true);
                    var validationRules = CreateValidationRules(property.Key, property.Value, isRequired);
                    model.ValidationRules.AddRange(validationRules);
                }
            }
        }
    }

    /// <summary>
    /// Creates FluentValidation rules for a property
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="propertySchema">The property schema</param>
    /// <param name="isRequired">Whether the property is required</param>
    /// <returns>List of validation rule strings</returns>
    private List<string> CreateValidationRules(string propertyName, Schema propertySchema, bool isRequired)
    {
        var rules = new List<string>();
        var pascalPropertyName = ToPascalCase(propertyName);

        // Required validation
        if (isRequired)
        {
            rules.Add($"RuleFor(x => x.{pascalPropertyName}).NotEmpty();");
        }

        // String validations
        if (propertySchema.Type == "string")
        {
            if (propertySchema.MinLength.HasValue)
            {
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).MinimumLength({propertySchema.MinLength.Value});");
            }
            if (propertySchema.MaxLength.HasValue)
            {
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).MaximumLength({propertySchema.MaxLength.Value});");
            }
            if (!string.IsNullOrEmpty(propertySchema.Pattern))
            {
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).Matches(@\"{propertySchema.Pattern}\");");
            }
        }

        // Numeric validations
        if (propertySchema.Type == "integer" || propertySchema.Type == "number")
        {
            if (propertySchema.Minimum.HasValue)
            {
                var op = propertySchema.ExclusiveMinimum == true ? "GreaterThan" : "GreaterThanOrEqualTo";
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).{op}({propertySchema.Minimum.Value});");
            }
            if (propertySchema.Maximum.HasValue)
            {
                var op = propertySchema.ExclusiveMaximum == true ? "LessThan" : "LessThanOrEqualTo";
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).{op}({propertySchema.Maximum.Value});");
            }
        }

        // Array validations
        if (propertySchema.Type == "array")
        {
            if (propertySchema.MinItems.HasValue)
            {
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).Must(x => x == null || x.Count >= {propertySchema.MinItems.Value});");
            }
            if (propertySchema.MaxItems.HasValue)
            {
                rules.Add($"RuleFor(x => x.{pascalPropertyName}).Must(x => x == null || x.Count <= {propertySchema.MaxItems.Value});");
            }
        }

        return rules;
    }

    /// <summary>
    /// Gets the C# type for a Swagger schema
    /// </summary>
    /// <param name="schema">The schema</param>
    /// <returns>The C# type string</returns>
    private string GetCSharpType(Schema schema)
    {
        // Handle references
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var typeName = schema.Ref.Split('/').Last();
            return typeName;
        }

        return schema.Type switch
        {
            "integer" => schema.Format switch
            {
                "int64" => "long",
                _ => "int"
            },
            "number" => schema.Format switch
            {
                "float" => "float",
                _ => "decimal"
            },
            "string" => schema.Format switch
            {
                "date-time" => "DateTime",
                "date" => "DateOnly",
                "byte" => "byte[]",
                _ => "string"
            },
            "boolean" => "bool",
            "array" => $"List<{GetCSharpType(schema.Items ?? new Schema { Type = "object" })}>",
            "object" => "object",
            _ => "object"
        };
    }

    /// <summary>
    /// Adds validation attributes to a property model
    /// </summary>
    /// <param name="property">The property model</param>
    /// <param name="schema">The property schema</param>
    /// <param name="isRequired">Whether the property is required</param>
    private void AddValidationAttributes(PropertyModel property, Schema schema, bool isRequired)
    {
        if (isRequired)
        {
            property.ValidationAttributes.Add("[Required]");
        }

        if (schema.Type == "string")
        {
            if (schema.MinLength.HasValue || schema.MaxLength.HasValue)
            {
                var minLength = schema.MinLength ?? 0;
                var maxLength = schema.MaxLength?.ToString() ?? "int.MaxValue";
                property.ValidationAttributes.Add($"[StringLength({maxLength}, MinimumLength = {minLength})]");
            }
        }

        if (schema.Type == "integer" || schema.Type == "number")
        {
            if (schema.Minimum.HasValue || schema.Maximum.HasValue)
            {
                var min = schema.Minimum?.ToString() ?? "double.MinValue";
                var max = schema.Maximum?.ToString() ?? "double.MaxValue";
                property.ValidationAttributes.Add($"[Range({min}, {max})]");
            }
        }
    }

    /// <summary>
    /// Gets the default value string for a property
    /// </summary>
    /// <param name="schema">The property schema</param>
    /// <returns>The default value as a string</returns>
    private string GetDefaultValue(Schema schema)
    {
        if (schema.Default == null) return string.Empty;

        return schema.Type switch
        {
            "string" => $"\"{schema.Default}\"",
            "boolean" => schema.Default.ToString()?.ToLower() ?? "false",
            "integer" or "number" => schema.Default.ToString() ?? "0",
            _ => schema.Default.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>The PascalCase string</returns>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Handle snake_case and kebab-case
        var words = Regex.Split(input, @"[_\-\s]+")
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());

        return string.Join("", words);
    }

    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>The camelCase string</returns>
    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var pascalCase = ToPascalCase(input);
        if (pascalCase.Length == 0) return pascalCase;
        
        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }
}