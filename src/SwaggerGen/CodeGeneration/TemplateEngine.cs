using HandlebarsDotNet;
using System.Reflection;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Service for rendering Handlebars templates
/// </summary>
public class TemplateEngine
{
    private readonly IHandlebars _handlebars;

    public TemplateEngine()
    {
        _handlebars = Handlebars.Create();
    }

    /// <summary>
    /// Renders a DTO class using the specified model
    /// </summary>
    /// <param name="model">The model containing data for the template</param>
    /// <returns>The rendered C# class code</returns>
    public string RenderDtoClass(DtoGenerationModel model)
    {
        var template = GetEmbeddedTemplate("dto-class.hbs");
        var compiledTemplate = _handlebars.Compile(template);
        return compiledTemplate(model);
    }

    /// <summary>
    /// Renders a FluentValidation validator using the specified model
    /// </summary>
    /// <param name="model">The model containing data for the template</param>
    /// <returns>The rendered C# validator code</returns>
    public string RenderValidator(ValidatorGenerationModel model)
    {
        var template = GetEmbeddedTemplate("validator-class.hbs");
        var compiledTemplate = _handlebars.Compile(template);
        return compiledTemplate(model);
    }

    /// <summary>
    /// Gets a template from the Templates directory
    /// </summary>
    /// <param name="templateName">The name of the template file</param>
    /// <returns>The template content</returns>
    private string GetEmbeddedTemplate(string templateName)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        // Try different possible paths for the template
        var possiblePaths = new[]
        {
            Path.Combine(assemblyDirectory!, "Templates", "DTO", templateName),
            Path.Combine(assemblyDirectory!, "Templates", "Validators", templateName),
            Path.Combine(Directory.GetCurrentDirectory(), "Templates", "DTO", templateName),
            Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Validators", templateName),
            Path.Combine(FindSourceDirectory() ?? "", "Templates", "DTO", templateName),
            Path.Combine(FindSourceDirectory() ?? "", "Templates", "Validators", templateName)
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
        }

        throw new FileNotFoundException($"Template '{templateName}' not found in any of the expected locations.");
    }

    /// <summary>
    /// Attempts to find the source directory by looking for the Templates folder
    /// </summary>
    /// <returns>The source directory path if found, null otherwise</returns>
    private static string? FindSourceDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);
        
        // Walk up the directory tree looking for the source structure
        while (directory != null)
        {
            var srcDir = Path.Combine(directory.FullName, "src", "SwaggerGen");
            var templatesDir = Path.Combine(srcDir, "Templates");
            
            if (Directory.Exists(templatesDir))
            {
                return srcDir;
            }
            
            directory = directory.Parent;
        }
        
        return null;
    }
}