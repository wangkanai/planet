# Task Completion Checklist

## Code Quality Checks
After completing any coding task, ensure these steps are followed:

### 1. Build Verification
```bash
dotnet build -c Release -tl          # Verify solution builds cleanly
```

### 2. Testing
```bash
dotnet test                          # Run all unit tests
# Run specific tests if relevant to changes:
dotnet test --project <affected-test-project>
```

### 3. Code Formatting
- **EditorConfig** automatically enforces formatting rules
- **ReSharper settings** handle C# specific formatting
- Files are automatically formatted on save in JetBrains Rider

### 4. Performance Testing (if applicable)
```bash
# For Graphics/Rasters changes:
dotnet run --project Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks -c Release

# For other components with benchmarks:
dotnet run --project <component>/benchmark -c Release
```

### 5. Database Migrations (Portal changes)
```powershell
# If Entity Framework models changed:
./Portal/db.ps1 -add "DescriptiveMigrationName"
./Portal/db.ps1 -update
```

### 6. Frontend Assets (Portal UI changes)
```bash
npm run deploy                       # Clean, lib, and build CSS
```

### 7. Integration Testing
```bash
# Test application startup:
dotnet run --project Portal/src/Server       # Verify Portal runs
dotnet run --project Engine/src/Console      # Verify Engine runs
```

## Code Review Checklist
- [ ] **Follows coding conventions** (PascalCase, camelCase, descriptive names)
- [ ] **Uses appropriate C# patterns** (async/await, IEnumerable<T>, etc.)
- [ ] **Includes unit tests** for new functionality
- [ ] **Updates documentation** if public API changed
- [ ] **No unused using directives**
- [ ] **Proper error handling** and logging
- [ ] **Resource disposal** patterns (especially for Graphics operations)

## Pre-Commit Validation
1. **Build succeeds** in Release configuration
2. **All tests pass**
3. **No SonarCloud issues** introduced
4. **Performance benchmarks** stable (if applicable)
5. **Documentation updated** (README, API docs)

## MCP Integration Checks
- [ ] **SonarCloud analysis** completed via MCP
- [ ] **GitHub integration** working (issues, PRs)
- [ ] **ConPort context** updated if architecture changed