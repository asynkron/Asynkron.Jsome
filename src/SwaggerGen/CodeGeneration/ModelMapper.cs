using SwaggerGen.CodeGeneration.Models;
using SwaggerGen.Models;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Maps Swagger schema definitions to code generation models
/// </summary>
public class ModelMapper
{
    private readonly string _defaultNamespace;

    public ModelMapper(string defaultNamespace = "Generated.DTOs")
    {
        _defaultNamespace = defaultNamespace;
    }

    /// <summary>
    /// Convert a Swagger schema to a DTO model
    /// </summary>
    /// <param name="schemaName">Name of the schema</param>
    /// <param name="schema">The schema definition</param>
    /// <param name="namespaceName">Optional namespace override</param>
    /// <returns>DTO model</returns>
    public DtoModel MapToDto(string schemaName, Schema schema, string? namespaceName = null)
    {
        var model = new DtoModel
        {
            ClassName = ToCSharpClassName(schemaName),
            Namespace = namespaceName ?? _defaultNamespace,
            Description = schema.Description,
            RequiredProperties = schema.Required.ToList()
        };

        foreach (var property in schema.Properties)
        {
            var propertyModel = new PropertyModel
            {
                Name = ToCSharpPropertyName(property.Key),
                Type = MapSwaggerTypeToCSharp(property.Value),
                Description = property.Value.Description,
                IsRequired = schema.Required.Contains(property.Key),
                IsNullable = !schema.Required.Contains(property.Key) && !IsValueType(property.Value),
                JsonPropertyName = property.Key,
                DefaultValue = GetDefaultValue(property.Value)
            };

            model.Properties.Add(propertyModel);
        }

        return model;
    }

    /// <summary>
    /// Convert a DTO model to a validator model
    /// </summary>
    /// <param name="dtoModel">The DTO model</param>
    /// <param name="schema">The original schema for validation rules</param>
    /// <returns>Validator model</returns>
    public ValidatorModel MapToValidator(DtoModel dtoModel, Schema schema)
    {
        var model = new ValidatorModel
        {
            ClassName = $"{dtoModel.ClassName}Validator",
            DtoClassName = dtoModel.ClassName,
            Namespace = dtoModel.Namespace.Replace("DTOs", "Validators"),
            Description = dtoModel.Description
        };

        foreach (var property in schema.Properties)
        {
            var rules = BuildValidationRules(property.Key, property.Value, schema.Required.Contains(property.Key));
            if (rules.Any())
            {
                model.ValidationRules.Add(new ValidationRuleModel
                {
                    PropertyName = ToCSharpPropertyName(property.Key),
                    Rules = rules
                });
            }
        }

        return model;
    }

    private List<string> BuildValidationRules(string propertyName, Schema propertySchema, bool isRequired)
    {
        var rules = new List<string>();

        if (isRequired)
        {
            rules.Add(".NotNull()");
            if (propertySchema.Type == "string")
            {
                rules.Add(".NotEmpty()");
            }
        }

        if (propertySchema.Type == "string")
        {
            if (propertySchema.MinLength.HasValue)
            {
                rules.Add($".MinimumLength({propertySchema.MinLength})");
            }
            if (propertySchema.MaxLength.HasValue)
            {
                rules.Add($".MaximumLength({propertySchema.MaxLength})");
            }
            if (!string.IsNullOrEmpty(propertySchema.Pattern))
            {
                rules.Add($".Matches(@\"{propertySchema.Pattern}\")");
            }
        }

        if (propertySchema.Type == "integer" || propertySchema.Type == "number")
        {
            if (propertySchema.Minimum.HasValue)
            {
                var method = propertySchema.ExclusiveMinimum == true ? "GreaterThan" : "GreaterThanOrEqualTo";
                rules.Add($".{method}({propertySchema.Minimum})");
            }
            if (propertySchema.Maximum.HasValue)
            {
                var method = propertySchema.ExclusiveMaximum == true ? "LessThan" : "LessThanOrEqualTo";
                rules.Add($".{method}({propertySchema.Maximum})");
            }
        }

        if (propertySchema.Type == "array")
        {
            if (propertySchema.MinItems.HasValue)
            {
                rules.Add($".Must(list => list.Count >= {propertySchema.MinItems}).WithMessage(\"Must have at least {propertySchema.MinItems} items\")");
            }
            if (propertySchema.MaxItems.HasValue)
            {
                rules.Add($".Must(list => list.Count <= {propertySchema.MaxItems}).WithMessage(\"Must have at most {propertySchema.MaxItems} items\")");
            }
        }

        return rules;
    }

    private string MapSwaggerTypeToCSharp(Schema schema)
    {
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            // Extract class name from reference
            var refParts = schema.Ref.Split('/');
            return ToCSharpClassName(refParts.Last());
        }

        return schema.Type switch
        {
            "string" => schema.Format switch
            {
                "date" => "DateOnly",
                "date-time" => "DateTime",
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
            "array" => $"List<{MapSwaggerTypeToCSharp(schema.Items ?? new Schema { Type = "object" })}>",
            "object" => schema.Properties.Any() ? "object" : "Dictionary<string, object>",
            _ => "object"
        };
    }

    private bool IsValueType(Schema schema)
    {
        var csharpType = MapSwaggerTypeToCSharp(schema);
        return csharpType switch
        {
            "int" or "long" or "float" or "double" or "decimal" or "bool" or "DateTime" or "DateOnly" or "Guid" => true,
            _ => false
        };
    }

    private object? GetDefaultValue(Schema schema)
    {
        if (schema.Default != null)
        {
            return FormatDefaultValue(schema.Default, schema.Type);
        }

        return null;
    }

    private object? FormatDefaultValue(object value, string type)
    {
        return type switch
        {
            "string" => $"\"{value}\"",
            "boolean" => value.ToString()?.ToLower(),
            _ => value
        };
    }

    private static string ToCSharpClassName(string name)
    {
        return TitleCase(name);
    }

    private static string ToCSharpPropertyName(string name)
    {
        return TitleCase(name);
    }

    private static string TitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle camelCase, snake_case, and kebab-case
        var parts = input.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 1)
        {
            // Handle camelCase by inserting spaces before capitals
            var camelCasePattern = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
            parts = camelCasePattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        
        var result = string.Join("", parts.Select(part => 
            char.ToUpper(part[0]) + part.Substring(1)));

        return result;
    }
}