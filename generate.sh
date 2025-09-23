#!/bin/bash

set -e  # Exit on any error

# Default paths - can be overridden by environment variables
SCHEMA_DIR=${SWAGGERGEN_SCHEMA_DIR:-"schemas/ocppv16/json_schemas"}
OCPP_CONFIG=${SWAGGERGEN_OCPP_CONFIG:-"ocpp-config.yaml"}
GUIDEWIRE_CONFIG=${SWAGGERGEN_GUIDEWIRE_CONFIG:-"guidewire-config.yaml"}
GUIDEWIRE_SCHEMA=${SWAGGERGEN_GUIDEWIRE_SCHEMA:-"schemas/guidewire/claims.json"}
OUTPUT_DIR=${SWAGGERGEN_OUTPUT_DIR:-"generated"}

echo "=== SwaggerGen Generation Script ==="
echo "Configuration:"
echo "  Schema Directory: $SCHEMA_DIR"
echo "  OCPP Config: $OCPP_CONFIG"
echo "  Guidewire Config: $GUIDEWIRE_CONFIG"
echo "  Guidewire Schema: $GUIDEWIRE_SCHEMA"
echo "  Output Directory: $OUTPUT_DIR"
echo

# Build the project first
echo "🔨 Building SwaggerGen..."
dotnet build
echo "✅ Build completed successfully"
echo

# Create output directories
echo "📁 Creating output directories..."
mkdir -p "${OUTPUT_DIR}/ocppv16"
mkdir -p "${OUTPUT_DIR}/guidewire/claims"
echo "✅ Output directories created"
echo

# Generate OCPP v1.6 schemas
echo "⚡ Generating OCPP v1.6 schemas..."
if [ -d "$SCHEMA_DIR" ]; then
    dotnet run --project src/SwaggerGen -- generate \
        --schema-dir "$SCHEMA_DIR" \
        --config "$OCPP_CONFIG" \
        --output "${OUTPUT_DIR}/ocppv16" \
        --yes
    echo "✅ OCPP v1.6 generation completed"
else
    echo "⚠️  OCPP schema directory not found: $SCHEMA_DIR"
    echo "   Skipping OCPP v1.6 generation"
fi
echo

# Generate Guidewire Claims
echo "🏢 Generating Guidewire Claims..."
if [ -f "$GUIDEWIRE_SCHEMA" ]; then
    dotnet run --project src/SwaggerGen -- generate \
        "$GUIDEWIRE_SCHEMA" \
        --config "$GUIDEWIRE_CONFIG" \
        --output "${OUTPUT_DIR}/guidewire/claims" \
        --yes
    echo "✅ Guidewire Claims generation completed"
else
    echo "⚠️  Guidewire schema file not found: $GUIDEWIRE_SCHEMA"
    echo "   Skipping Guidewire Claims generation"
fi
echo

echo "🎉 All code generation completed successfully!"
echo
echo "Generated files:"
echo "  - OCPP v1.6: ./${OUTPUT_DIR}/ocppv16/"
echo "  - Guidewire Claims: ./${OUTPUT_DIR}/guidewire/claims/"
echo
echo "Environment variables used:"
echo "  SWAGGERGEN_SCHEMA_DIR=$SCHEMA_DIR"
echo "  SWAGGERGEN_OCPP_CONFIG=$OCPP_CONFIG"
echo "  SWAGGERGEN_GUIDEWIRE_CONFIG=$GUIDEWIRE_CONFIG"
echo "  SWAGGERGEN_GUIDEWIRE_SCHEMA=$GUIDEWIRE_SCHEMA"
echo "  SWAGGERGEN_OUTPUT_DIR=$OUTPUT_DIR"