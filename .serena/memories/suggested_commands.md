# Essential Development Commands

## Build Commands
```bash
# Primary build commands
dotnet build -c Release -tl        # Build entire solution in Release
dotnet clean -c Release -tl        # Clean build artifacts
./build.ps1                        # Full build script (clean, restore, build)
dotnet restore                     # Restore NuGet packages
```

## Testing Commands
```bash
# Test execution
dotnet test                                    # Run all tests
dotnet test --project <specific-test-project> # Run specific project tests

# Benchmarking
dotnet run --project Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks -c Release
```

## Application Execution
```bash
# Run applications
dotnet run --project Portal/src/Server       # Portal web application
dotnet run --project Engine/src/Console      # Engine console application
```

## Database Management (Portal)
```powershell
# Entity Framework migrations
./Portal/db.ps1 -add "MigrationName"  # Add new migration
./Portal/db.ps1 -list                 # List migrations
./Portal/db.ps1 -remove               # Remove last migration
./Portal/db.ps1 -update               # Update to latest migration
./Portal/db.ps1 -clean                # Clean all migrations
./Portal/db.ps1 -reset                # Reset and create initial migration
```

## Frontend Development (Portal)
```bash
# CSS/SCSS processing
npm run build                         # Build CSS from SCSS
npm run watch                         # Watch and rebuild CSS
npm run lib                          # Copy library files to wwwroot
npm run clean                        # Clean generated files
npm run deploy                       # Full deployment build
```

## Engine Console Build
```powershell
./Engine/src/Console/build.ps1       # Build and publish as 'tiler' executable
```

## macOS System Commands
```bash
# Standard Unix commands work on macOS (Darwin)
ls                                   # List directory contents
cd                                   # Change directory
grep                                 # Search text patterns
find                                 # Find files
git                                  # Version control
```

## Quality & Linting
- **Code formatting**: Handled by EditorConfig and ReSharper settings
- **Code analysis**: SonarCloud integration via MCP
- **Testing**: xUnit v3 with testing platform support