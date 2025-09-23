namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Represents a generated file with content and metadata
/// </summary>
public class GeneratedFile
{
    /// <summary>
    /// The generated content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// File extension (without the dot, e.g., "cs", "fs")
    /// </summary>
    public string Extension { get; set; } = "cs";

    /// <summary>
    /// Creates a new GeneratedFile with content and extension
    /// </summary>
    public GeneratedFile(string content, string extension = "cs")
    {
        Content = content;
        Extension = extension;
    }
}