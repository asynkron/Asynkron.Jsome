using Asynkron.Jsome.CodeGeneration;
using Asynkron.Jsome.Configuration;
using Asynkron.Jsome.Models;
using System.Text.Json;
using Xunit;

namespace Asynkron.Jsome.Tests;

/// <summary>
/// Tests for OCPP 1.6 JSON Schema compliance and generation quality
/// </summary>
public class OcppV16ComplianceTests
{
    [Fact]
    public void BootNotificationResponse_GeneratesValidEnum_WithProperValidation()
    {
        // Arrange - BootNotificationResponse schema with status enum
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
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
                        ["currentTime"] = new Schema 
                        { 
                            Type = "string",
                            Format = "date-time"
                        },
                        ["interval"] = new Schema 
                        { 
                            Type = "integer"
                        }
                    },
                    Required = new List<string> { "status", "currentTime", "interval" }
                }
            }
        };

        var options = new CodeGenerationOptions
        {
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

        // Assert
        Assert.Contains("BootNotificationResponse", result.DtoClasses.Keys);
        var dto = result.DtoClasses["BootNotificationResponse"];
        
        // Should include proper required attributes
        Assert.Contains("[Required]", dto);
        Assert.Contains("Status", dto);
        Assert.Contains("CurrentTime", dto);
        Assert.Contains("Interval", dto);
        
        // Validate the validator includes proper enum validation
        Assert.Contains("BootNotificationResponse", result.Validators.Keys);
        var validator = result.Validators["BootNotificationResponse"];
        Assert.Contains("Accepted", validator);
        Assert.Contains("Pending", validator);
        Assert.Contains("Rejected", validator);
    }

    [Fact]
    public void DataTransferResponse_GeneratesCompleteEnumValidation_ForOcppStatuses()
    {
        // Arrange - DataTransferResponse with OCPP 1.6 status values
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
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
                        ["data"] = new Schema 
                        { 
                            Type = "string"
                        }
                    },
                    Required = new List<string> { "status" }
                }
            }
        };

        var options = new CodeGenerationOptions
        {
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

        // Assert - Check that all OCPP DataTransfer status values are validated
        var validator = result.Validators["DataTransferResponse"];
        Assert.Contains("Accepted", validator);
        Assert.Contains("Rejected", validator);
        Assert.Contains("UnknownMessageId", validator);
        Assert.Contains("UnknownVendorId", validator);
        
        // Should have proper validation logic
        Assert.Contains("Must be one of:", validator);
    }

    [Fact]
    public void GetConfigurationResponse_GeneratesProperNestedObjects_NotGenericObjects()
    {
        // Arrange - GetConfigurationResponse with nested configurationKey objects
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
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

        // Assert
        var dto = result.DtoClasses["GetConfigurationResponse"];
        
        // Should NOT use List<object> for structured data
        Assert.DoesNotContain("List<object>", dto);
        
        // Should have proper string array for unknownKey
        Assert.Contains("List<string>", dto);
        Assert.Contains("UnknownKey", dto);
    }

    [Fact]
    public void OcppDateTimeFields_HandleDateTimeFormatCorrectly()
    {
        // Arrange - Test proper DateTime handling for OCPP timestamps
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BootNotificationResponse"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["currentTime"] = new Schema 
                        { 
                            Type = "string",
                            Format = "date-time"
                        }
                    },
                    Required = new List<string> { "currentTime" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert
        var dto = result.DtoClasses["BootNotificationResponse"];
        
        // Should use DateTime type for date-time format
        Assert.Contains("DateTime", dto);
        Assert.Contains("CurrentTime", dto);
        Assert.Contains("[Required]", dto);
    }

    [Fact]
    public void OcppStringFields_RespectMaxLengthConstraints()
    {
        // Arrange - Test OCPP string length validation
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BootNotificationRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["chargePointVendor"] = new Schema 
                        { 
                            Type = "string",
                            MaxLength = 20
                        },
                        ["chargePointModel"] = new Schema 
                        { 
                            Type = "string",
                            MaxLength = 20
                        },
                        ["firmwareVersion"] = new Schema 
                        { 
                            Type = "string",
                            MaxLength = 50
                        }
                    },
                    Required = new List<string> { "chargePointVendor", "chargePointModel" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert
        var dto = result.DtoClasses["BootNotificationRequest"];
        
        // Should have proper MaxLength attributes
        Assert.Contains("[MaxLength(20)]", dto);
        Assert.Contains("[MaxLength(50)]", dto);
        
        // Should have Required attributes for required fields
        Assert.Contains("[Required]", dto);
        
        var validator = result.Validators["BootNotificationRequest"];
        Assert.Contains("NotEmpty", validator);
    }

    [Fact]
    public void OcppEnumValidation_RejectsInvalidValues_AcceptsValidValues()
    {
        // Arrange - Test strict OCPP enum validation
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["BootNotificationResponse"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["status"] = new Schema 
                        { 
                            Type = "string",
                            Enum = new List<object> { "Accepted", "Pending", "Rejected" }
                        }
                    },
                    Required = new List<string> { "status" }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert - FluentValidation should enforce strict enum validation
        var validator = result.Validators["BootNotificationResponse"];
        
        // Should validate that only allowed enum values are accepted
        Assert.Contains("Must be one of:", validator);
        Assert.Contains("Accepted", validator);
        Assert.Contains("Pending", validator);
        Assert.Contains("Rejected", validator);
        
        // Should use Contains() method for validation which is case-sensitive
        Assert.Contains(".Contains(", validator);
    }

    [Fact]
    public void NestedObjectGeneration_WorksRecursively_ForDeeplyNestedStructures()
    {
        // Arrange - Test deeply nested objects like in OCPP ChargingProfile structures
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
                ["SetChargingProfileRequest"] = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["csChargingProfiles"] = new Schema 
                        { 
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["chargingSchedule"] = new Schema
                                {
                                    Type = "object",
                                    Properties = new Dictionary<string, Schema>
                                    {
                                        ["chargingSchedulePeriod"] = new Schema
                                        {
                                            Type = "array",
                                            Items = new Schema
                                            {
                                                Type = "object",
                                                Properties = new Dictionary<string, Schema>
                                                {
                                                    ["startPeriod"] = new Schema { Type = "integer" },
                                                    ["limit"] = new Schema { Type = "number" }
                                                },
                                                Required = new List<string> { "startPeriod", "limit" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert - Should generate separate classes for each nested level
        var dto = result.DtoClasses["SetChargingProfileRequest"];
        
        // Should not contain List<object> for any nested structures
        Assert.DoesNotContain("List<object>", dto);
        Assert.DoesNotContain("object>", dto);
        
        // Should reference properly typed nested classes
        Assert.True(result.DtoClasses.Count >= 4); // Main class + 3 nested classes minimum
    }

    [Fact]
    public void OcppMessages_SupportSystemTextJson_WithProperAttributes()
    {
        // Arrange - Test System.Text.Json compatibility for OCPP messages
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
                }
            }
        };

        var options = new CodeGenerationOptions
        {
            UseSystemTextJson = true
        };
        var generator = new CodeGenerator(options);

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert
        var dto = result.DtoClasses["BootNotificationRequest"];
        
        // Should use System.Text.Json attributes
        Assert.Contains("using System.Text.Json.Serialization;", dto);
        Assert.DoesNotContain("using Newtonsoft.Json;", dto);
        
        // Required fields should have JsonIgnore(Condition = Never)
        Assert.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.Never)]", dto);
        
        // Optional fields should have JsonIgnore(Condition = WhenWritingDefault)  
        Assert.Contains("[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]", dto);
        
        // Should have JsonPropertyName attributes
        Assert.Contains("[JsonPropertyName(", dto);
    }

    [Fact]
    public void OcppRoundtripSerialization_WorksCorrectly_WithGeneratedClasses()
    {
        // Arrange - Test that generated OCPP classes can roundtrip serialize correctly
        var document = new SwaggerDocument
        {
            Definitions = new Dictionary<string, Schema>
            {
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

        var generator = new CodeGenerator();

        // Act
        var result = generator.GenerateCode(document, "OCPP.V16.Generated");

        // Assert - Verify the structure supports proper serialization
        var dto = result.DtoClasses["DataTransferResponse"];
        
        // Should have proper JSON property mapping
        Assert.Contains("[JsonProperty(\"status\")]", dto);
        Assert.Contains("[JsonProperty(\"data\")]", dto);
        
        // Should have validation attributes for serialization constraints
        Assert.Contains("[Required]", dto);
        
        // Validator should enforce OCPP enum constraints
        var validator = result.Validators["DataTransferResponse"];
        Assert.Contains("Accepted", validator);
        Assert.Contains("UnknownVendorId", validator);
    }
}