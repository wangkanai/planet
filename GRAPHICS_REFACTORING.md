# Refactor: Rename Drawing namespace and folder to Graphics

## Summary
This PR refactors the Drawing namespace to Graphics as discussed, making it more appropriate for the broader scope of image manipulation operations.

## Changes Made

### Namespace Changes
- Changed namespace from `Wangkanai.Planet.Drawing` to `Wangkanai.Graphics`
- This better reflects the comprehensive nature of the module which includes:
  - Image manipulation
  - Light adjustment
  - Color adjustment
  - Both raster and vector operations

### Directory Structure Changes
- Renamed `Drawing/` folder to `Graphics/`
- Updated all subdirectories:
  - `Drawing/Abstractions/` → `Graphics/Abstractions/`
  - `Drawing/Rasters/` → `Graphics/Rasters/`
  - `Drawing/Vectors/` → `Graphics/Vectors/`

### Project File Updates
- Renamed all project files:
  - `Wangkanai.Planet.Drawing.Abstractions.csproj` → `Wangkanai.Graphics.Abstractions.csproj`
  - `Wangkanai.Planet.Drawing.Rasters.csproj` → `Wangkanai.Graphics.Rasters.csproj`
  - `Wangkanai.Planet.Drawing.Vectors.csproj` → `Wangkanai.Graphics.Vectors.csproj`
  - `Wangkanai.Planet.Drawing.Rasters.Benchmark.csproj` → `Wangkanai.Graphics.Rasters.Benchmark.csproj`
  - `Wangkanai.Planet.Drawing.Rasters.UnitTests.csproj` → `Wangkanai.Graphics.Rasters.UnitTests.csproj`
  - `Wangkanai.Planet.Drawing.Vectors.UnitTests.csproj` → `Wangkanai.Graphics.Vectors.UnitTests.csproj`

### Code Changes
- Updated all namespace declarations in C# files
- Updated all project references to use the new project names
- Updated all using statements that referenced the old namespace

### Documentation Updates
- Updated README files to reflect the new naming:
  - Main Graphics README
  - Rasters README
  - Vectors README

### Solution File Updates
- Updated `Planet.slnx` to reference the new folder structure and project names

## Migration Guide

For any code referencing the old namespace, update as follows:

```csharp
// Old
using Wangkanai.Planet.Drawing;
using Wangkanai.Planet.Drawing.Rasters;
using Wangkanai.Planet.Drawing.Vectors;

// New
using Wangkanai.Graphics;
using Wangkanai.Graphics.Rasters;
using Wangkanai.Graphics.Vectors;
```

## Notes
- The old Drawing folder should be removed after this PR is merged
- Build artifacts (bin/obj folders) were not copied to the new structure
- All test projects have been updated to reference the new namespace
