namespace SwaggerGen.CodeGeneration.Models;

/// <summary>
/// Model for generating C# DTOs from Swagger definitions
/// </summary>
public class DtoModel
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PropertyModel> Properties { get; set; } = new();
    public List<string> RequiredProperties { get; set; } = new();
    public bool HasRequiredProperties => RequiredProperties.Any();
}

/// <summary>
/// Model for generating FluentValidation validators
/// </summary>
public class ValidatorModel
{
    public string ClassName { get; set; } = string.Empty;
    public string DtoClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ValidationRuleModel> ValidationRules { get; set; } = new();
    public bool HasValidationRules => ValidationRules.Any();
}

/// <summary>
/// Model for DTO properties
/// </summary>
public class PropertyModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public object? DefaultValue { get; set; }
    public string JsonPropertyName { get; set; } = string.Empty;
}

/// <summary>
/// Model for validation rules
/// </summary>
public class ValidationRuleModel
{
    public string PropertyName { get; set; } = string.Empty;
    public List<string> Rules { get; set; } = new();
    public bool HasRules => Rules.Any();
}