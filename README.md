# SwaggerGen

A powerful .NET tool for parsing Swagger 2.0 specifications and generating C# DTOs with FluentValidation validators using Handlebars templating.

## Features

- **Swagger 2.0 Parsing**: Complete parsing of Swagger/OpenAPI 2.0 JSON specifications
- **DTO Code Generation**: Automatically generate C# Data Transfer Objects (DTOs) from Swagger definitions
- **FluentValidation Support**: Generate comprehensive validators with validation rules based on Swagger schema constraints
- **Handlebars Templating**: Flexible code generation using Handlebars.Net templating engine
- **Multi-Platform**: Runs on Windows, macOS, and Linux with .NET 8.0
- **Comprehensive Testing**: Full test coverage with automated CI/CD pipeline

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Any IDE that supports .NET (Visual Studio, VS Code, JetBrains Rider)

### Building the Solution

```bash
git clone https://github.com/asynkron/SwaggerGen.git
cd SwaggerGen
dotnet build
```

### Running the Application

```bash
cd src/SwaggerGen
dotnet run
```

The application will:
1. Parse the sample Swagger file (`Samples/petstore-swagger.json`)
2. Display a summary of the parsed document
3. Generate DTOs and validators in the `Generated/` directory

### Running Tests

```bash
dotnet test
```

## Code Generation

### Generated DTOs

DTOs are generated with the following features:

- **JsonProperty Attributes**: Proper JSON serialization mapping
- **Data Annotations**: Required field validation
- **Nullable Properties**: Appropriate nullability based on Swagger requirements
- **XML Documentation**: Comments from Swagger descriptions
- **Type Mapping**: Swagger types mapped to appropriate C# types

Example generated DTO:
```csharp
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SwaggerGen.Generated.DTOs;

/// <summary>
/// A pet in the store
/// </summary>
public class Pet
{
    /// <summary>
    /// The pet identifier
    /// </summary>
    [Required]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// The pet name
    /// </summary>
    [Required]
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Pet status in the store
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; } = null;
}
```

### Generated Validators

FluentValidation validators are created with comprehensive rules:

- **Required Field Validation**: NotNull() and NotEmpty() for required properties
- **String Constraints**: MinimumLength, MaximumLength, and regex pattern validation
- **Numeric Constraints**: Range validation with inclusive/exclusive bounds
- **Array Validation**: Item count validation with custom error messages

Example generated validator:
```csharp
using FluentValidation;

namespace SwaggerGen.Generated.Validators;

/// <summary>
/// Validator for Pet - A pet in the store
/// </summary>
public class PetValidator : AbstractValidator<Pet>
{
    public PetValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100);
    }
}
```

## Templating Strategy

The code generation system uses Handlebars.Net templates located in `src/SwaggerGen/CodeGeneration/Templates/`:

### Template Structure

- **dto.hbs**: Template for generating C# DTO classes
- **validator.hbs**: Template for generating FluentValidation validators

### Template Models

Templates receive strongly-typed models:

- **DtoModel**: Contains class name, namespace, properties, and metadata
- **ValidatorModel**: Contains validation rules and associated DTO information
- **PropertyModel**: Detailed property information including type, nullability, and constraints

### Customization

Templates can be easily modified to change the generated code structure, add additional attributes, or implement different validation patterns.

## Architecture Integration

### Solution Structure

```
SwaggerGen/
├── .github/workflows/          # CI/CD pipeline
├── src/SwaggerGen/
│   ├── CodeGeneration/         # Code generation engine
│   │   ├── Models/            # Template models
│   │   ├── Templates/         # Handlebars templates
│   │   ├── ModelMapper.cs     # Schema to model mapping
│   │   ├── SwaggerCodeGenerator.cs  # Main generator
│   │   └── TemplateEngine.cs  # Handlebars integration
│   ├── Models/                # Swagger document models
│   ├── Samples/               # Sample Swagger files
│   ├── Program.cs             # Console application
│   └── SwaggerParser.cs       # Swagger parsing logic
├── tests/SwaggerGen.Tests/    # Unit tests
└── SwaggerGen.sln             # Solution file
```

### Integration Points

1. **SwaggerParser**: Parses Swagger JSON into strongly-typed models
2. **ModelMapper**: Converts Swagger schemas to code generation models
3. **TemplateEngine**: Processes Handlebars templates with model data
4. **SwaggerCodeGenerator**: Orchestrates the entire generation process

### Extension Points

- **Custom Templates**: Add new Handlebars templates for different output formats
- **Type Mapping**: Extend `ModelMapper` for custom type conversions
- **Validation Rules**: Add new validation logic in `BuildValidationRules`
- **Output Formats**: Support additional templating engines beyond Handlebars

## CI/CD Pipeline

The GitHub Actions workflow (`/.github/workflows/ci.yml`) provides:

- **Multi-Platform Testing**: Ubuntu, Windows, and macOS
- **NuGet Package Caching**: Improved build performance
- **Code Coverage**: Codecov integration
- **Automated Testing**: Runs on all pushes and pull requests to main and feature branches

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and add tests
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## Dependencies

- **Newtonsoft.Json**: JSON serialization for Swagger parsing
- **Handlebars.Net**: Template engine for code generation
- **FluentValidation**: Validation framework for generated validators
- **xUnit**: Testing framework with comprehensive test coverage

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Roadmap

- [ ] OpenAPI 3.0 support
- [ ] Additional template formats (TypeScript, Python, etc.)
- [ ] CLI parameter support for custom configuration
- [ ] Visual Studio extension integration
- [ ] Docker containerization