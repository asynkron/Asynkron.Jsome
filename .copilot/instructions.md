# SwaggerGen - Copilot Instructions

## Project Overview

SwaggerGen is a C# code generator that processes Swagger 2.0 JSON files to produce custom C# DTO (Data Transfer Object) classes and FluentValidators. The project focuses on generating clean, type-safe C# code from OpenAPI/Swagger specifications.

## Key Technologies

- **Target Framework**: .NET 8.0
- **JSON Processing**: Newtonsoft.Json for parsing Swagger documents
- **Testing**: xUnit for unit tests
- **Future**: HandleBars templating engine for customizable code generation

## Architecture Overview

### Core Components

1. **SwaggerParser** (`src/SwaggerGen/SwaggerParser.cs`)
   - Main entry point for parsing Swagger 2.0 JSON documents
   - Validates Swagger document structure and version compatibility
   - Provides document summary functionality

2. **Models** (`src/SwaggerGen/Models/`)
   - `SwaggerDocument.cs`: Root model representing the entire Swagger spec
   - `Schema.cs`: Defines data types and validation rules
   - `PathItem.cs` & `Operation.cs`: API endpoint definitions
   - `Response.cs` & `Parameter.cs`: HTTP response and parameter models
   - `Info.cs`: API metadata

3. **Program.cs** (`src/SwaggerGen/Program.cs`)
   - Console application entry point
   - Demonstrates Swagger parsing with sample files

## Code Generation Goals

The project aims to generate two primary outputs:

1. **Plain DTO Objects**: Simple C# classes with properties matching Swagger schema definitions
2. **FluentValidators**: Validation classes for the root objects based on Swagger validation rules

## Development Guidelines

### Code Style

- Use nullable reference types (`<Nullable>enable</Nullable>`)
- Follow standard C# naming conventions (PascalCase for public members)
- Use XML documentation comments for public APIs
- Prefer explicit property initialization over nullable properties where possible

### Testing

- All parsers and generators should have comprehensive unit tests
- Use xUnit framework with standard Arrange-Act-Assert pattern
- Test files are located in `tests/SwaggerGen.Tests/`
- Sample Swagger files for testing are in `src/SwaggerGen/Samples/`

### Error Handling

- Validate Swagger documents strictly - only support Swagger 2.0
- Throw `ArgumentException` for invalid document structure
- Throw `JsonException` for malformed JSON
- Provide clear, actionable error messages

## Project Structure

```
SwaggerGen/
├── src/SwaggerGen/                    # Main application
│   ├── Models/                        # Swagger document models
│   ├── Samples/                       # Sample Swagger files
│   ├── Program.cs                     # Console app entry point
│   ├── SwaggerParser.cs               # Core parsing logic
│   └── SwaggerGen.csproj              # Project file
├── tests/SwaggerGen.Tests/            # Unit tests
└── SwaggerGen.sln                     # Solution file
```

## Building and Running

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the console application
dotnet run --project src/SwaggerGen
```

## Swagger 2.0 Support

### Supported Features

- Document metadata (info, host, basePath, schemes)
- Path definitions with HTTP operations
- Schema definitions for complex types
- Parameter definitions (query, path, body, header)
- Response definitions
- Validation constraints (min/max length, patterns, required fields)

### Validation Rules

- Must have `swagger: "2.0"` field (versions 2.x supported)
- Must have valid `info` section with title and version
- All `$ref` references should be resolvable within the document
- Schemas must follow JSON Schema draft 4 subset

## Future Enhancements

### Template System (Planned)

The project will implement a HandleBars-based templating system for:
- Customizable DTO class generation
- FluentValidator generation
- Support for different output formats and styles
- User-defined templates for specific frameworks

### Code Generation Features (Planned)

- Generate C# classes from Swagger definitions
- Create FluentValidation rules from schema constraints
- Support for inheritance through `allOf` schemas
- Generate XML documentation from Swagger descriptions
- Configurable naming conventions and namespace organization

## Contributing Guidelines

### When Adding New Features

1. **Parser Extensions**: Add new Swagger feature support to appropriate model classes
2. **Code Generation**: Follow the planned template-based approach
3. **Testing**: Ensure comprehensive test coverage for all new functionality
4. **Documentation**: Update XML docs and this instruction file

### Common Patterns

- Use `JsonProperty` attributes for all JSON deserialization
- Initialize collections with `new()` to avoid null reference issues
- Follow the existing error handling patterns
- Maintain backward compatibility with existing Swagger documents

### Performance Considerations

- Use async methods for file I/O operations
- Consider memory usage when processing large Swagger files
- Cache parsed schemas when possible for better performance

## Examples

### Basic Usage

```csharp
// Parse a Swagger file
var document = await SwaggerParser.ParseFileAsync("swagger.json");

// Get document summary
var summary = SwaggerParser.GetDocumentSummary(document);

// Access specific definitions
var petSchema = document.Definitions["Pet"];
var properties = petSchema.Properties;
```

### Working with Schemas

```csharp
// Check if a property is required
var isRequired = schema.Required.Contains("propertyName");

// Get validation constraints
var maxLength = schema.MaxLength;
var pattern = schema.Pattern;
var minimum = schema.Minimum;
```

This project is actively developed and welcomes contributions that align with the goal of creating a robust, template-based Swagger code generator for .NET applications.