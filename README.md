# SwaggerGen

A C# code generator that processes Swagger 2.0 JSON files to produce C# DTO (Data Transfer Object) classes and FluentValidation validators. The project focuses on generating clean, type-safe C# code from OpenAPI/Swagger specifications with powerful configuration options for controlling the object graph generation.

## Features

- ✅ **Swagger 2.0 Support**: Full support for Swagger 2.0 JSON specifications
- ✅ **DTO Generation**: Creates C# classes with proper property mapping and attributes
- ✅ **FluentValidation**: Generates validators based on Swagger validation rules
- ✅ **Complex Schema Support**: Handles large, complex schemas (tested with Stripe's API)
- ✅ **Reference Resolution**: Properly resolves `$ref` properties to their correct types
- ✅ **Inheritance Support**: Handles `allOf` for schema inheritance
- ✅ **Validation Rules**: Converts Swagger constraints to FluentValidation rules
- ⭐ **Unified Modifier Configuration**: Control object graph generation via YAML or JSON configuration files
- ⭐ **Property Path Rules**: Fine-grained control over individual properties using dot notation paths
- ⭐ **Flexible Inclusion/Exclusion**: Include or exclude specific properties, classes, or entire object branches
- ⭐ **Custom Validation Override**: Override validation rules, messages, and constraints
- ⭐ **Type Mapping**: Map Swagger types to custom C# types
- ⭐ **Backward Compatible**: Works seamlessly with existing code without configuration

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

## Modifier Configuration

SwaggerGen supports advanced object graph control through YAML or JSON configuration files. This allows you to:

- Include or exclude specific properties and classes
- Override validation rules and messages
- Map types to custom C# types  
- Customize descriptions and default values
- Control the entire generation process

### Configuration File Format

Configuration files can be in YAML (.yml, .yaml) or JSON (.json) format:

#### YAML Example (`config.yaml`)

```yaml
# Global settings
global:
  namespace: "MyApi.Generated"
  generateEnumTypes: true
  defaultInclude: true
  includeDescriptions: true
  maxDepth: 8
  typeNamePrefix: "Api"      # Optional: Prefix for all type names
  typeNameSuffix: "DTO"      # Optional: Suffix for all type names

# Property-specific rules
rules:
  # Exclude sensitive properties
  "User.Password":
    include: false
  
  "Customer.CreditCard.Number":
    include: false

  # Custom validation for product names
  "Order.OrderDetail.Product.Name":
    include: true
    description: "Product name with enhanced validation"
    validation:
      required: true
      maxLength: 100
      pattern: "^[A-Za-z0-9\\s]+$"
      message: "Product name must contain only alphanumeric characters and spaces"

  # Type mapping for dates
  "Order.CreatedDate":
    include: true
    type: "DateTime"
    format: "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
    description: "Order creation timestamp"

  # Enhanced price validation
  "Product.Price":
    include: true
    validation:
      required: true
      minimum: 0.01
      maximum: 99999.99
      message: "Price must be between $0.01 and $99,999.99"
```

#### JSON Example (`config.json`)

```json
{
  "global": {
    "namespace": "MyApi.Generated",
    "generateEnumTypes": true,
    "defaultInclude": true,
    "includeDescriptions": true,
    "maxDepth": 8,
    "typeNamePrefix": "Api",
    "typeNameSuffix": "DTO"
  },
  "rules": {
    "User.Password": {
      "include": false
    },
    "Order.OrderDetail.Product.Name": {
      "include": true,
      "description": "Product name with enhanced validation",
      "validation": {
        "required": true,
        "maxLength": 100,
        "pattern": "^[A-Za-z0-9\\\\s]+$",
        "message": "Product name must contain only alphanumeric characters and spaces"
      }
    },
    "Order.CreatedDate": {
      "include": true,
      "type": "DateTime",
      "format": "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
      "description": "Order creation timestamp"
    }
  }
}
```

### Using Configuration Files

#### Programmatic Usage

```csharp
using SwaggerGen;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Configuration;

// Load configuration from file
var options = new CodeGenerationOptions
{
    ModifierConfigurationPath = "path/to/config.yaml",
    GenerateEnumTypes = true
};

// Or use configuration instance directly
var config = ConfigurationLoader.Load("config.yaml");
var options = new CodeGenerationOptions
{
    ModifierConfiguration = config,
    GenerateEnumTypes = true
};

var generator = new CodeGenerator(options);
var document = await SwaggerParser.ParseFileAsync("swagger.json");
var result = generator.GenerateCode(document, "MyApi.Generated");
```

### Property Path Syntax

Property paths use dot notation to specify the exact location in the object hierarchy:

- `"User"` - Targets the entire User class
- `"User.Name"` - Targets the Name property of User
- `"Order.OrderDetail"` - Targets the OrderDetail property of Order
- `"Order.OrderDetail.Product.Name"` - Targets the Name property of Product within OrderDetail within Order

### Configuration Options

#### Global Settings

- `namespace`: Override the target namespace for generated code
- `generateEnumTypes`: Enable/disable enum type generation
- `defaultInclude`: Default inclusion policy (default: true)
- `includeDescriptions`: Whether to include original descriptions
- `maxDepth`: Maximum depth for object graph traversal
- `typeNamePrefix`: Prefix to apply to all generated type names (DTOs, enums, constants)
- `typeNameSuffix`: Suffix to apply to all generated type names (DTOs, enums, constants)

#### Type Name Formatting

When `typeNamePrefix` and/or `typeNameSuffix` are specified, they are applied to all generated type names:

```yaml
global:
  typeNamePrefix: "Api"
  typeNameSuffix: "DTO"
```

This transforms type names as follows:
- `User` class becomes `ApiUserDTO`
- `OrderStatus` enum becomes `ApiOrderStatusDTO`
- `ProductCategoryConstants` class becomes `ApiProductCategoryConstantsDTO`
- All type references in generated code are automatically updated

**Note**: Property names are never affected by prefix/suffix - only type names change.

#### Property Rules

Each property rule can specify:

- `include`: Whether to include the property (boolean)
- `description`: Custom description override (string)
- `type`: Custom C# type mapping (string)
- `format`: Format specification (string)
- `default`: Default value override
- `validation`: Validation rule overrides (object)

#### Validation Overrides

- `required`: Whether the property is required (boolean)
- `minLength`/`maxLength`: String length constraints (integer)
- `minimum`/`maximum`: Numeric range constraints (decimal)
- `pattern`: Regular expression pattern (string)
- `message`: Custom validation message (string)

### Backward Compatibility

The modifier configuration system is fully backward compatible:

- **Missing `include`**: Defaults to `true` (included)
- **No configuration**: Behaves exactly like the original system
- **Partial configuration**: Only specified rules are applied, others use defaults
- **Legacy code**: Continues to work without any changes

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
- Modifier configuration system testing
- YAML and JSON configuration loading
- Property inclusion/exclusion rules
- Custom validation overrides

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
- **JSON Processing**: Newtonsoft.Json for parsing Swagger documents and JSON configurations
- **YAML Processing**: YamlDotNet for parsing YAML configuration files
- **Templating**: HandleBars.Net for code generation templates
- **Testing**: xUnit for unit tests
- **Validation**: FluentValidation for generated validators