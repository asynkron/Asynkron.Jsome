# Asynkron.Jsome

A C# code generator that converts Swagger 2.0 JSON files and JSON Schema directories into clean C# DTO classes and FluentValidation validators. Perfect for generating type-safe C# code from OpenAPI specifications.

## Installation

Install Asynkron.Jsome as a .NET global tool:

```bash
# Install the global tool
dotnet tool install -g dotnet-jsome

# Update to the latest version
dotnet tool update -g dotnet-jsome
```

**Requirements:** .NET 8.0 Runtime or SDK

## Standard Use Case

The most common use case is generating C# DTOs from a Swagger 2.0 specification:

### Quick Start

```bash
# Generate from Petstore sample (built-in)
jsome

# Generate from your own Swagger file
jsome my-api-spec.json

# Generate from a URL
jsome https://petstore.swagger.io/v2/swagger.json
```

### Example Output

**Input Swagger Schema:**
```json
{
  "User": {
    "type": "object",
    "properties": {
      "id": { "type": "integer", "format": "int64" },
      "name": { "type": "string" },
      "email": { "type": "string", "format": "email" }
    },
    "required": ["id", "name"]
  }
}
```

**Generated C# DTO:**
```csharp
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Generated;

public partial class User
{
    [JsonProperty("id")]
    [Required]
    public long Id { get; set; }

    [JsonProperty("name")]
    [Required]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }
}
```

**Generated FluentValidator:**
```csharp
using FluentValidation;

namespace Generated.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

## Tweaking the Standard Use Case

### Common Customizations

```bash
# Specify output directory and namespace
jsome my-api.json --output ./Generated --namespace MyProject.DTOs

# Use System.Text.Json instead of Newtonsoft.Json
jsome my-api.json --system-text-json

# Generate modern C# with nullable types and required keyword
jsome my-api.json --modern

# Generate C# records instead of classes
jsome my-api.json --modern --records

# Skip confirmation prompts
jsome my-api.json --yes
```

### Configuration Files

For more control, use a configuration file to customize generation:

**config.yaml:**
```yaml
global:
  namespace: "MyApi.Generated"
  generateEnumTypes: true

rules:
  # Exclude sensitive properties
  "User.Password":
    include: false
  
  # Custom validation
  "User.Email":
    validation:
      required: true
      pattern: "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"
      message: "Please enter a valid email address"
```

**Usage:**
```bash
jsome my-api.json --config config.yaml --output ./Generated
```

---

## Advanced Features

### JSON Schema Directory Support

Generate DTOs from multiple JSON Schema files:

```bash
# Process all JSON Schema files in a directory
jsome generate --schema-dir ./schemas --namespace MyProject.Generated
```

### Modern C# Features

**Nullable Reference Types & Required Keyword:**
```bash
jsome my-api.json --modern
```

Generates:
```csharp
public class User
{
    public required string Name { get; set; }  // Uses 'required' keyword
    public string? Email { get; set; }         // Explicitly nullable
}
```

**C# Records:**
```bash
jsome my-api.json --modern --records
```

Generates:
```csharp
public record User(
    [JsonProperty("name")] required string Name,
    [JsonProperty("email")] string? Email
);
```

### Swashbuckle Integration

Generate Swashbuckle attributes for rich OpenAPI documentation:

```bash
jsome my-api.json --swashbuckle-attributes
```

Generates:
```csharp
[JsonProperty("email")]
[SwaggerSchema(Description = "User's email address", Format = "email")]
[SwaggerExampleValue("john.doe@example.com")]
public string Email { get; set; }
```

### Custom Templates

Use custom Handlebars templates for different output formats:

```bash
# Use custom templates
jsome my-api.json --templates MyCustomTemplate.hbs

# Custom template directory
jsome my-api.json --template-dir ./my-templates
```

### Protocol Buffers Generation

Generate Protocol Buffers (.proto) files alongside C# code:

```bash
jsome my-api.json --proto --output ./generated
```

### Advanced Configuration

**Property Path Rules:**
```yaml
rules:
  # Target specific nested properties
  "Order.OrderDetail.Product.Name":
    include: true
    type: "string"
    validation:
      maxLength: 100
      pattern: "^[A-Za-z0-9\\s]+$"
  
  # Type name prefixes/suffixes
global:
  typeNamePrefix: "Api"
  typeNameSuffix: "DTO"  # User becomes ApiUserDTO
```

### Command Line Reference

```
Usage: jsome generate [options] [swagger-file-path]

Options:
  -s, --schema-dir <schema-dir>         Directory containing JSON Schema files
  -c, --config <config>                 Configuration file (YAML or JSON)
  -n, --namespace <namespace>           Override namespace
  -o, --output <output>                 Output directory
  -y, --yes                            Skip confirmation prompts
  -t, --template-dir <template-dir>     Custom template directory
  -m, --modern                         Enable modern C# features
  --records                            Generate C# records
  --system-text-json                   Use System.Text.Json
  --swashbuckle-attributes             Generate Swashbuckle attributes
  --proto                              Generate Protocol Buffers files
  --templates                          Custom template files
  -h, --help                           Show help
```

## Development

### Building from Source

```bash
git clone https://github.com/asynkron/Asynkron.Jsome.git
cd Asynkron.Jsome
dotnet build
dotnet test
dotnet run --project src/Asynkron.Jsome
```

### Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License.
