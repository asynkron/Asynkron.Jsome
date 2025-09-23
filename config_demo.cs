using SwaggerGen;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Configuration;
using SwaggerGen.Models;

Console.WriteLine("SwaggerGen Modifier Configuration Demo");
Console.WriteLine("======================================");

// Create a simple Swagger document
var document = new SwaggerDocument
{
    Definitions = new Dictionary<string, Schema>
    {
        ["User"] = new Schema
        {
            Type = "object",
            Properties = new Dictionary<string, Schema>
            {
                ["id"] = new Schema { Type = "string" },
                ["name"] = new Schema { Type = "string", MaxLength = 50 },
                ["password"] = new Schema { Type = "string" },
                ["email"] = new Schema { Type = "string" }
            },
            Required = new List<string> { "id", "name" }
        }
    }
};

Console.WriteLine("\n1. Generating without configuration (original behavior):");
var originalGenerator = new CodeGenerator();
var originalResult = originalGenerator.GenerateCode(document, "Original.Generated");
Console.WriteLine("Generated User class with all properties including password");

Console.WriteLine("\n2. Generating with modifier configuration:");

// Create configuration that excludes password and customizes name validation
var config = new ModifierConfiguration
{
    Global = new GlobalSettings { Namespace = "Secure.Generated" }
};
config.Rules["User.password"] = new PropertyRule { Include = false };
config.Rules["User.name"] = new PropertyRule
{
    Include = true,
    Description = "User's display name with enhanced validation",
    Validation = new PropertyValidation
    {
        Required = true,
        MaxLength = 100,
        Pattern = "^[A-Za-z\\s]+$",
        Message = "Name must contain only letters and spaces"
    }
};

var options = new CodeGenerationOptions { ModifierConfiguration = config };
var generator = new CodeGenerator(options);
var result = generator.GenerateCode(document);

Console.WriteLine("\nGenerated DTO (password excluded, custom namespace):");
Console.WriteLine("----");
Console.WriteLine(result.DtoClasses["User"]);

Console.WriteLine("\nGenerated Validator (custom validation for name):");
Console.WriteLine("----");
Console.WriteLine(result.Validators["User"]);

Console.WriteLine("\n3. Testing configuration file loading:");
try
{
    var configPath = FindSampleConfigPath();
    if (File.Exists(configPath))
    {
        var yamlConfig = ConfigurationLoader.Load(configPath);
        Console.WriteLine($"✓ Loaded YAML config with {yamlConfig.Rules.Count} rules");
        Console.WriteLine($"  Global namespace: {yamlConfig.Global?.Namespace ?? "not set"}");
        Console.WriteLine($"  Generate enum types: {yamlConfig.Global?.GenerateEnumTypes ?? false}");
        
        // Show some example rules
        foreach (var rule in yamlConfig.Rules.Take(3))
        {
            Console.WriteLine($"  Rule '{rule.Key}': include={rule.Value.Include}");
        }
    }
    else
    {
        Console.WriteLine($"Sample config file not found at: {configPath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading configuration: {ex.Message}");
}

static string FindSampleConfigPath()
{
    // Try to find the source directory by walking up the directory tree
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    
    while (directory != null)
    {
        var srcDir = Path.Combine(directory.FullName, "src", "SwaggerGen");
        var configPath = Path.Combine(srcDir, "Samples", "Configuration", "sample-config.yaml");
        
        if (File.Exists(configPath))
        {
            return configPath;
        }
        
        directory = directory.Parent;
    }
    
    // Fallback to relative path
    return "src/SwaggerGen/Samples/Configuration/sample-config.yaml";
}

Console.WriteLine("\n4. Converting configuration to different formats:");
var simpleConfig = new ModifierConfiguration();
simpleConfig.Rules["Test.Property"] = new PropertyRule { Include = false };

Console.WriteLine("\nYAML format:");
Console.WriteLine(ConfigurationLoader.ToYaml(simpleConfig));

Console.WriteLine("JSON format:");
Console.WriteLine(ConfigurationLoader.ToJson(simpleConfig));

Console.WriteLine("\nDemo completed! The modifier configuration system provides:");
Console.WriteLine("- Property-level inclusion/exclusion control");
Console.WriteLine("- Custom validation rule overrides");
Console.WriteLine("- Type mapping and description customization");
Console.WriteLine("- Both YAML and JSON configuration support");
Console.WriteLine("- Full backward compatibility");