# SwaggerGen

SwaggerGen is a C# code generator that processes Swagger 2.0 JSON files to produce custom C# DTO (Data Transfer Object) classes and FluentValidation validators. The project focuses on generating clean, type-safe C# code from OpenAPI/Swagger specifications using a Handlebars-based templating system.

## Features

- **Swagger 2.0 Parser**: Robust parsing of Swagger 2.0 JSON documents with validation
- **DTO Generation**: Automatic generation of C# DTO classes with proper type mapping
- **FluentValidation**: Automatic generation of FluentValidation validators based on Swagger constraints
- **Handlebars Templates**: Customizable code generation using Handlebars templates
- **Schema Composition**: Support for `allOf` composition (partial implementation)
- **Validation Attributes**: Automatic addition of DataAnnotations validation attributes
- **Type Safety**: Nullable reference types and proper C# type mapping

## Generated Output

For each Swagger definition, SwaggerGen produces two files:

### DTO Classes
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Generated.DTOs;

/// <summary>
/// DTO class for NewPet
/// </summary>
public class NewPet
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public string? Tag { get; set; }
}
```

### FluentValidation Validators
```csharp
using FluentValidation;

namespace Generated.DTOs.Validators;

/// <summary>
/// FluentValidation validator for NewPet
/// </summary>
public class NewPetValidator : AbstractValidator<NewPet>
{
    public NewPetValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

## Getting Started

### Prerequisites
- .NET 8.0 or later
- Swagger 2.0 JSON file

### Usage

1. **Build the project:**
```bash
dotnet build
```

2. **Run the console application:**
```bash
dotnet run --project src/SwaggerGen
```

3. **Run tests:**
```bash
dotnet test
```

The generated files will be saved to a `Generated` folder in the current directory.

## Architecture

### Core Components

- **SwaggerParser**: Parses and validates Swagger 2.0 JSON documents
- **SwaggerCodeGenerator**: Orchestrates the code generation process
- **TemplateEngine**: Renders Handlebars templates with model data
- **Models**: Swagger document object model and code generation models

### Templates

Templates are located in `src/SwaggerGen/Templates/`:
- `DTO/dto-class.hbs`: Template for generating DTO classes
- `Validators/validator-class.hbs`: Template for generating FluentValidation validators

## Type Mapping

| Swagger Type | C# Type |
|-------------|----------|
| `integer` | `int` |
| `integer` (int64) | `long` |
| `number` | `decimal` |
| `number` (float) | `float` |
| `string` | `string` |
| `string` (date-time) | `DateTime` |
| `string` (date) | `DateOnly` |
| `boolean` | `bool` |
| `array` | `List<T>` |
| Reference (`$ref`) | Referenced type name |

## Validation Support

### DataAnnotations
- `[Required]` for required properties
- `[StringLength]` for string length constraints
- `[Range]` for numeric range constraints

### FluentValidation Rules
- `NotEmpty()` for required properties
- `MinimumLength()` / `MaximumLength()` for string constraints
- `GreaterThanOrEqualTo()` / `LessThanOrEqualTo()` for numeric constraints
- `Matches()` for regex pattern validation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License.