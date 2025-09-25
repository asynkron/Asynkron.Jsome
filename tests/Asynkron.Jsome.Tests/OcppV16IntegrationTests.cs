using Asynkron.Jsome.CodeGeneration;
using Asynkron.Jsome.Configuration;
using Asynkron.Jsome.Models;
using System.Text.Json;
using Xunit;

namespace Asynkron.Jsome.Tests;

/// <summary>
/// Integration tests demonstrating end-to-end OCPP 1.6 compliance with real-world scenarios
/// </summary>
public class OcppV16IntegrationTests
{
    [Fact]
    public void CompleteOcppWorkflow_GeneratesValidCode_ForBootAndDataTransfer()
    {
        // Arrange - Complete OCPP BootNotification + DataTransfer workflow
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BootNotificationRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["chargePointVendor"] = new Schema { Type = "string", MaxLength = 20 },
                        ["chargePointModel"] = new Schema { Type = "string", MaxLength = 20 },
                        ["firmwareVersion"] = new Schema { Type = "string", MaxLength = 50 }
                    },
                    Required = new List<string> { "chargePointVendor", "chargePointModel" }
                },
                ["BootNotificationResponse"] = new Schema
                {
                    Type = "object", 
                    Properties = new Dictionary<string, Schema>
                    {
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = new List<object> { "Accepted", "Pending", "Rejected" }
                        },
                        ["currentTime"] = new Schema { Type = "string", Format = "date-time" },
                        ["interval"] = new Schema { Type = "integer" }
                    },
                    Required = new List<string> { "status", "currentTime", "interval" }
                },
                ["DataTransferResponse"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = new List<object> { "Accepted", "Rejected", "UnknownMessageId", "UnknownVendorId" }
                        },
                        ["data"] = new Schema { Type = "string" }
                    },
                    Required = new List<string> { "status" }
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true,
            ModifierConfiguration = new ModifierConfiguration
            {
                Global = new GlobalSettings
                {
                    Namespace = "OCPP.V16.Generated",
                    TypeNamePrefix = "V16",
                    GenerateEnumTypes = true
                }
            }
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document);

        // Assert - Complete OCPP message validation
        Assert.Equal(3, result.DtoClasses.Count);
        Assert.Equal(3, result.Validators.Count);

        // Validate BootNotificationRequest
        var bootReq = result.DtoClasses["BootNotificationRequest"];
        Assert.Contains("System.Text.Json.Serialization", bootReq);
        Assert.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.Never)]", bootReq);
        Assert.Contains("[StringLength(20)]", bootReq);
        Assert.Contains("[StringLength(50)]", bootReq);

        // Validate BootNotificationResponse enum handling
        var bootResp = result.DtoClasses["BootNotificationResponse"];
        Assert.Contains("DateTime", bootResp);
        
        var bootRespValidator = result.Validators["BootNotificationResponse"];
        Assert.Contains("Accepted", bootRespValidator);
        Assert.Contains("Pending", bootRespValidator);
        Assert.Contains("Rejected", bootRespValidator);

        // Validate DataTransfer strict enum validation
        var dataTransferValidator = result.Validators["DataTransferResponse"];
        Assert.Contains("UnknownMessageId", dataTransferValidator);
        Assert.Contains("UnknownVendorId", dataTransferValidator);
        Assert.Contains("Must be one of:", dataTransferValidator);
    }

    [Fact]
    public void OcppGetConfigurationWorkflow_GeneratesProperlyTypedNestedStructures()
    {
        // Arrange - Realistic GetConfiguration request/response from OCPP 1.6 spec
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["GetConfigurationRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["key"] = new Schema 
                        { 
                            Type = "array",
                            Items = new Schema { Type = "string", MaxLength = 50 }
                        }
                    }
                },
                ["GetConfigurationResponse"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["configurationKey"] = new Schema 
                        { 
                            Type = "array",
                            Items = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["key"] = new Schema { Type = "string", MaxLength = 50 },
                                    ["readonly"] = new Schema { Type = "boolean" },
                                    ["value"] = new Schema { Type = "string", MaxLength = 500 }
                                },
                                Required = new List<string> { "key", "readonly" }
                            }
                        },
                        ["unknownKey"] = new Schema 
                        { 
                            Type = "array",
                            Items = new Schema { Type = "string", MaxLength = 50 }
                        }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert - Nested object generation works correctly
        Assert.True(result.DtoClasses.Count >= 3); // Request + Response + ConfigurationKeyItem

        var response = result.DtoClasses["GetConfigurationResponse"];
        Assert.DoesNotContain("List<object>", response);
        Assert.Contains("GetConfigurationResponseConfigurationKeyItem", response);
        Assert.Contains("List<string>", response); // unknownKey should remain as string array

        // Verify the nested object class was generated
        var hasNestedClass = result.DtoClasses.Keys.Any(k => k.Contains("ConfigurationKeyItem"));
        Assert.True(hasNestedClass, "Should generate a separate class for nested configurationKey items");

        // Verify validation is properly applied to nested structures
        var hasNestedValidator = result.Validators.Keys.Any(k => k.Contains("ConfigurationKeyItem"));
        Assert.True(hasNestedValidator, "Should generate validator for nested configurationKey items");
    }

    [Fact]
    public void OcppMeterValuesComplexNesting_HandlesMultipleLevelsCorrectly()
    {
        // Arrange - Complex MeterValues structure with multiple nesting levels
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["MeterValuesRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["connectorId"] = new Schema { Type = "integer" },
                        ["transactionId"] = new Schema { Type = "integer" },
                        ["meterValue"] = new Schema 
                        { 
                            Type = "array",
                            Items = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["timestamp"] = new Schema { Type = "string", Format = "date-time" },
                                    ["sampledValue"] = new Schema
                                    {
                                        Type = "array",
                                        Items = new Schema
                                        {
                                            Type = "object",
                                            Properties = new Dictionary<string, Schema>
                                            {
                                                ["value"] = new Schema { Type = "string" },
                                                ["context"] = new Schema 
                                                { 
                                                    Type = "string",
                                                    Enum = new List<object> { "Interruption.Begin", "Interruption.End", "Sample.Clock", "Sample.Periodic" }
                                                },
                                                ["format"] = new Schema 
                                                { 
                                                    Type = "string",
                                                    Enum = new List<object> { "Raw", "SignedData" }
                                                },
                                                ["measurand"] = new Schema { Type = "string" },
                                                ["phase"] = new Schema { Type = "string" },
                                                ["location"] = new Schema { Type = "string" },
                                                ["unit"] = new Schema { Type = "string" }
                                            },
                                            Required = new List<string> { "value" }
                                        }
                                    }
                                },
                                Required = new List<string> { "timestamp", "sampledValue" }
                            }
                        }
                    },
                    Required = new List<string> { "connectorId", "meterValue" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert - Multiple levels of nesting should generate separate classes
        var mainDto = result.DtoClasses["MeterValuesRequest"];
        
        // Should not have any List<object> anywhere
        Assert.DoesNotContain("List<object>", mainDto);
        
        // Should have properly typed arrays for each level
        Assert.Contains("MeterValuesRequestMeterValueItem", mainDto);
        
        // Should generate classes for each nesting level
        Assert.True(result.DtoClasses.Count >= 3, "Should generate main class + at least 2 nested classes");
        
        // Verify nested structures have proper types
        var meterValueKeys = result.DtoClasses.Keys.Where(k => k.Contains("MeterValue")).ToList();
        Assert.NotEmpty(meterValueKeys);
        
        // Verify enum validation in deeply nested structures
        var hasEnumValidation = result.Validators.Values.Any(v => 
            v.Contains("Interruption.Begin") || v.Contains("Sample.Clock"));
        Assert.True(hasEnumValidation, "Should validate enums in deeply nested structures");
    }
}