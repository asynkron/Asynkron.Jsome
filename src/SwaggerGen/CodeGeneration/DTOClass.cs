namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents a generated DTO class
/// </summary>
public class DTOClass
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<DTOProperty> Properties { get; set; } = new();
}