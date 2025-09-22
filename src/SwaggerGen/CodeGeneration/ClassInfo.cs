namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents a class to be generated from Swagger schema
/// </summary>
public class ClassInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PropertyInfo> Properties { get; set; } = [];
    public bool UseSystemTextJson { get; set; }
}