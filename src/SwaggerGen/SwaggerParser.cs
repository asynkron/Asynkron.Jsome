using Newtonsoft.Json;
using SwaggerGen.Models;

namespace SwaggerGen;

/// <summary>
/// Parser for Swagger 2.0 JSON documents
/// </summary>
public class SwaggerParser
{
    /// <summary>
    /// Parses a Swagger 2.0 JSON string into a SwaggerDocument object
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>A SwaggerDocument object</returns>
    /// <exception cref="ArgumentException">Thrown when the JSON is null, empty, or not valid Swagger 2.0</exception>
    /// <exception cref="JsonException">Thrown when the JSON is not valid</exception>
    public static SwaggerDocument Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON string cannot be null or empty", nameof(json));
        }

        try
        {
            var document = JsonConvert.DeserializeObject<SwaggerDocument>(json);
            
            if (document == null)
            {
                throw new JsonException("Failed to deserialize JSON to SwaggerDocument");
            }

            ValidateSwaggerDocument(document);
            
            return document;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error parsing Swagger JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses a Swagger 2.0 JSON file into a SwaggerDocument object
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>A SwaggerDocument object</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON is not valid Swagger 2.0</exception>
    /// <exception cref="JsonException">Thrown when the JSON is not valid</exception>
    public static async Task<SwaggerDocument> ParseFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return Parse(json);
        }
        catch (IOException ex)
        {
            throw new JsonException($"Error reading file {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that the document is a valid Swagger 2.0 document
    /// </summary>
    /// <param name="document">The document to validate</param>
    /// <exception cref="ArgumentException">Thrown when the document is not valid Swagger 2.0</exception>
    private static void ValidateSwaggerDocument(SwaggerDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.Swagger))
        {
            throw new ArgumentException("Document must have a 'swagger' field");
        }

        if (!document.Swagger.StartsWith("2."))
        {
            throw new ArgumentException($"Only Swagger 2.0 is supported. Found version: {document.Swagger}");
        }

        if (document.Info == null)
        {
            throw new ArgumentException("Document must have an 'info' field");
        }

        if (string.IsNullOrWhiteSpace(document.Info.Title))
        {
            throw new ArgumentException("Document info must have a 'title' field");
        }

        if (string.IsNullOrWhiteSpace(document.Info.Version))
        {
            throw new ArgumentException("Document info must have a 'version' field");
        }
    }

    /// <summary>
    /// Gets a summary of the parsed Swagger document
    /// </summary>
    /// <param name="document">The Swagger document</param>
    /// <returns>A formatted summary string</returns>
    public static string GetDocumentSummary(SwaggerDocument document)
    {
        if (document == null)
        {
            return "No document provided";
        }

        var summary = $"Swagger Document Summary:\n";
        summary += $"  Title: {document.Info?.Title ?? "N/A"}\n";
        summary += $"  Version: {document.Info?.Version ?? "N/A"}\n";
        summary += $"  Description: {document.Info?.Description ?? "N/A"}\n";
        summary += $"  Swagger Version: {document.Swagger}\n";
        summary += $"  Host: {document.Host}\n";
        summary += $"  Base Path: {document.BasePath}\n";
        summary += $"  Schemes: [{string.Join(", ", document.Schemes)}]\n";
        summary += $"  Paths: {document.Paths.Count}\n";
        summary += $"  Definitions: {document.Definitions.Count}\n";

        if (document.Paths.Any())
        {
            summary += "\n  Available Paths:\n";
            foreach (var path in document.Paths.Keys.Take(10)) // Show first 10 paths
            {
                summary += $"    {path}\n";
            }
            if (document.Paths.Count > 10)
            {
                summary += $"    ... and {document.Paths.Count - 10} more\n";
            }
        }

        if (document.Definitions.Any())
        {
            summary += "\n  Available Definitions:\n";
            foreach (var definition in document.Definitions.Keys.Take(10)) // Show first 10 definitions
            {
                summary += $"    {definition}\n";
            }
            if (document.Definitions.Count > 10)
            {
                summary += $"    ... and {document.Definitions.Count - 10} more\n";
            }
        }

        return summary;
    }
}