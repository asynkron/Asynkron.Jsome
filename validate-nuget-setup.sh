#!/bin/bash

# NuGet Publishing Validation Script
# This script validates that the NuGet publishing setup is ready

echo "🔍 Validating NuGet Publishing Setup"
echo "===================================="

# Check if we can build
echo -n "✓ Building project... "
if dotnet build --configuration Release > /dev/null 2>&1; then
    echo "✅ PASS"
else
    echo "❌ FAIL - Build errors"
    exit 1
fi

# Check if we can run tests
echo -n "✓ Running tests... "
if dotnet test --configuration Release --verbosity quiet > /dev/null 2>&1; then
    echo "✅ PASS"
else
    echo "❌ FAIL - Test failures"
    exit 1
fi

# Check if we can create NuGet package
echo -n "✓ Creating NuGet package... "
rm -rf ./validation-nupkg
if dotnet pack src/Asynkron.Jsome/Asynkron.Jsome.csproj --configuration Release --output ./validation-nupkg -p:PackageVersion=1.0.0-validation --verbosity quiet > /dev/null 2>&1; then
    echo "✅ PASS"
else
    echo "❌ FAIL - Package creation failed"
    exit 1
fi

# Check if package was created with correct name
echo -n "✓ Verifying package file exists... "
if ls ./validation-nupkg/dotnet-jsome.1.0.0-validation.nupkg > /dev/null 2>&1; then
    echo "✅ PASS"
else
    echo "❌ FAIL - Package file not found"
    exit 1
fi

# Check if package contains expected files
echo -n "✓ Verifying package contents... "
# Extract package to check contents
mkdir -p ./validation-extract
unzip -q ./validation-nupkg/dotnet-jsome.1.0.0-validation.nupkg -d ./validation-extract

# Check for key files
missing_files=()
if [ ! -f "./validation-extract/tools/net8.0/any/Asynkron.Jsome.dll" ]; then
    missing_files+=("Asynkron.Jsome.dll")
fi
if [ ! -f "./validation-extract/README.md" ]; then
    missing_files+=("README.md")
fi
if [ ! -d "./validation-extract/contentFiles/any/any/Templates" ]; then
    missing_files+=("Templates")
fi

if [ ${#missing_files[@]} -eq 0 ]; then
    echo "✅ PASS"
else
    echo "❌ FAIL - Missing files: ${missing_files[*]}"
    exit 1
fi

# Check GitHub Actions workflow
echo -n "✓ Checking GitHub Actions workflow... "
if [ -f ".github/workflows/publish.yml" ]; then
    echo "✅ PASS"
else
    echo "❌ FAIL - publish.yml not found"
    exit 1
fi

# Cleanup
rm -rf ./validation-nupkg ./validation-extract

echo ""
echo "🎉 All validations passed!"
echo ""
echo "📝 To publish a new release:"
echo "   1. git tag v1.0.0"
echo "   2. git push origin v1.0.0"
echo "   3. Monitor GitHub Actions for publishing status"
echo ""
echo "🔑 Required GitHub secret: NUGET_API_KEY"
echo "   Set this in: Settings > Secrets and variables > Actions"
echo ""