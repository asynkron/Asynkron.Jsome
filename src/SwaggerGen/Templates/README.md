# SwaggerGen Templates

This directory contains Handlebars templates used for code generation from Swagger definitions.

## Available Templates

### DTO.hbs
Generates C# Data Transfer Object (DTO) classes from Swagger schema definitions.

**Context Variables:**
- `ClassName`: The name of the generated class
- `Description`: Optional description from the Swagger schema
- `Properties`: Array of property objects with the following structure:
  - `Name`: Property name in PascalCase
  - `Type`: C# type (string, int, etc.)
  - `Description`: Optional property description
  - `ValidationAttributes`: Array of C# validation attributes (e.g., `[Required]`, `[StringLength(50)]`)
  - `DefaultValue`: Optional default value for the property

### Validator.hbs
Generates FluentValidation validator classes for root DTOs.

**Context Variables:**
- `ClassName`: The name of the DTO class being validated
- `Description`: Optional description from the Swagger schema
- `Properties`: Array of property objects with the following structure:
  - `Name`: Property name in PascalCase
  - `ValidationRules`: Array of FluentValidation rule strings (e.g., `RuleFor(x => x.Name).NotEmpty();`)

## Customizing Templates

You can modify these templates to customize the generated code:

1. **Adding new using statements**: Add them at the top of the template
2. **Changing class structure**: Modify the class definition and property layouts
3. **Adding custom attributes**: Include them in the template where needed
4. **Modifying validation rules**: Update the Validator.hbs template to include additional validation logic

## Template Syntax

Templates use Handlebars.NET syntax:

- `{{variable}}`: Output a variable
- `{{#if condition}}...{{/if}}`: Conditional blocks
- `{{#each array}}...{{/each}}`: Loop over arrays
- `{{#unless condition}}...{{/unless}}`: Negative conditional
- `{{{variable}}}`: Output without HTML escaping (for raw code)

## Adding New Templates

To add a new template:

1. Create a new `.hbs` file in this directory
2. Update the `CodeGenerator.cs` class to load and use your template
3. Define the context data structure your template expects