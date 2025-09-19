namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents a property in a generated DTO class
/// </summary>
public class DTOProperty
{
    public string Name { get; set; } = string.Empty;
    public string JsonName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public string? Pattern { get; set; }
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsNestedObject { get; set; }
    public bool IsArray { get; set; }
    public string? ArrayItemValidator { get; set; }
}