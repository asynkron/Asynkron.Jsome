#!/bin/bash

set -e  # Exit on any error

echo "=== SwaggerGen Generation Script ==="
echo

# Build the project first
echo "🔨 Building SwaggerGen..."
dotnet build
echo "✅ Build completed successfully"
echo

# Create output directories
echo "📁 Creating output directories..."
mkdir -p generated/ocppv16
mkdir -p generated/guidewire/claims
echo "✅ Output directories created"
echo

# Generate OCPP v1.6 schemas
echo "⚡ Generating OCPP v1.6 schemas..."
dotnet run --project src/SwaggerGen -- generate \
    --schema-dir schemas/ocppv16/json_schemas \
    --config ocpp-config.yaml \
    --output generated/ocppv16 \
    --yes
echo "✅ OCPP v1.6 generation completed"
echo

# Generate Guidewire Claims
echo "🏢 Generating Guidewire Claims..."
dotnet run --project src/SwaggerGen -- generate \
    schemas/guidewire/claims.json \
    --config guidewire-config.yaml \
    --output generated/guidewire/claims \
    --yes
echo "✅ Guidewire Claims generation completed"
echo

echo "🎉 All code generation completed successfully!"
echo
echo "Generated files:"
echo "  - OCPP v1.6: ./generated/ocppv16/"
echo "  - Guidewire Claims: ./generated/guidewire/claims/"