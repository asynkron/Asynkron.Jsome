using Asynkron.Jsome.Models;
using Spectre.Console;

namespace Asynkron.Jsome.Configuration;

/// <summary>
/// Validates property paths in configuration against Swagger schema definitions
/// </summary>
public static class SchemaValidator
{
    /// <summary>
    /// Validates that all property paths in the configuration exist in the Swagger schema
    /// </summary>
    /// <param name="config">The modifier configuration to validate</param>
    /// <param name="document">The Swagger document to validate against</param>
    /// <returns>List of validation errors, empty if all paths are valid</returns>
    public static List<ValidationError> ValidatePropertyPaths(ModifierConfiguration config, SwaggerDocument document)
    {
        var errors = new List<ValidationError>();
        
        foreach (var rule in config.Rules)
        {
            var propertyPath = rule.Key;
            
            // Skip wildcard patterns for now (like "*.Id")
            if (propertyPath.Contains("*"))
                continue;
                
            if (!IsValidPropertyPath(propertyPath, document))
            {
                errors.Add(new ValidationError
                {
                    PropertyPath = propertyPath,
                    Message = $"Property path '{propertyPath}' was not found in the Swagger definition."
                });
            }
        }
        
        return errors;
    }
    
    /// <summary>
    /// Checks if a property path exists in the Swagger schema
    /// </summary>
    /// <param name="propertyPath">The property path to validate (e.g., "Order.Details.Product")</param>
    /// <param name="document">The Swagger document</param>
    /// <returns>True if the path exists, false otherwise</returns>
    private static bool IsValidPropertyPath(string propertyPath, SwaggerDocument document)
    {
        var pathParts = propertyPath.Split('.');
        
        if (pathParts.Length == 0)
            return false;
            
        // First part should be a definition name
        var rootDefinition = pathParts[0];
        if (!document.Definitions.ContainsKey(rootDefinition))
            return false;
            
        // If it's just the root definition, it's valid
        if (pathParts.Length == 1)
            return true;
            
        // Navigate through the schema following the property path
        var currentSchema = document.Definitions[rootDefinition];
        
        for (int i = 1; i < pathParts.Length; i++)
        {
            var propertyName = pathParts[i];
            
            // Check if current schema has properties
            if (currentSchema.Properties == null || !currentSchema.Properties.ContainsKey(propertyName))
                return false;
                
            currentSchema = currentSchema.Properties[propertyName];
            
            // If this property references another definition, follow the reference
            if (!string.IsNullOrEmpty(currentSchema.Ref))
            {
                var refName = ExtractRefName(currentSchema.Ref);
                if (!document.Definitions.ContainsKey(refName))
                    return false;
                    
                currentSchema = document.Definitions[refName];
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Extracts the definition name from a $ref string
    /// </summary>
    /// <param name="refString">The $ref string (e.g., "#/definitions/Pet")</param>
    /// <returns>The definition name (e.g., "Pet")</returns>
    private static string ExtractRefName(string refString)
    {
        const string definitionsPrefix = "#/definitions/";
        if (refString.StartsWith(definitionsPrefix))
        {
            return refString.Substring(definitionsPrefix.Length);
        }
        return refString;
    }
    
    /// <summary>
    /// Displays validation errors using Spectre.Console
    /// </summary>
    /// <param name="errors">The validation errors to display</param>
    public static void DisplayValidationErrors(List<ValidationError> errors)
    {
        if (errors.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✓ All configuration property paths are valid[/]");
            return;
        }
        
        AnsiConsole.MarkupLine("[red]✗ Configuration validation errors found:[/]");
        AnsiConsole.WriteLine();
        
        foreach (var error in errors)
        {
            AnsiConsole.MarkupLine($"[red]  • {error.Message}[/]");
        }
        
        AnsiConsole.WriteLine();
    }
}

/// <summary>
/// Represents a validation error for a property path
/// </summary>
public class ValidationError
{
    public string PropertyPath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}