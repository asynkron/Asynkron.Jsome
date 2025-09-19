using HandlebarsDotNet;
using SwaggerGen.CodeGeneration.Models;
using System.Reflection;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Handlebars-based template engine for code generation
/// </summary>
public class TemplateEngine
{
    private readonly IHandlebars _handlebars;
    private readonly Dictionary<string, HandlebarsTemplate<object, object>> _compiledTemplates;

    public TemplateEngine()
    {
        _handlebars = Handlebars.Create();
        _compiledTemplates = new Dictionary<string, HandlebarsTemplate<object, object>>();
        InitializeTemplates();
    }

    /// <summary>
    /// Generate C# DTO code from a model
    /// </summary>
    /// <param name="model">The DTO model</param>
    /// <returns>Generated C# code</returns>
    public string GenerateDto(DtoModel model)
    {
        if (!_compiledTemplates.TryGetValue("dto", out var template))
        {
            throw new InvalidOperationException("DTO template not found");
        }

        return template(model);
    }

    /// <summary>
    /// Generate FluentValidation validator code from a model
    /// </summary>
    /// <param name="model">The validator model</param>
    /// <returns>Generated C# validator code</returns>
    public string GenerateValidator(ValidatorModel model)
    {
        if (!_compiledTemplates.TryGetValue("validator", out var template))
        {
            throw new InvalidOperationException("Validator template not found");
        }

        return template(model);
    }

    private void InitializeTemplates()
    {
        LoadTemplate("dto");
        LoadTemplate("validator");
    }

    private void LoadTemplate(string templateName)
    {
        var templateContent = LoadEmbeddedTemplate($"{templateName}.hbs");
        var template = _handlebars.Compile(templateContent);
        _compiledTemplates[templateName] = template;
    }

    private string LoadEmbeddedTemplate(string templateFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templatePath = Path.Combine("CodeGeneration", "Templates", templateFileName);
        var fullPath = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", templatePath);

        if (File.Exists(fullPath))
        {
            return File.ReadAllText(fullPath);
        }

        // Fallback: try to load from current directory structure
        var currentDir = Directory.GetCurrentDirectory();
        var fallbackPath = Path.Combine(currentDir, "src", "SwaggerGen", templatePath);
        
        if (File.Exists(fallbackPath))
        {
            return File.ReadAllText(fallbackPath);
        }

        // Final fallback: look for template files relative to source
        var sourceDir = FindSourceDirectory(currentDir);
        if (sourceDir != null)
        {
            var sourcePath = Path.Combine(sourceDir, templatePath);
            if (File.Exists(sourcePath))
            {
                return File.ReadAllText(sourcePath);
            }
        }

        throw new FileNotFoundException($"Template file not found: {templateFileName}");
    }

    private static string? FindSourceDirectory(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);
        
        while (directory != null)
        {
            var srcDir = Path.Combine(directory.FullName, "src", "SwaggerGen");
            if (Directory.Exists(srcDir))
            {
                return srcDir;
            }
            
            directory = directory.Parent;
        }
        
        return null;
    }
}