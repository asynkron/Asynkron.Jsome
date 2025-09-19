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
    public List<ValidationRule> ValidationRules { get; set; } = new();
}

/// <summary>
/// Represents a validation rule for FluentValidation
/// </summary>
public class ValidationRule
{
    public string Rule { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
    public string? Message { get; set; }
}