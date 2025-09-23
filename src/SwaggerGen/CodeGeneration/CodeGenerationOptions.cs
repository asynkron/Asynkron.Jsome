using SwaggerGen.Configuration;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Configuration options for code generation behavior
/// </summary>
public class CodeGenerationOptions
{
    /// <summary>
    /// When true, generates C# enums for integer enum properties and static classes with constants for string enum properties.
    /// When false, uses the legacy behavior of generating string/int properties with validation constraints.
    /// Default: false (for backward compatibility)
    /// </summary>
    public bool GenerateEnumTypes { get; set; } = false;

    /// <summary>
    /// Path to a modifier configuration file (YAML or JSON) that controls object graph generation.
    /// If null or empty, no modifier configuration is applied.
    /// </summary>
    public string? ModifierConfigurationPath { get; set; }

    /// <summary>
    /// Modifier configuration instance. If both this and ModifierConfigurationPath are provided,
    /// this instance takes precedence.
    /// </summary>
    public ModifierConfiguration? ModifierConfiguration { get; set; }

    /// <summary>
    /// Custom directory path for Handlebars templates. If specified, templates will be loaded from this directory.
    /// If null or empty, the default template discovery logic is used.
    /// </summary>
    public string? TemplateDirectory { get; set; }

    /// <summary>
    /// When true, generates nullable reference types (e.g., string?) for non-required properties.
    /// When false, uses the legacy behavior of non-nullable types.
    /// Default: false (for backward compatibility)
    /// </summary>
    public bool UseNullableReferenceTypes { get; set; } = false;

    /// <summary>
    /// When true, uses the C# 'required' keyword instead of [Required] attributes for required properties.
    /// Requires C# 11+ and nullable reference types to be enabled.
    /// Default: false (for backward compatibility)
    /// </summary>
    public bool UseRequiredKeyword { get; set; } = false;

    /// <summary>
    /// When true, generates C# records instead of classes for DTOs.
    /// Default: false (for backward compatibility)
    /// </summary>
    public bool GenerateRecords { get; set; } = false;

    /// <summary>
    /// When true, uses System.Text.Json attributes (JsonPropertyName, JsonIgnore) instead of Newtonsoft.Json.
    /// Also generates enhanced validation attributes like Required(AllowEmptyStrings = false) and StringLength.
    /// Default: false (for backward compatibility with Newtonsoft.Json)
    /// </summary>
    public bool UseSystemTextJson { get; set; } = false;

    /// <summary>
    /// When true, generates Swashbuckle.AspNetCore.Annotations attributes (SwaggerSchema, SwaggerExampleValue)
    /// based on OpenAPI metadata in addition to existing DataAnnotations.
    /// Default: false (for backward compatibility)
    /// </summary>
    public bool UseSwashbuckleAttributes { get; set; } = false;

    /// <summary>
    /// List of specific template files to use for code generation. When specified, only these templates will be processed.
    /// Template files should be specified as file names (e.g., "DTO.hbs", "MyCustomTemplate.hbs").
    /// If null or empty, the default behavior applies (load all standard templates).
    /// </summary>
    public List<string>? CustomTemplateFiles { get; set; }
}