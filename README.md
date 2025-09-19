# SwaggerGen

SwaggerGen is a C# code generator that processes Swagger 2.0 JSON files to produce C# DTO (Data Transfer Object) classes and FluentValidation validators using Handlebars templates.

## Features

- **Handlebars Template Engine**: Customizable code generation using Handlebars.Net templates
- **DTO Generation**: Creates clean C# classes from Swagger schema definitions with proper inheritance support
- **FluentValidator Generation**: Generates FluentValidation validators with rules based on Swagger constraints
- **Inheritance Support**: Handles Swagger `allOf` schemas to generate proper C# inheritance hierarchies
- **Validation Attributes**: Adds Data Annotations validation attributes based on schema constraints
- **Command-Line Interface**: Easy-to-use CLI for generating code from Swagger files

## Installation & Usage

### Prerequisites

- .NET 8.0 or later

### Building the Project

```bash
# Clone the repository
git clone https://github.com/asynkron/SwaggerGen.git
cd SwaggerGen

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Using the Code Generator

```bash
# Generate code from a Swagger file
dotnet run --project src/SwaggerGen -- <swagger-file> [output-directory]

# Example: Generate from the included sample
dotnet run --project src/SwaggerGen -- src/SwaggerGen/Samples/petstore-swagger.json ./Generated
```

This will create:
- `./Generated/Models/` - Directory containing generated DTO classes
- `./Generated/Validators/` - Directory containing FluentValidation validator classes

### Example Generated Code

Given this Swagger definition:
```json
{
  "definitions": {
    "Pet": {
      "allOf": [
        { "$ref": "#/definitions/NewPet" },
        {
          "required": ["id"],
          "properties": {
            "id": { "type": "integer", "format": "int64" }
          }
        }
      ]
    },
    "NewPet": {
      "required": ["name"],
      "properties": {
        "name": { "type": "string" },
        "tag": { "type": "string" }
      }
    }
  }
}
```

SwaggerGen generates:

**Models/NewPet.cs:**
```csharp
using System.ComponentModel.DataAnnotations;

public class NewPet
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Tag { get; set; }
}
```

**Models/Pet.cs:**
```csharp
using System.ComponentModel.DataAnnotations;

public class Pet : NewPet
{
    [Required]
    public long Id { get; set; }
}
```

**Validators/PetValidator.cs:**
```csharp
using FluentValidation;

public class PetValidator : AbstractValidator<Pet>
{
    public PetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
```

## Template Customization

SwaggerGen uses Handlebars templates located in `src/SwaggerGen/Templates/`:

- **DTO.hbs** - Template for generating C# DTO classes
- **Validator.hbs** - Template for generating FluentValidation validators
- **README.md** - Documentation on template structure and customization

You can modify these templates to customize the generated code output. See the [Templates README](src/SwaggerGen/Templates/README.md) for detailed documentation.

## Supported Swagger Features

- ✅ Object schemas with properties
- ✅ Required property validation
- ✅ String length constraints (`minLength`, `maxLength`)
- ✅ Numeric constraints (`minimum`, `maximum`)
- ✅ Pattern validation (regular expressions)
- ✅ Type mapping (string, integer, number, boolean, array)
- ✅ Format mapping (int64 → long, date-time → DateTime, etc.)
- ✅ Inheritance via `allOf` with `$ref`
- ✅ Inline schemas within `allOf`

## Architecture

- **SwaggerParser** - Parses Swagger 2.0 JSON documents
- **CodeGenerator** - Generates code using Handlebars templates
- **Models** - Contains Swagger document model classes
- **Templates** - Handlebars templates for code generation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License.