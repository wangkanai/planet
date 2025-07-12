# Coding Style & Conventions

## C# Coding Guidelines

### Variable Naming
- **Descriptive variable names** - Always use meaningful names
- **PascalCase** for public members
- **camelCase** for private members
- **Use `var`** for local variables when type is obvious

### Method Style
- **Expression bodies** for single-line methods when possible
- **`async`/`await`** for asynchronous methods
- **No unused using directives**

### Type Usage
- **`IEnumerable<T>`** for return types when possible
- **`IEnumerable<T>`** for collections in method signatures
- **`IServiceCollection`** for dependency injection
- **`ILogger<T>`** for logging
- **`IConfiguration`** for configuration settings
- **`IOptions<T>`** for strongly typed configuration
- **`IHttpClientFactory`** for HTTP client creation
- **`IActionResult`** for controller actions

### Code Formatting (.editorconfig)
- **UTF-8** charset
- **Tab indentation** (4 spaces width)
- **CRLF** line endings
- **Trim trailing whitespace**
- **Insert final newline**
- **Next line braces** (C# brace style)
- **Max line length**: 500 characters

### File Headers
```csharp
// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0
```

## Unit Testing Guidelines
- **xUnit v3** framework
- **No '*Tests' suffix** in test namespace
- **`xunit.runner.json`** for test configuration
- **`Fact`** attribute for test methods
- **`Theory`** attribute for parameterized tests
- **`Assert`** methods for assertions

## Project Structure
- **Clean Architecture** patterns (Domain, Application, Infrastructure, Persistence)
- **Modular design** with clear separation of concerns
- **Namespace alignment** with project structure