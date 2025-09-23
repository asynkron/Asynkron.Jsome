using System.Globalization;
using SwaggerGen.CodeGeneration;
using SwaggerGen.Models;
using Xunit;

namespace SwaggerGen.Tests;

public class LocaleDebugTests
{
    [Fact]
    public void Debug_MultipleOfGeneration_WithSwedishLocale()
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
            
            // Test decimal formatting in Swedish culture
            var testDecimal = 0.5m;
            var defaultFormatted = testDecimal.ToString(); // Should use Swedish format with comma
            var invariantFormatted = testDecimal.ToString(CultureInfo.InvariantCulture); // Should use dot
            
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

            // Debug output
            var validator = result.Validators["NumberTest"];
            
            Assert.True(false, $"Swedish culture default: '{defaultFormatted}', " +
                              $"Invariant culture: '{invariantFormatted}', " +
                              $"Generated validator: {validator}");
        }
        finally
        {
            // Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}