using HandlebarsDotNet;
using SwaggerGen.Models;
using SwaggerGen.Configuration;
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
    private readonly string _enumTemplate;
    private readonly string _constantsTemplate;
    private readonly CodeGenerationOptions _options;
    private readonly ModifierConfiguration? _modifierConfig;

    public CodeGenerator(CodeGenerationOptions? options = null)
    {
        _options = options ?? new CodeGenerationOptions();
        _handlebars = Handlebars.Create();
        
        // Load modifier configuration if specified
        _modifierConfig = LoadModifierConfiguration();
        
        // Load templates
        var basePath = GetTemplatesPath();
        var requiredTemplates = new[]
        {
            ("DTO.hbs", "DTO template"),
            ("Validator.hbs", "Validator template"), 
            ("Enum.hbs", "Enum template"),
            ("Constants.hbs", "Constants template")
        };

        var missingTemplates = new List<string>();

        foreach (var (fileName, description) in requiredTemplates)
        {
            var templatePath = Path.Combine(basePath, fileName);
            if (!File.Exists(templatePath))
            {
                missingTemplates.Add($"  - {description} ({fileName})");
            }
        }

        if (missingTemplates.Any())
        {
            throw new FileNotFoundException(
                $"Required template files not found in directory: {basePath}\n" +
                $"Missing templates:\n{string.Join("\n", missingTemplates)}\n\n" +
                "Make sure all required .hbs template files are present in the template directory.\n" +
                "To specify a custom template directory, use the --template-dir option.");
        }

        // Load all templates
        _dtoTemplate = File.ReadAllText(Path.Combine(basePath, "DTO.hbs"));
        _validatorTemplate = File.ReadAllText(Path.Combine(basePath, "Validator.hbs"));
        _enumTemplate = File.ReadAllText(Path.Combine(basePath, "Enum.hbs"));
        _constantsTemplate = File.ReadAllText(Path.Combine(basePath, "Constants.hbs"));
    }

    private ModifierConfiguration? LoadModifierConfiguration()
    {
        // If configuration instance is provided directly, use it
        if (_options.ModifierConfiguration != null)
            return _options.ModifierConfiguration;

        // If configuration path is provided, load from file
        if (!string.IsNullOrWhiteSpace(_options.ModifierConfigurationPath))
        {
            try
            {
                return ConfigurationLoader.Load(_options.ModifierConfigurationPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load modifier configuration from '{_options.ModifierConfigurationPath}': {ex.Message}", ex);
            }
        }

        return null;
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
        var enumInfos = new Dictionary<string, EnumInfo>();
        var constantsInfos = new Dictionary<string, ConstantsInfo>();
        
        // Apply global configuration settings
        if (_modifierConfig?.Global != null)
        {
            if (!string.IsNullOrEmpty(_modifierConfig.Global.Namespace))
                targetNamespace = _modifierConfig.Global.Namespace;
        }
        
        // First pass: collect all enum properties and generate enum/constants types
        if (_options.GenerateEnumTypes)
        {
            foreach (var definition in document.Definitions)
            {
                CollectEnumTypesFromSchema(definition.Key, definition.Value, targetNamespace, document.Definitions, enumInfos, constantsInfos);
            }
            
            // Generate enum types
            foreach (var enumInfo in enumInfos.Values)
            {
                var enumCode = GenerateEnum(enumInfo);
                result.EnumTypes.Add(enumInfo.EnumName, enumCode);
            }
            
            // Generate constants classes
            foreach (var constantsInfo in constantsInfos.Values)
            {
                var constantsCode = GenerateConstants(constantsInfo);
                result.ConstantClasses.Add(constantsInfo.ClassName, constantsCode);
            }
        }
        
        // Second pass: generate DTOs and validators
        foreach (var definition in document.Definitions)
        {
            // Check if this class should be included based on configuration
            var classPath = definition.Key;
            if (_modifierConfig != null && !_modifierConfig.IsIncluded(classPath))
                continue;
            
            var classInfo = ConvertSchemaToClassInfo(definition.Key, definition.Value, targetNamespace, document.Definitions, enumInfos, constantsInfos, classPath);
            
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

    private string GenerateEnum(EnumInfo enumInfo)
    {
        var template = _handlebars.Compile(_enumTemplate);
        return template(enumInfo);
    }

    private string GenerateConstants(ConstantsInfo constantsInfo)
    {
        var template = _handlebars.Compile(_constantsTemplate);
        return template(constantsInfo);
    }

    private void CollectEnumTypesFromSchema(string schemaName, Schema schema, string targetNamespace, 
        Dictionary<string, Schema> allDefinitions,
        Dictionary<string, EnumInfo> enumInfos,
        Dictionary<string, ConstantsInfo> constantsInfos)
    {
        if (schema.Properties == null) return;

        foreach (var property in schema.Properties)
        {
            CollectEnumFromProperty(schemaName, property.Key, property.Value, targetNamespace, enumInfos, constantsInfos);
        }

        // Handle allOf inheritance
        if (schema.AllOf?.Any() == true)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                if (!string.IsNullOrEmpty(allOfSchema?.Ref))
                {
                    var refName = allOfSchema.Ref.Replace("#/definitions/", "");
                    if (allDefinitions.TryGetValue(refName, out var refSchema))
                    {
                        CollectEnumTypesFromSchema(refName, refSchema, targetNamespace, allDefinitions, enumInfos, constantsInfos);
                    }
                }
                else if (allOfSchema?.Properties != null)
                {
                    foreach (var property in allOfSchema.Properties)
                    {
                        CollectEnumFromProperty(schemaName, property.Key, property.Value, targetNamespace, enumInfos, constantsInfos);
                    }
                }
            }
        }
    }

    private void CollectEnumFromProperty(string schemaName, string propertyName, Schema propertySchema, string targetNamespace,
        Dictionary<string, EnumInfo> enumInfos,
        Dictionary<string, ConstantsInfo> constantsInfos)
    {
        if (propertySchema.Enum == null || propertySchema.Enum.Count == 0) return;

        var propertyType = propertySchema.Type?.ToLower();
        
        if (propertyType == "integer")
        {
            // Generate enum for integer enums
            var enumName = ApplyTypeNameFormatting($"{ToPascalCase(schemaName)}{ToPascalCase(propertyName)}");
            if (!enumInfos.ContainsKey(enumName))
            {
                var enumInfo = new EnumInfo
                {
                    EnumName = enumName,
                    Namespace = targetNamespace,
                    Description = $"Enum values for {schemaName}.{propertyName}",
                    Values = propertySchema.Enum.Select((value, index) => new EnumValueInfo
                    {
                        Name = GenerateEnumValueName(value),
                        Value = value,
                        Description = $"Value: {value}"
                    }).ToList()
                };
                enumInfos.Add(enumName, enumInfo);
            }
        }
        else if (propertyType == "string")
        {
            // Generate constants class for string enums
            var className = ApplyTypeNameFormatting($"{ToPascalCase(schemaName)}{ToPascalCase(propertyName)}Constants");
            if (!constantsInfos.ContainsKey(className))
            {
                var constantsInfo = new ConstantsInfo
                {
                    ClassName = className,
                    Namespace = targetNamespace,
                    Description = $"Constants for {schemaName}.{propertyName}",
                    Constants = propertySchema.Enum.Select(value => new ConstantInfo
                    {
                        Name = GenerateConstantName(value.ToString() ?? ""),
                        Value = value.ToString() ?? "",
                        Description = $"Value: {value}"
                    }).ToList()
                };
                constantsInfos.Add(className, constantsInfo);
            }
        }
    }

    private string GenerateEnumValueName(object value)
    {
        var stringValue = value.ToString() ?? "";
        
        // Convert to PascalCase and ensure it's a valid C# identifier
        var name = ToPascalCase(stringValue);
        
        // Handle numeric values by prefixing with "Value"
        if (char.IsDigit(name[0]))
        {
            name = "Value" + name;
        }
        
        // Replace invalid characters
        name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "");
        
        return string.IsNullOrEmpty(name) ? "Unknown" : name;
    }

    private string GenerateConstantName(string value)
    {
        // Convert to UPPER_CASE format for constants
        var name = value.Replace("-", "_").Replace(" ", "_").ToUpper();
        
        // Ensure it starts with a letter or underscore
        if (name.Length > 0 && char.IsDigit(name[0]))
        {
            name = "VALUE_" + name;
        }
        
        // Replace invalid characters
        name = System.Text.RegularExpressions.Regex.Replace(name, @"[^A-Z0-9_]", "");
        
        return string.IsNullOrEmpty(name) ? "UNKNOWN" : name;
    }

    private ClassInfo ConvertSchemaToClassInfo(string name, Schema schema, string targetNamespace, Dictionary<string, Schema> allDefinitions)
    {
        return ConvertSchemaToClassInfo(name, schema, targetNamespace, allDefinitions, new Dictionary<string, EnumInfo>(), new Dictionary<string, ConstantsInfo>(), name);
    }

    private ClassInfo ConvertSchemaToClassInfo(string name, Schema schema, string targetNamespace, Dictionary<string, Schema> allDefinitions,
        Dictionary<string, EnumInfo> enumInfos, Dictionary<string, ConstantsInfo> constantsInfos, string propertyPath = "")
    {
        if (string.IsNullOrEmpty(propertyPath))
            propertyPath = name;
            
        var classInfo = new ClassInfo
        {
            ClassName = ApplyTypeNameFormatting(ToPascalCase(name)),
            Namespace = targetNamespace,
            Description = schema.Description ?? ""
        };

        // Apply configuration rule for class description override
        var classRule = _modifierConfig?.GetRule(propertyPath);
        if (classRule != null && !string.IsNullOrEmpty(classRule.Description))
        {
            classInfo.Description = classRule.Description;
        }

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
                                var propPath = $"{propertyPath}.{property.Key}";
                                
                                // Check if property should be included
                                if (_modifierConfig != null && !_modifierConfig.IsIncluded(propPath))
                                    continue;
                                
                                var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, refSchema.Required ?? new List<string>(), enumInfos, constantsInfos, propPath);
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
                        var propPath = $"{propertyPath}.{property.Key}";
                        
                        // Check if property should be included
                        if (_modifierConfig != null && !_modifierConfig.IsIncluded(propPath))
                            continue;
                        
                        var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, allOfSchema.Required ?? new List<string>(), enumInfos, constantsInfos, propPath);
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
                    var propPath = $"{propertyPath}.{property.Key}";
                    
                    // Check if property should be included
                    if (_modifierConfig != null && !_modifierConfig.IsIncluded(propPath))
                        continue;
                    
                    var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, schema.Required ?? new List<string>(), enumInfos, constantsInfos, propPath);
                    classInfo.Properties.Add(propertyInfo);
                }
            }
        }

        return classInfo;
    }

    private PropertyInfo ConvertPropertyToPropertyInfo(string name, Schema schema, List<string> requiredFields)
    {
        // Handle null schema
        if (schema == null)
        {
            return new PropertyInfo
            {
                Name = ToPascalCase(name),
                JsonPropertyName = name,
                Type = "object",
                Description = "",
                IsRequired = requiredFields?.Contains(name) ?? false,
                ValidationRules = new List<ValidationRule>()
            };
        }

        var propertyInfo = new PropertyInfo
        {
            Name = ToPascalCase(name),
            JsonPropertyName = name,
            Type = MapSwaggerTypeToCSharpType(schema),
            Description = schema.Description ?? "",
            IsRequired = requiredFields?.Contains(name) ?? false,
            MaxLength = schema.MaxLength,
            MinLength = schema.MinLength,
            MaxItems = schema.MaxItems,
            MinItems = schema.MinItems,
            UniqueItems = schema.UniqueItems,
            MaxProperties = schema.MaxProperties,
            MinProperties = schema.MinProperties,
            MultipleOf = schema.MultipleOf,
            EnumValues = schema.Enum ?? new List<object>()
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

    private PropertyInfo ConvertPropertyToPropertyInfo(string parentSchemaName, string name, Schema schema, List<string> requiredFields,
        Dictionary<string, EnumInfo> enumInfos, Dictionary<string, ConstantsInfo> constantsInfos, string propertyPath = "")
    {
        // For backward compatibility, fall back to the original method if not using enum generation and no modifier config
        if (!_options.GenerateEnumTypes && _modifierConfig == null)
        {
            return ConvertPropertyToPropertyInfo(name, schema, requiredFields);
        }

        // Handle null schema
        if (schema == null)
        {
            return new PropertyInfo
            {
                Name = ToPascalCase(name),
                JsonPropertyName = name,
                Type = "object",
                Description = "",
                IsRequired = requiredFields?.Contains(name) ?? false,
                ValidationRules = new List<ValidationRule>()
            };
        }

        var propertyInfo = new PropertyInfo
        {
            Name = ToPascalCase(name),
            JsonPropertyName = name,
            Description = schema.Description ?? "",
            IsRequired = requiredFields?.Contains(name) ?? false,
            MaxLength = schema.MaxLength,
            MinLength = schema.MinLength,
            MaxItems = schema.MaxItems,
            MinItems = schema.MinItems,
            UniqueItems = schema.UniqueItems,
            MaxProperties = schema.MaxProperties,
            MinProperties = schema.MinProperties,
            MultipleOf = schema.MultipleOf,
            EnumValues = schema.Enum ?? new List<object>()
        };

        // Apply configuration rules
        var rule = _modifierConfig?.GetRule(propertyPath);
        if (rule != null)
        {
            // Override description if specified
            if (!string.IsNullOrEmpty(rule.Description))
            {
                propertyInfo.Description = rule.Description;
            }

            // Override type if specified
            if (!string.IsNullOrEmpty(rule.Type))
            {
                propertyInfo.Type = rule.Type;
                if (rule.Default != null)
                    propertyInfo.DefaultValue = FormatDefaultValue(rule.Default, rule.Type);
            }
            
            // Override default value if specified
            else if (rule.Default != null)
            {
                propertyInfo.DefaultValue = FormatDefaultValue(rule.Default, propertyInfo.Type);
            }

            // Apply validation overrides
            if (rule.Validation != null)
            {
                if (rule.Validation.Required.HasValue)
                    propertyInfo.IsRequired = rule.Validation.Required.Value;
                if (rule.Validation.MinLength.HasValue)
                    propertyInfo.MinLength = rule.Validation.MinLength.Value;
                if (rule.Validation.MaxLength.HasValue)
                    propertyInfo.MaxLength = rule.Validation.MaxLength.Value;
            }
        }

        // Handle enum type mapping first (only if type wasn't overridden by configuration)
        var typeOverridden = !string.IsNullOrEmpty(rule?.Type);
        if (!typeOverridden && schema.Enum != null && schema.Enum.Count > 0)
        {
            var propertyType = schema.Type?.ToLower();
            
            if (propertyType == "integer")
            {
                // Use generated enum type
                var enumName = ApplyTypeNameFormatting($"{ToPascalCase(parentSchemaName)}{ToPascalCase(name)}");
                if (enumInfos.ContainsKey(enumName))
                {
                    propertyInfo.Type = enumName;
                    propertyInfo.EnumTypeName = enumName;
                    typeOverridden = true;
                }
            }
            else if (propertyType == "string")
            {
                // Property stays as string, but we reference the constants class
                var constantsClassName = ApplyTypeNameFormatting($"{ToPascalCase(parentSchemaName)}{ToPascalCase(name)}Constants");
                if (constantsInfos.ContainsKey(constantsClassName))
                {
                    propertyInfo.ConstantsClassName = constantsClassName;
                }
            }
        }

        // Set the type if not overridden by enum handling
        if (!typeOverridden)
        {
            propertyInfo.Type = MapSwaggerTypeToCSharpType(schema);
        }

        // Generate validation rules
        propertyInfo.ValidationRules = GenerateValidationRules(schema, propertyInfo.IsRequired, propertyInfo.EnumTypeName, propertyInfo.ConstantsClassName, rule);

        // Set default value if available
        if (schema.Default != null)
        {
            propertyInfo.DefaultValue = FormatDefaultValue(schema.Default, propertyInfo.Type);
        }

        return propertyInfo;
    }

    private List<ValidationRule> GenerateValidationRules(Schema schema, bool isRequired)
    {
        return GenerateValidationRules(schema, isRequired, null, null, null);
    }

    private List<ValidationRule> GenerateValidationRules(Schema schema, bool isRequired, string? enumTypeName, string? constantsClassName, PropertyRule? configRule = null)
    {
        var rules = new List<ValidationRule>();

        // Override schema validation with configuration if provided
        var effectiveMinLength = configRule?.Validation?.MinLength ?? schema.MinLength;
        var effectiveMaxLength = configRule?.Validation?.MaxLength ?? schema.MaxLength;
        var effectiveMinimum = configRule?.Validation?.Minimum ?? schema.Minimum;
        var effectiveMaximum = configRule?.Validation?.Maximum ?? schema.Maximum;
        var effectivePattern = configRule?.Validation?.Pattern ?? schema.Pattern;
        var customMessage = configRule?.Validation?.Message;

        // Required field validation
        if (isRequired)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "NotEmpty", 
                Parameters = new List<string>(),
                Message = customMessage ?? "This field is required"
            });
        }

        // String length validation
        if (effectiveMinLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MinimumLength", 
                Parameters = new List<string> { effectiveMinLength.Value.ToString() },
                Message = customMessage ?? $"Must be at least {effectiveMinLength.Value} characters long"
            });
        }

        if (effectiveMaxLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MaximumLength", 
                Parameters = new List<string> { effectiveMaxLength.Value.ToString() },
                Message = customMessage ?? $"Must be no more than {effectiveMaxLength.Value} characters long"
            });
        }

        // Pattern validation
        if (!string.IsNullOrEmpty(effectivePattern))
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "Matches", 
                Parameters = new List<string> { $"\"{effectivePattern}\"" },
                Message = customMessage ?? $"Must match pattern: {effectivePattern}"
            });
        }

        // Numeric range validation
        if (effectiveMinimum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "GreaterThanOrEqualTo", 
                Parameters = new List<string> { effectiveMinimum.Value.ToString() },
                Message = customMessage ?? $"Must be greater than or equal to {effectiveMinimum.Value}"
            });
        }

        if (effectiveMaximum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "LessThanOrEqualTo", 
                Parameters = new List<string> { effectiveMaximum.Value.ToString() },
                Message = customMessage ?? $"Must be less than or equal to {effectiveMaximum.Value}"
            });
        }

        // Array validation - maintain backward compatibility when no configuration provided
        if (schema.MinItems.HasValue)
        {
            if (configRule == null)
            {
                // Original format for backward compatibility
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => x.Count >= {schema.MinItems.Value}" },
                    Message = $"Must contain at least {schema.MinItems.Value} items"
                });
            }
            else
            {
                // Enhanced format with null checks when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => x == null || x.Count() >= {schema.MinItems.Value}" },
                    Message = customMessage ?? $"Must contain at least {schema.MinItems.Value} items"
                });
            }
        }

        if (schema.MaxItems.HasValue)
        {
            if (configRule == null)
            {
                // Original format for backward compatibility
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => x.Count <= {schema.MaxItems.Value}" },
                    Message = $"Must contain at most {schema.MaxItems.Value} items"
                });
            }
            else
            {
                // Enhanced format with null checks when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => x == null || x.Count() <= {schema.MaxItems.Value}" },
                    Message = customMessage ?? $"Must contain no more than {schema.MaxItems.Value} items"
                });
            }
        }

        if (schema.UniqueItems == true)
        {
            if (configRule == null)
            {
                // Original format for backward compatibility
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { "x => x.Distinct().Count() == x.Count" },
                    Message = "All items must be unique"
                });
            }
            else
            {
                // Enhanced format when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { "x => x.Distinct().Count() == x.Count()" },
                    Message = customMessage ?? "All items must be unique"
                });
            }
        }

        // Number validation constraints
        if (schema.MultipleOf.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "Must", 
                Parameters = new List<string> { $"x => x % {schema.MultipleOf.Value} == 0" },
                Message = customMessage ?? $"Must be a multiple of {schema.MultipleOf.Value}"
            });
        }

        // Enum validation - improved for new enum types
        if (schema.Enum != null && schema.Enum.Count > 0)
        {
            if (!string.IsNullOrEmpty(enumTypeName))
            {
                // For integer enums, use Enum.IsDefined
                rules.Add(new ValidationRule 
                {
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => Enum.IsDefined(typeof({enumTypeName}), x)" },
                    Message = customMessage ?? $"Must be a valid {enumTypeName} value"
                });
            }
            else if (!string.IsNullOrEmpty(constantsClassName))
            {
                // For string enums with constants class, use direct string values for backward compatibility
                var enumValues = string.Join(", ", schema.Enum.Select(e => $"\"{e}\""));
                rules.Add(new ValidationRule 
                {
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => new[] {{ {enumValues} }}.Contains(x)" },
                    Message = customMessage ?? $"Must be one of: {enumValues}"
                });
            }
            else
            {
                // Fallback to original validation for backward compatibility
                var enumValues = string.Join(", ", schema.Enum.Select(e => $"\"{e}\""));
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = new List<string> { $"x => new[] {{ {enumValues} }}.Contains(x.ToString())" },
                    Message = customMessage ?? $"Must be one of: {enumValues}"
                });
            }
        }

        return rules;
    }

    private string MapSwaggerTypeToCSharpType(Schema schema)
    {
        // Handle null schema
        if (schema == null)
        {
            return "object";
        }

        // Handle $ref references
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var refName = schema.Ref.Replace("#/definitions/", "");
            return ApplyTypeNameFormatting(refName);
        }

        // Handle cases where Type might be null or empty
        var schemaType = schema.Type?.ToLower() ?? "object";
        
        return schemaType switch
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

    /// <summary>
    /// Applies global type name prefix and suffix formatting to a type name
    /// </summary>
    /// <param name="typeName">The base type name</param>
    /// <returns>The formatted type name with prefix/suffix applied</returns>
    private string ApplyTypeNameFormatting(string typeName)
    {
        var prefix = _modifierConfig?.Global?.TypeNamePrefix ?? "";
        var suffix = _modifierConfig?.Global?.TypeNameSuffix ?? "";
        
        return $"{prefix}{typeName}{suffix}";
    }

    private string GetTemplatesPath()
    {
        // Priority 1: Check user-specified template directory
        if (!string.IsNullOrWhiteSpace(_options.TemplateDirectory))
        {
            if (Directory.Exists(_options.TemplateDirectory))
            {
                return _options.TemplateDirectory;
            }
            throw new DirectoryNotFoundException($"Custom template directory not found: {_options.TemplateDirectory}");
        }

        // Priority 2: Check tool installation directory (NuGet contentFiles location)
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var templatesPath = Path.Combine(basePath, "Templates");
        
        if (Directory.Exists(templatesPath))
        {
            return templatesPath;
        }

        // Priority 3: Check contentFiles location for NuGet package
        var contentFilesPath = Path.Combine(basePath, "contentFiles", "any", "any", "Templates");
        if (Directory.Exists(contentFilesPath))
        {
            return contentFilesPath;
        }

        // Priority 4: Development-time fallback - walk up directory tree to find source templates
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

        // Priority 5: Last resort - check if templates are in the current directory
        if (Directory.Exists("Templates"))
        {
            return Path.GetFullPath("Templates");
        }

        throw new DirectoryNotFoundException(
            "Template files not found. Checked locations:\n" +
            $"  - Custom directory: {_options.TemplateDirectory ?? "(not specified)"}\n" +
            $"  - Tool directory: {templatesPath}\n" +
            $"  - ContentFiles directory: {contentFilesPath}\n" +
            $"  - Source directory search from: {Directory.GetCurrentDirectory()}\n" +
            $"  - Current directory: Templates\n\n" +
            "To specify a custom template directory, use the --template-dir option or set the TemplateDirectory property in CodeGenerationOptions.");
    }
}