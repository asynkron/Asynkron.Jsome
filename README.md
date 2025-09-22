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
- 🆕 **Modern C# Features**: Support for nullable reference types (`string?`) and `required` keyword
- 🆕 **C# Records Support**: Generate immutable records instead of mutable classes
- 🆕 **Multiple DTO Styles**: Choose between traditional classes, modern classes, and records

## Installation

SwaggerGen is available as a .NET global tool. Install it using the following command:

```bash
# Install the global tool
dotnet tool install -g dotnet-swaggergen

# Update to the latest version
dotnet tool update -g dotnet-swaggergen

# Uninstall if needed
dotnet tool uninstall -g dotnet-swaggergen
```

### Prerequisites

- .NET 8.0 Runtime or SDK

## Usage

After installation, use the `swaggergen` command from anywhere in your terminal:

#### Default Usage (Petstore Sample)

```bash
swaggergen
```

#### With Custom Swagger File

```bash
# Use a relative path
swaggergen my-api-spec.json

# Use an absolute path  
swaggergen /path/to/swagger.json

# Use a remote URL
swaggergen https://petstore.swagger.io/v2/swagger.json
```

#### Command Line Options

```
Usage: swaggergen [swagger-file-path]
  swagger-file-path: Path to a Swagger 2.0 JSON file (optional)

Examples:
  swaggergen
  swaggergen petstore-swagger.json
  swaggergen /path/to/my-api.json
  swaggergen https://api.example.com/swagger.json
```

## Development

If you want to contribute to SwaggerGen or run it from source:

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

# Run from source
dotnet run --project src/SwaggerGen
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

## Modern C# Features

SwaggerGen supports modern C# language features that enable cleaner, more expressive code generation:

### Nullable Reference Types & Required Keyword

Use the `--modern` flag to enable nullable reference types and the `required` keyword:

```bash
swaggergen generate my-api.json --output ./Generated --modern
```

**Traditional approach (default):**
```csharp
public class User
{
    [Required]
    public string Name { get; set; }
    
    public string Email { get; set; }  // May be null but not explicitly marked
}
```

**Modern approach (`--modern`):**
```csharp
public class User
{
    public required string Name { get; set; }  // Uses 'required' keyword
    
    public string? Email { get; set; }         // Explicitly nullable
}
```

### C# Records

Generate immutable records using the `--records` flag:

```bash
swaggergen generate my-api.json --output ./Generated --modern --records
```

**Generated C# record:**
```csharp
public record User(
    [JsonProperty("id")] required int Id,
    [JsonProperty("name")] required string Name,
    [JsonProperty("email")] string? Email,
    [JsonProperty("age")] int? Age
);
```

### Feature Comparison

| Feature | Legacy | Modern Class | Modern Record |
|---------|--------|--------------|---------------|
| **Mutability** | Mutable | Mutable | Immutable |
| **Required Fields** | `[Required]` attribute | `required` keyword | `required` keyword |
| **Optional Fields** | Non-nullable | Nullable (`?`) | Nullable (`?`) |
| **Syntax** | Properties | Properties | Positional parameters |
| **Performance** | Standard | Standard | Optimized |
| **Equality** | Reference | Reference | Structural |

### CLI Options Summary

```bash
# Generate traditional classes (default)
swaggergen generate api.json --output ./Generated

# Generate modern classes with nullable types and required keyword
swaggergen generate api.json --output ./Generated --modern

# Generate modern records
swaggergen generate api.json --output ./Generated --modern --records

# View all available options
swaggergen generate --help
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

## For Maintainers

### Publishing a New Release

This project uses automated NuGet publishing via GitHub Actions. To publish a new version:

1. **Set up the NuGet API Key** (one-time setup):
   - Go to [NuGet.org](https://www.nuget.org/account/apikeys) and create an API key
   - In GitHub, go to repository Settings > Secrets and variables > Actions
   - Add a new secret named `NUGET_API_KEY` with your NuGet API key

2. **Create and push a release tag**:
   ```bash
   # Tag the current commit with version number
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **Monitor the release**:
   - The GitHub Actions workflow will automatically:
     - Build and test the project
     - Create a NuGet package with the version from the tag
     - Publish to NuGet.org
   - Check the Actions tab to monitor progress
   - The package will be available at: https://www.nuget.org/packages/dotnet-swaggergen/

### Versioning

- Use semantic versioning (e.g., v1.0.0, v1.1.0, v2.0.0)
- The workflow extracts the version from the git tag (removes the 'v' prefix)
- Pre-release versions can use suffixes (e.g., v1.0.0-beta.1)

### Validation

Before creating a release, you can validate the NuGet publishing setup:

```bash
# Run the validation script
./validate-nuget-setup.sh
```

This script checks:
- Build and test success
- NuGet package creation
- Package contents and structure
- GitHub Actions workflow configuration

### Troubleshooting

If the publishing fails:
1. Check the GitHub Actions logs in the "Actions" tab
2. Verify the `NUGET_API_KEY` secret is correctly set
3. Ensure the tag follows semantic versioning (vX.Y.Z)
4. Run the validation script locally to test package creation

## License

This project is licensed under the MIT License.

## Technologies Used

- **Target Framework**: .NET 8.0
- **JSON Processing**: Newtonsoft.Json for parsing Swagger documents and JSON configurations
- **YAML Processing**: YamlDotNet for parsing YAML configuration files
- **Templating**: HandleBars.Net for code generation templates
- **Testing**: xUnit for unit tests
- **Validation**: FluentValidation for generated validators