namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents an enum type to be generated
/// </summary>
public class EnumInfo
{
    public string EnumName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<EnumValueInfo> Values { get; set; } = [];
}

/// <summary>
/// Represents a value in an enum
/// </summary>
public class EnumValueInfo
{
    public string Name { get; set; } = string.Empty;
    public object Value { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Represents a static constants class to be generated
/// </summary>
public class ConstantsInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ConstantInfo> Constants { get; set; } = [];
}

/// <summary>
/// Represents a constant value in a constants class
/// </summary>
public class ConstantInfo
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}