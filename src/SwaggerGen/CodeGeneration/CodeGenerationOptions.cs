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
}