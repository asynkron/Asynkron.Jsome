# SwaggerGen

A C# code generator that processes Swagger 2.0 JSON files to produce C# DTO (Data Transfer Object) classes and FluentValidation validators. The project focuses on generating clean, type-safe C# code from OpenAPI/Swagger specifications.

## Features

- ✅ **Swagger 2.0 Support**: Full support for Swagger 2.0 JSON specifications
- ✅ **DTO Generation**: Creates C# classes with proper property mapping and attributes
- ✅ **FluentValidation**: Generates validators based on Swagger validation rules
- ✅ **Complex Schema Support**: Handles large, complex schemas (tested with Stripe's API)
- ✅ **Reference Resolution**: Properly resolves `$ref` properties to their correct types
- ✅ **Inheritance Support**: Handles `allOf` for schema inheritance
- ✅ **Validation Rules**: Converts Swagger constraints to FluentValidation rules

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Building

```bash
# Clone the repository
git clone https://github.com/asynkron/SwaggerGen.git
cd SwaggerGen

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running

#### Default Usage (Petstore Sample)

```bash
dotnet run --project src/SwaggerGen
```

#### With Custom Swagger File

```bash
# Use a relative path
dotnet run --project src/SwaggerGen my-api-spec.json

# Use an absolute path  
dotnet run --project src/SwaggerGen /path/to/swagger.json

# Use the included Stripe example
dotnet run --project src/SwaggerGen testdata/stripe-swagger.json
```

#### Command Line Options

```
Usage: SwaggerGen [swagger-file-path]
  swagger-file-path: Path to a Swagger 2.0 JSON file (optional)

Examples:
  SwaggerGen
  SwaggerGen petstore-swagger.json
  SwaggerGen /path/to/my-api.json
  SwaggerGen testdata/stripe-swagger.json
```

## Output

The generator produces two types of files for each definition in your Swagger spec:

### DTO Classes

```csharp
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SwaggerGen.Generated;

/// <summary>
/// Customer object description
/// </summary>
public class Customer
{
    [JsonProperty("id")]
    [Required]
    public string Id { get; set; }

    [JsonProperty("email")]
    [MaxLength(512)]
    public string Email { get; set; }

    [JsonProperty("address")]
    public Address Address { get; set; }
}
```

### FluentValidation Validators

```csharp
using FluentValidation;

namespace SwaggerGen.Generated.Validators;

/// <summary>
/// Validator for Customer
/// </summary>
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Matches("^cus_[a-zA-Z0-9]+$");
            
        RuleFor(x => x.Email)
            .MaximumLength(512);
    }
}
```

## Supported Swagger Features

### Schema Types
- ✅ `string`, `integer`, `number`, `boolean`, `array`, `object`
- ✅ Format specifications (`date-time`, `int64`, `float`, etc.)
- ✅ `$ref` references to other definitions
- ✅ `allOf` for inheritance

### Validation Constraints
- ✅ `required` fields → `[Required]` and `.NotEmpty()`
- ✅ `minLength`/`maxLength` → `[MinLength]`/`[MaxLength]` and validation rules
- ✅ `minimum`/`maximum` → `.GreaterThanOrEqualTo()`/`.LessThanOrEqualTo()`
- ✅ `pattern` → `.Matches()` with regex
- ✅ `enum` values → validation rules

### Complex Scenarios
- ✅ Large schemas (tested with 22+ definitions)
- ✅ Nested object references
- ✅ Arrays with typed items
- ✅ Properties without explicit types (handled gracefully)

## Project Structure

```
SwaggerGen/
├── src/SwaggerGen/                    # Main application
│   ├── CodeGeneration/                # Code generation logic
│   ├── Models/                        # Swagger document models
│   ├── Templates/                     # Handlebars templates
│   ├── Samples/                       # Sample Swagger files
│   ├── Program.cs                     # Console app entry point
│   └── SwaggerParser.cs               # Core parsing logic
├── testdata/                          # Test Swagger files
│   └── stripe-swagger.json            # Complex real-world example
├── tests/SwaggerGen.Tests/            # Unit tests
└── SwaggerGen.sln                     # Solution file
```

## Testing

Run the comprehensive test suite:

```bash
dotnet test
```

The tests include:
- Basic DTO and validator generation
- Complex schema handling (including Stripe API)
- `$ref` reference resolution
- Validation rule generation
- Error handling for edge cases

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Technologies Used

- **Target Framework**: .NET 8.0
- **JSON Processing**: Newtonsoft.Json for parsing Swagger documents
- **Templating**: HandleBars.Net for code generation templates
- **Testing**: xUnit for unit tests
- **Validation**: FluentValidation for generated validators