using System.Globalization;
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
    private readonly Dictionary<string, string> _templates;
    private readonly CodeGenerationOptions _options;
    private readonly ModifierConfiguration? _modifierConfig;

    public CodeGenerator(CodeGenerationOptions? options = null)
    {
        _options = options ?? new CodeGenerationOptions();
        _handlebars = Handlebars.Create();
        _templates = new Dictionary<string, string>();
        
        // Load modifier configuration if specified
        _modifierConfig = LoadModifierConfiguration();
        
        // Load templates
        var basePath = GetTemplatesPath();
        
        // Determine which templates to load
        var templatesToLoad = GetTemplatesToLoad();
        
        var missingTemplates = new List<string>();

        foreach (var (fileName, description) in templatesToLoad)
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

        // Load all templates into the dictionary
        foreach (var (fileName, description) in templatesToLoad)
        {
            var templatePath = Path.Combine(basePath, fileName);
            _templates[fileName] = File.ReadAllText(templatePath);
        }
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

    private List<(string fileName, string description)> GetTemplatesToLoad()
    {
        // If custom templates are specified, use only those
        if (_options.CustomTemplateFiles != null && _options.CustomTemplateFiles.Count > 0)
        {
            return _options.CustomTemplateFiles
                .Select(fileName => (fileName, $"Custom template: {fileName}"))
                .ToList();
        }

        // Otherwise, use the standard templates
        var standardTemplates = new List<(string, string)>
        {
            ("DTO.hbs", "DTO template"),
            ("Validator.hbs", "Validator template"), 
            ("Enum.hbs", "Enum template"),
            ("Constants.hbs", "Constants template")
        };

        // Add DTORecord template if records are enabled
        if (_options.GenerateRecords)
        {
            standardTemplates.Add(("DTORecord.hbs", "DTO Record template"));
        }

        return standardTemplates;
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
        if (_options.GenerateEnumTypes && (_templates.ContainsKey("Enum.hbs") || _templates.ContainsKey("Constants.hbs")))
        {
            foreach (var definition in document.Definitions)
            {
                CollectEnumTypesFromSchema(definition.Key, definition.Value, targetNamespace, document.Definitions, enumInfos, constantsInfos);
            }
            
            // Generate enum types only if template is available
            if (_templates.ContainsKey("Enum.hbs"))
            {
                foreach (var enumInfo in enumInfos.Values)
                {
                    var enumCode = GenerateEnum(enumInfo);
                    result.EnumTypes.Add(enumInfo.EnumName, enumCode);
                }
            }
            
            // Generate constants classes only if template is available
            if (_templates.ContainsKey("Constants.hbs"))
            {
                foreach (var constantsInfo in constantsInfos.Values)
                {
                    var constantsCode = GenerateConstants(constantsInfo);
                    result.ConstantClasses.Add(constantsInfo.ClassName, constantsCode);
                }
            }
        }
        
        // Second pass: generate DTOs and validators based on available templates
        foreach (var definition in document.Definitions)
        {
            // Check if this class should be included based on configuration
            var classPath = definition.Key;
            if (_modifierConfig != null && !_modifierConfig.IsIncluded(classPath))
                continue;
            
            var classInfo = ConvertSchemaToClassInfo(definition.Key, definition.Value, targetNamespace, document.Definitions, enumInfos, constantsInfos, classPath);
            
            // Generate DTO only if template is available
            if (_templates.ContainsKey("DTO.hbs") || _templates.ContainsKey("DTORecord.hbs"))
            {
                try
                {
                    var dtoCode = GenerateDto(classInfo);
                    result.DtoClasses.Add(definition.Key, dtoCode);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("No DTO template available"))
                {
                    // Skip DTO generation if no suitable template is available
                }
            }
            
            // Generate Validator only if template is available
            if (_templates.ContainsKey("Validator.hbs"))
            {
                try
                {
                    var validatorCode = GenerateValidator(classInfo);
                    result.Validators.Add(definition.Key, validatorCode);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("No Validator template available"))
                {
                    // Skip validator generation if template is not available
                }
            }
        }
        
        // Third pass: handle any custom templates that aren't part of the standard set
        if (_options.CustomTemplateFiles != null && _options.CustomTemplateFiles.Count > 0)
        {
            var standardTemplateNames = new HashSet<string> { "DTO.hbs", "DTORecord.hbs", "Validator.hbs", "Enum.hbs", "Constants.hbs" };
            var customTemplates = _options.CustomTemplateFiles
                .Where(templateFile => !standardTemplateNames.Contains(templateFile))
                .ToList();
                
            foreach (var customTemplate in customTemplates)
            {
                if (_templates.ContainsKey(customTemplate))
                {
                    foreach (var definition in document.Definitions)
                    {
                        // Check if this class should be included based on configuration
                        var classPath = definition.Key;
                        if (_modifierConfig != null && !_modifierConfig.IsIncluded(classPath))
                            continue;
                        
                        var classInfo = ConvertSchemaToClassInfo(definition.Key, definition.Value, targetNamespace, document.Definitions, enumInfos, constantsInfos, classPath);
                        
                        try
                        {
                            var template = _handlebars.Compile(_templates[customTemplate]);
                            var generatedCode = template(classInfo);
                            
                            // Store custom template output in a separate collection
                            var templateKey = $"{definition.Key}_{Path.GetFileNameWithoutExtension(customTemplate)}";
                            result.CustomTemplateOutput[templateKey] = generatedCode;
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Error processing custom template '{customTemplate}' for class '{definition.Key}': {ex.Message}", ex);
                        }
                    }
                }
            }
        }
        
        return result;
    }

    private string GenerateDto(ClassInfo classInfo)
    {
        // Use record template if records are enabled and available
        if (_options.GenerateRecords && _templates.ContainsKey("DTORecord.hbs"))
        {
            var recordTemplate = _handlebars.Compile(_templates["DTORecord.hbs"]);
            return recordTemplate(classInfo);
        }
        
        // Use standard DTO template if available
        if (_templates.ContainsKey("DTO.hbs"))
        {
            var template = _handlebars.Compile(_templates["DTO.hbs"]);
            return template(classInfo);
        }
        
        // If no standard DTO template is available, throw an error
        throw new InvalidOperationException("No DTO template available for code generation. Ensure DTO.hbs or DTORecord.hbs is included in your templates.");
    }

    private string GenerateValidator(ClassInfo classInfo)
    {
        if (_templates.ContainsKey("Validator.hbs"))
        {
            var template = _handlebars.Compile(_templates["Validator.hbs"]);
            return template(classInfo);
        }
        
        throw new InvalidOperationException("No Validator template available for code generation. Ensure Validator.hbs is included in your templates.");
    }

    private string GenerateEnum(EnumInfo enumInfo)
    {
        if (_templates.ContainsKey("Enum.hbs"))
        {
            var template = _handlebars.Compile(_templates["Enum.hbs"]);
            return template(enumInfo);
        }
        
        throw new InvalidOperationException("No Enum template available for code generation. Ensure Enum.hbs is included in your templates.");
    }

    private string GenerateConstants(ConstantsInfo constantsInfo)
    {
        if (_templates.ContainsKey("Constants.hbs"))
        {
            var template = _handlebars.Compile(_templates["Constants.hbs"]);
            return template(constantsInfo);
        }
        
        throw new InvalidOperationException("No Constants template available for code generation. Ensure Constants.hbs is included in your templates.");
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
                        Name = GenerateConstantName(Convert.ToString( value, CultureInfo.InvariantCulture) ?? ""),
                        Value =   Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
                        Description = $"Value: {Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""}"
                    }).ToList()
                };
                constantsInfos.Add(className, constantsInfo);
            }
        }
    }

    private string GenerateEnumValueName(object value)
    {
        var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
        
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
            Description = schema.Description ?? "",
            UseSystemTextJson = _options.UseSystemTextJson,
            UseSwashbuckleAttributes = _options.UseSwashbuckleAttributes
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
                                
                                var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, refSchema.Required ??
                                    [], enumInfos, constantsInfos, propPath);
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
                        
                        var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, allOfSchema.Required ??
                            [], enumInfos, constantsInfos, propPath);
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
                    
                    var propertyInfo = ConvertPropertyToPropertyInfo(name, property.Key, property.Value, schema.Required ??
                        [], enumInfos, constantsInfos, propPath);
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
            var nullSchemaRequired = requiredFields?.Contains(name) ?? false;
            var nullSchemaNullable = _options.UseNullableReferenceTypes && !nullSchemaRequired;
            return new PropertyInfo
            {
                Name = ToPascalCase(name),
                JsonPropertyName = name,
                Type = nullSchemaNullable ? "object?" : "object",
                Description = "",
                IsRequired = nullSchemaRequired,
                IsNullable = nullSchemaNullable,
                UseRequiredKeyword = _options.UseRequiredKeyword && nullSchemaRequired,
                ValidationRules = [],
                UseSystemTextJson = _options.UseSystemTextJson,
                UseEnhancedValidation = _options.UseSystemTextJson,
                UseSwashbuckleAttributes = _options.UseSwashbuckleAttributes,
                SwaggerNullable = nullSchemaNullable
            };
        }

        var isRequired = requiredFields?.Contains(name) ?? false;
        var isNullable = _options.UseNullableReferenceTypes && !isRequired;

        var propertyInfo = new PropertyInfo
        {
            Name = ToPascalCase(name),
            JsonPropertyName = name,
            Type = MapSwaggerTypeToCSharpType(schema, isNullable),
            Description = schema.Description ?? "",
            IsRequired = isRequired,
            IsNullable = isNullable,
            UseRequiredKeyword = _options.UseRequiredKeyword && isRequired,
            MaxLength = schema.MaxLength,
            MinLength = schema.MinLength,
            MaxItems = schema.MaxItems,
            MinItems = schema.MinItems,
            UniqueItems = schema.UniqueItems,
            MaxProperties = schema.MaxProperties,
            MinProperties = schema.MinProperties,
            MultipleOf = schema.MultipleOf,
            EnumValues = schema.Enum ?? [],
            UseSystemTextJson = _options.UseSystemTextJson,
            UseEnhancedValidation = _options.UseSystemTextJson,
            // Swashbuckle attributes
            UseSwashbuckleAttributes = _options.UseSwashbuckleAttributes,
            SwaggerSchemaDescription = _options.UseSwashbuckleAttributes ? schema.Description : null,
            SwaggerFormat = _options.UseSwashbuckleAttributes ? schema.Format : null,
            SwaggerExample = _options.UseSwashbuckleAttributes && schema.Example != null ? FormatExampleValue(schema.Example) : null,
            SwaggerNullable = isNullable
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
            var nullSchemaRequired2 = requiredFields?.Contains(name) ?? false;
            var nullSchemaNullable2 = _options.UseNullableReferenceTypes && !nullSchemaRequired2;
            return new PropertyInfo
            {
                Name = ToPascalCase(name),
                JsonPropertyName = name,
                Type = nullSchemaNullable2 ? "object?" : "object",
                Description = "",
                IsRequired = nullSchemaRequired2,
                IsNullable = nullSchemaNullable2,
                UseRequiredKeyword = _options.UseRequiredKeyword && nullSchemaRequired2,
                ValidationRules = [],
                UseSystemTextJson = _options.UseSystemTextJson,
                UseEnhancedValidation = _options.UseSystemTextJson,
                UseSwashbuckleAttributes = _options.UseSwashbuckleAttributes,
                SwaggerNullable = nullSchemaNullable2
            };
        }

        var isRequired = requiredFields?.Contains(name) ?? false;
        var isNullable = _options.UseNullableReferenceTypes && !isRequired;

        var propertyInfo = new PropertyInfo
        {
            Name = ToPascalCase(name),
            JsonPropertyName = name,
            Description = schema.Description ?? "",
            IsRequired = isRequired,
            IsNullable = isNullable,
            UseRequiredKeyword = _options.UseRequiredKeyword && isRequired,
            MaxLength = schema.MaxLength,
            MinLength = schema.MinLength,
            MaxItems = schema.MaxItems,
            MinItems = schema.MinItems,
            UniqueItems = schema.UniqueItems,
            MaxProperties = schema.MaxProperties,
            MinProperties = schema.MinProperties,
            MultipleOf = schema.MultipleOf,
            EnumValues = schema.Enum ?? [],
            UseSystemTextJson = _options.UseSystemTextJson,
            UseEnhancedValidation = _options.UseSystemTextJson,
            // Swashbuckle attributes
            UseSwashbuckleAttributes = _options.UseSwashbuckleAttributes,
            SwaggerSchemaDescription = _options.UseSwashbuckleAttributes ? schema.Description : null,
            SwaggerFormat = _options.UseSwashbuckleAttributes ? schema.Format : null,
            SwaggerExample = _options.UseSwashbuckleAttributes && schema.Example != null ? FormatExampleValue(schema.Example) : null,
            SwaggerNullable = isNullable
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
            propertyInfo.Type = MapSwaggerTypeToCSharpType(schema, propertyInfo.IsNullable);
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
                Parameters = [],
                Message = customMessage ?? "This field is required"
            });
        }

        // String length validation
        if (effectiveMinLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MinimumLength", 
                Parameters = [Convert.ToString( effectiveMinLength.Value, CultureInfo.InvariantCulture)],
                Message = customMessage ?? $"Must be at least {Convert.ToString(effectiveMinLength.Value, CultureInfo.InvariantCulture)  } characters long"
            });
        }

        if (effectiveMaxLength.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "MaximumLength", 
                Parameters = [Convert.ToString(effectiveMaxLength.Value, CultureInfo.InvariantCulture)],
                Message = customMessage ?? $"Must be no more than {Convert.ToString(effectiveMaxLength.Value, CultureInfo.InvariantCulture) } characters long"
            });
        }

        // Pattern validation
        if (!string.IsNullOrEmpty(effectivePattern))
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "Matches", 
                Parameters = [$"@\"{effectivePattern}\""],
                Message = customMessage ?? $"Must match pattern: {effectivePattern}"
            });
        }

        // Numeric range validation
        if (effectiveMinimum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "GreaterThanOrEqualTo", 
                Parameters = [effectiveMinimum.Value.ToString(CultureInfo.InvariantCulture)],
                Message = customMessage ?? $"Must be greater than or equal to {effectiveMinimum.Value.ToString(CultureInfo.InvariantCulture)}"
            });
        }

        if (effectiveMaximum.HasValue)
        {
            rules.Add(new ValidationRule 
            { 
                Rule = "LessThanOrEqualTo", 
                Parameters = [effectiveMaximum.Value.ToString(CultureInfo.InvariantCulture)],
                Message = customMessage ?? $"Must be less than or equal to {effectiveMaximum.Value.ToString(CultureInfo.InvariantCulture)}"
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
                    Parameters = [$"x => x.Count >= {schema.MinItems.Value}"],
                    Message = $"Must contain at least {schema.MinItems.Value} items"
                });
            }
            else
            {
                // Enhanced format with null checks when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = [$"x => x == null || x.Count() >= {schema.MinItems.Value}"],
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
                    Parameters = [$"x => x.Count <= {schema.MaxItems.Value}"],
                    Message = $"Must contain at most {schema.MaxItems.Value} items"
                });
            }
            else
            {
                // Enhanced format with null checks when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = [$"x => x == null || x.Count() <= {schema.MaxItems.Value}"],
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
                    Parameters = ["x => x.Distinct().Count() == x.Count"],
                    Message = "All items must be unique"
                });
            }
            else
            {
                // Enhanced format when using configuration
                rules.Add(new ValidationRule 
                { 
                    Rule = "Must", 
                    Parameters = ["x => x.Distinct().Count() == x.Count()"],
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
                Parameters = [$"x => x % {schema.MultipleOf.Value.ToString(CultureInfo.InvariantCulture)} == 0"],
                Message = customMessage ?? $"Must be a multiple of {schema.MultipleOf.Value.ToString(CultureInfo.InvariantCulture)}"
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
                    Parameters = [$"x => Enum.IsDefined(typeof({enumTypeName}), x)"],
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
                    Parameters = [$"x => new[] {{ {enumValues} }}.Contains(x)"],
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
                    Parameters = [$"x => new[] {{ {enumValues} }}.Contains(x.ToString())"],
                    Message = customMessage ?? $"Must be one of: {enumValues}"
                });
            }
        }

        return rules;
    }

    private string MapSwaggerTypeToCSharpType(Schema schema)
    {
        return MapSwaggerTypeToCSharpType(schema, false);
    }

    private string MapSwaggerTypeToCSharpType(Schema schema, bool isNullable)
    {
        // Handle null schema
        if (schema == null)
        {
            return isNullable ? "object?" : "object";
        }

        // Handle $ref references
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var refName = schema.Ref.Replace("#/definitions/", "");
            var typeName = ApplyTypeNameFormatting(refName);
            return isNullable ? $"{typeName}?" : typeName;
        }

        // Handle cases where Type might be null or empty
        var schemaType = schema.Type?.ToLower() ?? "object";
        
        var baseType = schemaType switch
        {
            "integer" => schema.Format == "int64" ? "long" : "int",
            "number" => schema.Format == "float" ? "float" : "decimal",
            "string" => schema.Format == "date-time" ? "DateTime" : "string",
            "boolean" => "bool",
            "array" => $"List<{MapSwaggerTypeToCSharpType(schema.Items ?? new Schema { Type = "object" })}>",
            "object" => "object",
            _ => "object"
        };

        // Apply nullability for reference types and value types that support it
        if (isNullable && _options.UseNullableReferenceTypes)
        {
            var isValueType = baseType is "int" or "long" or "float" or "decimal" or "bool";
            var isReferenceType = baseType is "string" or "DateTime" or "object" || baseType.StartsWith("List<");
            
            if (isValueType)
            {
                return $"{baseType}?";
            }
            else if (isReferenceType && baseType != "object")
            {
                return $"{baseType}?";
            }
        }

        return baseType;
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle cases like "NewPet" - don't split on capital letters, just ensure first letter is uppercase
        if (char.IsUpper(input[0]))
            return input;

        var words = input.Split(new char[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
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

        return Convert.ToString(result, CultureInfo.InvariantCulture) ?? "";
    }

    private string FormatDefaultValue(object value, string type)
    {
        var valueStr = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        return type.ToLower() switch
        {
            "string" => $"\"{valueStr}\"",
            "bool" => valueStr.ToLower(),
            _ => valueStr
        };
    }

    private string FormatExampleValue(object value)
    {
        if (value == null) return "null";
        
        var valueStr = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        
        // If it's already a JSON string (starts and ends with quotes), use as-is
        if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
        {
            return valueStr;
        }
        
        // For numeric values, booleans, etc., use as-is
        if (bool.TryParse(valueStr, out _) || 
            decimal.TryParse(valueStr, out _) ||
            valueStr == "null")
        {
            return valueStr.ToLower();
        }
        
        // For everything else, treat as string and escape properly for C# string literals
        return $"\"{valueStr.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
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