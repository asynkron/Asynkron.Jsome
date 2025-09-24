using Newtonsoft.Json;
using Asynkron.Jsome.Models;

namespace Asynkron.Jsome;

/// <summary>
/// Parser for JSON Schema files that can merge multiple schemas into a unified SwaggerDocument
/// </summary>
public class JsonSchemaParser
{
    /// <summary>
    /// Loads and parses all JSON Schema files in a directory into a unified SwaggerDocument
    /// </summary>
    /// <param name="directoryPath">Path to directory containing .json schema files</param>
    /// <returns>A SwaggerDocument with merged definitions</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when directory doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when schema definitions conflict</exception>
    public static SwaggerDocument ParseDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Schema directory not found: {directoryPath}");
        }

        var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
        
        if (jsonFiles.Length == 0)
        {
            throw new InvalidOperationException($"No .json files found in directory: {directoryPath}");
        }

        var mergedDocument = new SwaggerDocument
        {
            Swagger = "2.0",
            Info = new Info
            {
                Title = "Generated from JSON Schema Directory",
                Version = "1.0.0"
            },
            Definitions = new Dictionary<string, Schema>()
        };

        var processedSchemas = new Dictionary<string, (Schema Schema, string SourceFile)>();

        // First pass: Load all schemas and extract both root schemas and internal definitions
        foreach (var filePath in jsonFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var jsonContent = File.ReadAllText(filePath);
                
                // First, parse as a generic JSON object to check for definitions
                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
                if (jsonObject == null)
                {
                    throw new JsonException($"Failed to parse JSON from file: {filePath}");
                }

                var schema = JsonConvert.DeserializeObject<Schema>(jsonContent, new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                });

                if (schema == null)
                {
                    throw new JsonException($"Failed to deserialize schema from file: {filePath}");
                }

                // Use the title if available, otherwise use filename
                var schemaName = !string.IsNullOrEmpty(schema.Title) ? schema.Title : fileName;
                
                // Check for duplicates and handle conflicts
                if (processedSchemas.ContainsKey(schemaName))
                {
                    var existing = processedSchemas[schemaName];
                    if (!AreSchemasSemanticallyEqual(schema, existing.Schema))
                    {
                        throw new InvalidOperationException(
                            $"Conflicting schema definitions found for '{schemaName}'. " +
                            $"Source files: '{existing.SourceFile}' and '{filePath}'. " +
                            "Schema definitions with the same name must be identical.");
                    }
                    // Skip duplicate - they're identical
                    continue;
                }

                processedSchemas[schemaName] = (schema, filePath);

                // Check if this schema has internal definitions and extract them
                if (jsonObject.ContainsKey("definitions") && jsonObject["definitions"] is Newtonsoft.Json.Linq.JObject definitionsObject)
                {
                    foreach (var defPair in definitionsObject)
                    {
                        var defSchema = JsonConvert.DeserializeObject<Schema>(defPair.Value.ToString(), new JsonSerializerSettings
                        {
                            MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                        });

                        if (defSchema != null)
                        {
                            var defName = defPair.Key;
                            
                            // Check for conflicts with this definition
                            if (processedSchemas.ContainsKey(defName))
                            {
                                var existing = processedSchemas[defName];
                                if (!AreSchemasSemanticallyEqual(defSchema, existing.Schema))
                                {
                                    throw new InvalidOperationException(
                                        $"Conflicting schema definitions found for '{defName}'. " +
                                        $"Source files: '{existing.SourceFile}' and '{filePath}' (internal definition). " +
                                        "Schema definitions with the same name must be identical.");
                                }
                                // Skip duplicate - they're identical
                                continue;
                            }

                            processedSchemas[defName] = (defSchema, filePath);
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new JsonException($"Error parsing JSON Schema file '{filePath}': {ex.Message}", ex);
            }
        }

        // Second pass: Add all schemas to the definitions
        foreach (var kvp in processedSchemas)
        {
            mergedDocument.Definitions[kvp.Key] = kvp.Value.Schema;
        }

        // Third pass: Resolve $ref references and validate cross-references
        ResolveReferences(mergedDocument);

        return mergedDocument;
    }

    /// <summary>
    /// Resolves $ref references within the merged document
    /// </summary>
    private static void ResolveReferences(SwaggerDocument document)
    {
        foreach (var definition in document.Definitions.Values)
        {
            ResolveSchemaReferences(definition, document.Definitions);
        }
    }

    /// <summary>
    /// Recursively resolves $ref references in a schema
    /// </summary>
    private static void ResolveSchemaReferences(Schema schema, Dictionary<string, Schema> allDefinitions)
    {
        if (schema == null) return;

        // Handle properties
        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties.Values)
            {
                ResolveSchemaReferences(property, allDefinitions);
            }
        }

        // Handle array items
        if (schema.Items != null)
        {
            ResolveSchemaReferences(schema.Items, allDefinitions);
        }

        // Handle allOf
        if (schema.AllOf != null)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                ResolveSchemaReferences(allOfSchema, allDefinitions);
            }
        }

        // Handle additionalProperties
        if (schema.AdditionalProperties != null)
        {
            ResolveSchemaReferences(schema.AdditionalProperties, allDefinitions);
        }

        // Validate $ref references
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var refName = schema.Ref.Replace("#/definitions/", "");
            if (!allDefinitions.ContainsKey(refName))
            {
                throw new InvalidOperationException(
                    $"Invalid reference '{schema.Ref}' - referenced schema '{refName}' not found in definitions.");
            }
        }
    }

    /// <summary>
    /// Compares two schemas for semantic equality (ignoring description differences)
    /// </summary>
    private static bool AreSchemasSemanticallyEqual(Schema schema1, Schema schema2)
    {
        if (schema1 == null && schema2 == null) return true;
        if (schema1 == null || schema2 == null) return false;

        // Compare essential properties that affect code generation
        return schema1.Type == schema2.Type &&
               schema1.Format == schema2.Format &&
               schema1.Ref == schema2.Ref &&
               schema1.MaxLength == schema2.MaxLength &&
               schema1.MinLength == schema2.MinLength &&
               schema1.Maximum == schema2.Maximum &&
               schema1.Minimum == schema2.Minimum &&
               schema1.MaxItems == schema2.MaxItems &&
               schema1.MinItems == schema2.MinItems &&
               schema1.UniqueItems == schema2.UniqueItems &&
               schema1.Pattern == schema2.Pattern &&
               AreListsEqual(schema1.Required, schema2.Required) &&
               AreListsEqual(schema1.Enum, schema2.Enum) &&
               ArePropertiesEqual(schema1.Properties, schema2.Properties) &&
               AreSchemasEqual(schema1.Items, schema2.Items) &&
               AreAllOfEqual(schema1.AllOf, schema2.AllOf);
    }

    private static bool AreListsEqual<T>(List<T> list1, List<T> list2)
    {
        if (list1.Count != list2.Count) return false;
        return list1.OrderBy(x => x?.ToString()).SequenceEqual(list2.OrderBy(x => x?.ToString()));
    }

    private static bool ArePropertiesEqual(Dictionary<string, Schema> props1, Dictionary<string, Schema> props2)
    {
        if (props1.Count != props2.Count) return false;
        
        foreach (var kvp in props1)
        {
            if (!props2.ContainsKey(kvp.Key) || !AreSchemasSemanticallyEqual(kvp.Value, props2[kvp.Key]))
                return false;
        }
        
        return true;
    }

    private static bool AreSchemasEqual(Schema? schema1, Schema? schema2)
    {
        return AreSchemasSemanticallyEqual(schema1, schema2);
    }

    private static bool AreAllOfEqual(List<Schema> allOf1, List<Schema> allOf2)
    {
        if (allOf1.Count != allOf2.Count) return false;
        
        for (int i = 0; i < allOf1.Count; i++)
        {
            if (!AreSchemasSemanticallyEqual(allOf1[i], allOf2[i]))
                return false;
        }
        
        return true;
    }
}