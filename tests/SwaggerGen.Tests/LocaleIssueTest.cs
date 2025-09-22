using System.Globalization;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class LocaleIssueTests
{
    [Fact]
    public void CodeGenerator_HandlesNumberValidationConstraints_WithSwedishLocale()
    {
        // Save current culture
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        
        try
        {
            // Set Swedish locale (uses comma as decimal separator)
            var swedishCulture = new CultureInfo("sv-SE");
            CultureInfo.CurrentCulture = swedishCulture;
            CultureInfo.CurrentUICulture = swedishCulture;
            
            // Arrange
            var document = new SwaggerDocument
            {
                Definitions = new Dictionary<string, Schema>
                {
                    ["NumberTest"] = new Schema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Schema>
                        {
                            ["percentage"] = new Schema 
                            { 
                                Type = "number",
                                MultipleOf = 0.5m,
                                Minimum = 0,
                                Maximum = 100
                            }
                        }
                    }
                }
            };

            var generator = new CodeGenerator();

            // Act
            var result = generator.GenerateCode(document, "Test.Generated");

            // Assert - this should expect dots, not commas
            var validator = result.Validators["NumberTest"];
            Assert.Contains(".Must(x => x % 0.5 == 0)", validator);
            Assert.Contains("Must be a multiple of 0.5", validator);
        }
        finally
        {
            // Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}