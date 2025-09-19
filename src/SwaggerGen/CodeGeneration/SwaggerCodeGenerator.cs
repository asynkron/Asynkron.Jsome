using SwaggerGen.Models;

namespace SwaggerGen.CodeGeneration;

/// <summary>
/// Main code generator for Swagger-based DTO and validator generation
/// </summary>
public class SwaggerCodeGenerator
{
    private readonly TemplateEngine _templateEngine;
    private readonly ModelMapper _modelMapper;

    public SwaggerCodeGenerator(string defaultNamespace = "Generated")
    {
        _templateEngine = new TemplateEngine();
        _modelMapper = new ModelMapper($"{defaultNamespace}.DTOs");
    }

    /// <summary>
    /// Generate DTOs and validators for all definitions in a Swagger document
    /// </summary>
    /// <param name="document">The Swagger document</param>
    /// <param name="outputDirectory">Directory to write generated files</param>
    /// <param name="generateValidators">Whether to generate FluentValidation validators</param>
    /// <returns>Generation results</returns>
    public async Task<CodeGenerationResult> GenerateAsync(
        SwaggerDocument document, 
        string outputDirectory, 
        bool generateValidators = true)
    {
        var result = new CodeGenerationResult();
        
        // Create output directories
        var dtosDir = Path.Combine(outputDirectory, "DTOs");
        var validatorsDir = Path.Combine(outputDirectory, "Validators");
        
        Directory.CreateDirectory(dtosDir);
        if (generateValidators)
        {
            Directory.CreateDirectory(validatorsDir);
        }

        // Generate code for each definition
        foreach (var definition in document.Definitions)
        {
            try
            {
                // Generate DTO
                var dtoModel = _modelMapper.MapToDto(definition.Key, definition.Value);
                var dtoCode = _templateEngine.GenerateDto(dtoModel);
                var dtoFilePath = Path.Combine(dtosDir, $"{dtoModel.ClassName}.cs");
                
                await File.WriteAllTextAsync(dtoFilePath, dtoCode);
                result.GeneratedFiles.Add(dtoFilePath);

                // Generate validator if requested
                if (generateValidators)
                {
                    var validatorModel = _modelMapper.MapToValidator(dtoModel, definition.Value);
                    if (validatorModel.HasValidationRules)
                    {
                        var validatorCode = _templateEngine.GenerateValidator(validatorModel);
                        var validatorFilePath = Path.Combine(validatorsDir, $"{validatorModel.ClassName}.cs");
                        
                        await File.WriteAllTextAsync(validatorFilePath, validatorCode);
                        result.GeneratedFiles.Add(validatorFilePath);
                    }
                }

                result.GeneratedDtos.Add(dtoModel.ClassName);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error generating code for {definition.Key}: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Generate a single DTO from a schema definition
    /// </summary>
    /// <param name="schemaName">Name of the schema</param>
    /// <param name="schema">The schema definition</param>
    /// <param name="namespaceName">Optional namespace override</param>
    /// <returns>Generated DTO code</returns>
    public string GenerateDto(string schemaName, Schema schema, string? namespaceName = null)
    {
        var dtoModel = _modelMapper.MapToDto(schemaName, schema, namespaceName);
        return _templateEngine.GenerateDto(dtoModel);
    }

    /// <summary>
    /// Generate a single validator from a schema definition
    /// </summary>
    /// <param name="schemaName">Name of the schema</param>
    /// <param name="schema">The schema definition</param>
    /// <param name="namespaceName">Optional namespace override</param>
    /// <returns>Generated validator code</returns>
    public string GenerateValidator(string schemaName, Schema schema, string? namespaceName = null)
    {
        var dtoModel = _modelMapper.MapToDto(schemaName, schema, namespaceName);
        var validatorModel = _modelMapper.MapToValidator(dtoModel, schema);
        return _templateEngine.GenerateValidator(validatorModel);
    }
}

/// <summary>
/// Results of code generation operation
/// </summary>
public class CodeGenerationResult
{
    public List<string> GeneratedFiles { get; set; } = new();
    public List<string> GeneratedDtos { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    
    public bool IsSuccess => !Errors.Any();
    public int TotalFilesGenerated => GeneratedFiles.Count;
}