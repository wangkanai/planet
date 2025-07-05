#!/bin/bash

echo "Graphics Namespace Refactoring Cleanup Script"
echo "============================================="

# Check if we're in the right directory
if [ ! -f "Planet.slnx" ]; then
    echo "Error: Must be run from the root of the planet repository"
    exit 1
fi

# Remove the old Drawing folder
if [ -d "Drawing" ]; then
    echo "Removing old Drawing folder..."
    rm -rf Drawing
    echo "✓ Drawing folder removed"
else
    echo "✓ Drawing folder already removed"
fi

# Clean build artifacts in Graphics folder
echo "Cleaning build artifacts in Graphics folder..."
find Graphics -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find Graphics -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
echo "✓ Build artifacts cleaned"

# Verify the new structure exists
echo ""
echo "Verifying new Graphics structure..."
required_dirs=(
    "Graphics"
    "Graphics/Abstractions/src"
    "Graphics/Rasters/src/Root"
    "Graphics/Rasters/benchmark"
    "Graphics/Rasters/tests/Unit"
    "Graphics/Vectors/src/Root"
    "Graphics/Vectors/tests/Unit"
)

all_good=true
for dir in "${required_dirs[@]}"; do
    if [ -d "$dir" ]; then
        echo "✓ $dir exists"
    else
        echo "✗ $dir is missing!"
        all_good=false
    fi
done

if [ "$all_good" = true ]; then
    echo ""
    echo "✅ Refactoring completed successfully!"
    echo ""
    echo "Next steps:"
    echo "1. Review the changes"
    echo "2. Build the solution to ensure everything compiles"
    echo "3. Run tests to ensure functionality is preserved"
    echo "4. Commit the changes with the message in GRAPHICS_REFACTORING.md"
else
    echo ""
    echo "❌ Some directories are missing. Please check the refactoring."
fi
