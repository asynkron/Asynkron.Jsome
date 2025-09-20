namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Contains the results of code generation
/// </summary>
public class CodeGenerationResult
{
    /// <summary>
    /// Generated DTO classes keyed by schema name
    /// </summary>
    public Dictionary<string, string> DtoClasses { get; set; } = new();

    /// <summary>
    /// Generated validator classes keyed by schema name
    /// </summary>
    public Dictionary<string, string> Validators { get; set; } = new();

    /// <summary>
    /// Generated enum types keyed by enum name
    /// </summary>
    public Dictionary<string, string> EnumTypes { get; set; } = new();

    /// <summary>
    /// Generated static constant classes keyed by constants class name
    /// </summary>
    public Dictionary<string, string> ConstantClasses { get; set; } = new();
}