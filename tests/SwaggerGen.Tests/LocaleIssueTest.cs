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
            
            // Ensure we don't accidentally get Swedish locale comma formatting
            Assert.DoesNotContain("0,5", validator);
            
            // Check for either HTML encoded or plain format
            Assert.True(
                validator.Contains(".Must(x => x % 0.5 == 0)") || 
                validator.Contains(".Must(x =&gt; x % 0.5 == 0)"),
                $"Expected to find decimal with dot, but got: {validator}");
            Assert.True(
                validator.Contains("Must be a multiple of 0.5"),
                $"Expected message with dot, but got: {validator}");
        }
        finally
        {
            // Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Theory]
    [InlineData("sv-SE")] // Swedish - comma decimal separator
    [InlineData("de-DE")] // German - comma decimal separator
    [InlineData("fr-FR")] // French - comma decimal separator
    [InlineData("es-ES")] // Spanish - comma decimal separator
    [InlineData("it-IT")] // Italian - comma decimal separator
    public void CodeGenerator_UsesInvariantCultureForAllNumericValidation_AcrossLocales(string cultureCode)
    {
        // Save current culture
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        
        try
        {
            // Set the specific locale that uses comma as decimal separator
            var testCulture = new CultureInfo(cultureCode);
            CultureInfo.CurrentCulture = testCulture;
            CultureInfo.CurrentUICulture = testCulture;
            
            // Arrange - comprehensive test with various decimal values
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
                                Minimum = 0.25m,
                                Maximum = 99.75m
                            },
                            ["weight"] = new Schema 
                            { 
                                Type = "number",
                                MultipleOf = 2.5m,
                                Minimum = 1.1m,
                                Maximum = 100.9m
                            }
                        }
                    }
                }
            };

            var generator = new CodeGenerator();

            // Act
            var result = generator.GenerateCode(document, "Test.Generated");

            // Assert - should always use invariant culture (dots, not locale-specific separators)
            var validator = result.Validators["NumberTest"];
            
            // Should never contain comma decimal separators (regardless of locale)
            Assert.DoesNotContain("0,5", validator);
            Assert.DoesNotContain("0,25", validator);
            Assert.DoesNotContain("99,75", validator);
            Assert.DoesNotContain("2,5", validator);
            Assert.DoesNotContain("1,1", validator);
            Assert.DoesNotContain("100,9", validator);
            
            // Should always contain dot decimal separators (English/invariant format)
            Assert.Contains("0.5", validator);
            Assert.Contains("0.25", validator);
            Assert.Contains("99.75", validator);
            Assert.Contains("2.5", validator);
            Assert.Contains("1.1", validator);
            Assert.Contains("100.9", validator);
            
            // Verify all validation rules use proper formatting
            Assert.Contains(".Must(x => x % 0.5 == 0)", validator);
            Assert.Contains(".Must(x => x % 2.5 == 0)", validator);
            Assert.Contains(".GreaterThanOrEqualTo(0.25)", validator);
            Assert.Contains(".GreaterThanOrEqualTo(1.1)", validator);
            Assert.Contains(".LessThanOrEqualTo(99.75)", validator);
            Assert.Contains(".LessThanOrEqualTo(100.9)", validator);

            // Verify error messages use proper formatting
            Assert.Contains("Must be a multiple of 0.5", validator);
            Assert.Contains("Must be a multiple of 2.5", validator);
            Assert.Contains("Must be greater than or equal to 0.25", validator);
            Assert.Contains("Must be greater than or equal to 1.1", validator);
            Assert.Contains("Must be less than or equal to 99.75", validator);
            Assert.Contains("Must be less than or equal to 100.9", validator);
        }
        finally
        {
            // Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}