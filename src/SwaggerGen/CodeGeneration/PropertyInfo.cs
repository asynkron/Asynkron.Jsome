namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents a property in a generated DTO class
/// </summary>
public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string JsonPropertyName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public string? DefaultValue { get; set; }
    public List<ValidationRule> ValidationRules { get; set; } = [];
    
    // Additional validation properties for arrays, objects, and numbers
    public int? MaxItems { get; set; }
    public int? MinItems { get; set; }
    public bool? UniqueItems { get; set; }
    public int? MaxProperties { get; set; }
    public int? MinProperties { get; set; }
    public decimal? MultipleOf { get; set; }
    public List<object> EnumValues { get; set; } = [];
    
    // Enum generation properties
    public string? EnumTypeName { get; set; }
    public string? ConstantsClassName { get; set; }
    public bool IsEnum => EnumValues.Count > 0;
    
    // Modern C# features
    public bool IsNullable { get; set; }
    public bool UseRequiredKeyword { get; set; }
    
    // System.Text.Json and enhanced validation features
    public bool UseSystemTextJson { get; set; }
    public bool UseEnhancedValidation { get; set; }
    public bool HasStringLength => MinLength.HasValue || MaxLength.HasValue;
    
    // Swashbuckle attributes support
    public bool UseSwashbuckleAttributes { get; set; }
    public string? SwaggerSchemaDescription { get; set; }
    public string? SwaggerFormat { get; set; }
    public string? SwaggerExample { get; set; }
    public bool SwaggerNullable { get; set; }
}

/// <summary>
/// Represents a validation rule for FluentValidation
/// </summary>
public class ValidationRule
{
    public string Rule { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = [];
    public string? Message { get; set; }
}