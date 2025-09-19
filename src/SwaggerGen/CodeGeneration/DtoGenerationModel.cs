namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Model representing a DTO class for code generation
/// </summary>
public class DtoGenerationModel
{
    /// <summary>
    /// The namespace for the generated class
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The class name
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The class description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of properties for the class
    /// </summary>
    public List<PropertyModel> Properties { get; set; } = new();
}

/// <summary>
/// Model representing a property in a DTO class
/// </summary>
public class PropertyModel
{
    /// <summary>
    /// The property name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The C# type of the property
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the property is nullable
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// The property description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Validation attributes for the property
    /// </summary>
    public List<string> ValidationAttributes { get; set; } = new();

    /// <summary>
    /// Whether the property has a default value
    /// </summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>
    /// The default value as a string
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
}

/// <summary>
/// Model representing a FluentValidation validator for code generation
/// </summary>
public class ValidatorGenerationModel
{
    /// <summary>
    /// The namespace for the generated validator
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The class name being validated
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// List of validation rules
    /// </summary>
    public List<string> ValidationRules { get; set; } = new();
}