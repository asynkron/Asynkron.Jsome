namespace Asynkron.Jsome.CodeGeneration;

/// <summary>
/// Metadata extracted from template frontmatter
/// </summary>
public class TemplateMetadata
{
    /// <summary>
    /// File extension to use for generated files (default: "cs")
    /// </summary>
    public string Extension { get; set; } = "cs";

    /// <summary>
    /// Optional description of the template
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The template content without frontmatter
    /// </summary>
    public string Content { get; set; } = string.Empty;
}