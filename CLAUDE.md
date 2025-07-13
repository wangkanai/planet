# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Claude Code

- Claude Code local development has MCP access to extra resources like SonarCloud, GitHub issues, and discussions.
- **Serena Code Intelligence** is enabled for advanced code analysis and project understanding
- **ConPort Knowledge Management** is available for project knowledge organization and retrieval

## Coding Guidelines

- Always use a descriptive variable name
- Use expression bodies for single-line methods when possible
- Use PascalCase for public members and camelCase for private members
- Do not include unused using directives
- Use `var` for local variables when the type is obvious
- Use `async`/`await` for asynchronous methods
- Use `IEnumerable<T>` for return types when possible
- Use `IEnumerable<T>` for collections in method signatures
- Use `IServiceCollection` for dependency injection
- Use `ILogger<T>` for logging
- Use `IConfiguration` for configuration settings
- Use `IOptions<T>` for strongly typed configuration
- Use `IHttpClientFactory` for HTTP client creation
- Use `IActionResult` for controller actions

## Code Unit Tests guidelines

- Use xUnit v3 for unit tests
- Don't use '*Tests' suffix in the test namespace.
- Use `xunit.runner.json` to configure xUnit test settings
- Use `Fact` attribute for test methods
- Use `Theory` attribute for parameterized tests
- Use `Assert` methods for assertions

## Test Project Structure Guidelines

### Core Principle: Clear Separation
Maintain clear separation between core source code and test codebase to ensure clean architecture and maintainability throughout the entire repository.

### Test Platform Project
The test platform project (`<Module>.TestPlatform`) should contain:
- **Mocks**: All mock implementations and mock factories
- **Examples**: Sample data, test fixtures, and example implementations
- **Test Validation Logic**: Common validation helpers and assertion extensions
- **Test Comparison Logic**: Comparison utilities and result analysis tools

### Test Organization Rules
- **Core Source Projects**: Should contain only production code with minimal test utilities
- **Unit Test Projects**: Should contain only actual test methods and simple test setup
- **Platform Test Projects**: Should contain all reusable test infrastructure, utilities, and complex test logic
- **Integration Test Projects**: Should focus on integration scenarios, using platform utilities for setup

### Project Structure Example
This pattern should be applied consistently across all modules in the repository:

```
<ModuleName>/
├── src/
│   └── Root/                    # Core production code only
├── tests/
│   ├── Unit/                    # Unit tests using platform utilities
│   ├── Integration/             # Integration tests
│   └── Platform/                # Test platform with mocks, examples, validation logic
│       ├── Mocks/
│       ├── Examples/
│       ├── Extensions/          # Test comparison and validation logic
│       └── UseCase.cs
```

Examples of modules that should follow this pattern:
- **Graphics/**: Graphics processing and image handling
- **Spatial/**: Geospatial data handling and coordinate systems  
- **Portal/**: Blazor web application components
- **Engine/**: Console application and tile processing
- **Providers/**: External map service integrations
- **Protocols/**: Map service protocol implementations

### Benefits
- **Maintainability**: Test utilities are centralized and reusable across all modules
- **Clarity**: Clear distinction between production code and test infrastructure
- **Consistency**: Standardized test patterns across the entire solution
- **Efficiency**: Reduced code duplication in test projects

## Available MCP Tools

### Advanced Tools
- `mcp__sequential-thinking__sequentialthinking` - Sequential thinking process
- `mcp__fetch__fetch` - Fetch URLs with content extraction
- `mcp__ide__getDiagnostics` - Get IDE diagnostics

### Filesystem Operations
- `mcp__filesystem__read_file` - Read file contents with optional head/tail
- `mcp__filesystem__read_multiple_files` - Read multiple files simultaneously
- `mcp__filesystem__write_file` - Create or overwrite files
- `mcp__filesystem__edit_file` - Make line-based edits to files
- `mcp__filesystem__create_directory` - Create directories
- `mcp__filesystem__list_directory` - List directory contents
- `mcp__filesystem__list_directory_with_sizes` - List with file sizes
- `mcp__filesystem__directory_tree` - Get recursive directory tree
- `mcp__filesystem__move_file` - Move or rename files
- `mcp__filesystem__search_files` - Search for files by pattern
- `mcp__filesystem__get_file_info` - Get file metadata
- `mcp__filesystem__list_allowed_directories` - Show accessible directories

### GitHub Integration
- `mcp__github__get_file_contents` - Read repository files
- `mcp__github__create_or_update_file` - Create/update repository files
- `mcp__github__push_files` - Push multiple files in single commit
- `mcp__github__create_repository` - Create new repositories
- `mcp__github__fork_repository` - Fork repositories
- `mcp__github__create_branch` - Create new branches
- `mcp__github__list_commits` - List repository commits
- `mcp__github__list_issues` - List repository issues
- `mcp__github__create_issue` - Create new issues
- `mcp__github__update_issue` - Update existing issues
- `mcp__github__add_issue_comment` - Add comments to issues
- `mcp__github__get_issue` - Get specific issue details
- `mcp__github__list_pull_requests` - List pull requests
- `mcp__github__create_pull_request` - Create new pull requests
- `mcp__github__get_pull_request` - Get PR details
- `mcp__github__get_pull_request_files` - Get PR file changes
- `mcp__github__get_pull_request_status` - Get PR status
- `mcp__github__get_pull_request_comments` - Get PR comments
- `mcp__github__get_pull_request_reviews` - Get PR reviews
- `mcp__github__create_pull_request_review` - Create PR reviews
- `mcp__github__merge_pull_request` - Merge pull requests
- `mcp__github__update_pull_request_branch` - Update PR branch
- `mcp__github__search_repositories` - Search repositories
- `mcp__github__search_code` - Search code
- `mcp__github__search_issues` - Search issues
- `mcp__github__search_users` - Search users

### Container Management (Podman/Docker)
- `mcp__podman__container_list` - List containers
- `mcp__podman__container_run` - Run containers
- `mcp__podman__container_stop` - Stop containers
- `mcp__podman__container_remove` - Remove containers
- `mcp__podman__container_inspect` - Inspect container details
- `mcp__podman__container_logs` - View container logs
- `mcp__podman__image_list` - List images
- `mcp__podman__image_pull` - Pull images
- `mcp__podman__image_push` - Push images
- `mcp__podman__image_remove` - Remove images
- `mcp__podman__image_build` - Build images
- `mcp__podman__network_list` - List networks
- `mcp__podman__volume_list` - List volumes

### IDE Integration (JetBrains)
- `mcp__jetbrains__get_open_in_editor_file_text` - Get current file text
- `mcp__jetbrains__get_open_in_editor_file_path` - Get current file path
- `mcp__jetbrains__get_selected_in_editor_text` - Get selected text
- `mcp__jetbrains__replace_selected_text` - Replace selected text
- `mcp__jetbrains__replace_current_file_text` - Replace entire file
- `mcp__jetbrains__create_new_file_with_text` - Create new file
- `mcp__jetbrains__find_files_by_name_substring` - Find files by name
- `mcp__jetbrains__get_file_text_by_path` - Get file text by path
- `mcp__jetbrains__replace_file_text_by_path` - Replace file text
- `mcp__jetbrains__replace_specific_text` - Replace specific text
- `mcp__jetbrains__get_project_vcs_status` - Get VCS status
- `mcp__jetbrains__list_files_in_folder` - List folder contents
- `mcp__jetbrains__list_directory_tree_in_folder` - Get directory tree
- `mcp__jetbrains__search_in_files_content` - Search in file contents
- `mcp__jetbrains__run_configuration` - Run configurations
- `mcp__jetbrains__get_run_configurations` - Get available configurations
- `mcp__jetbrains__get_project_modules` - Get project modules
- `mcp__jetbrains__get_project_dependencies` - Get dependencies
- `mcp__jetbrains__toggle_debugger_breakpoint` - Toggle breakpoints
- `mcp__jetbrains__get_debugger_breakpoints` - Get breakpoints
- `mcp__jetbrains__open_file_in_editor` - Open files in editor
- `mcp__jetbrains__execute_action_by_id` - Execute IDE actions
- `mcp__jetbrains__get_current_file_errors` - Get file errors
- `mcp__jetbrains__reformat_current_file` - Reformat current file
- `mcp__jetbrains__get_project_problems` - Get project problems
- `mcp__jetbrains__execute_terminal_command` - Execute terminal commands

### Memory/Knowledge Management
- `mcp__memory__create_entities` - Create knowledge graph entities
- `mcp__memory__create_relations` - Create entity relations
- `mcp__memory__add_observations` - Add entity observations
- `mcp__memory__delete_entities` - Delete entities
- `mcp__memory__delete_observations` - Delete observations
- `mcp__memory__delete_relations` - Delete relations
- `mcp__memory__read_graph` - Read entire knowledge graph
- `mcp__memory__search_nodes` - Search graph nodes
- `mcp__memory__open_nodes` - Open specific nodes

### Serena Code Intelligence
- `mcp__serena__list_dir` - List directory contents with recursive option
- `mcp__serena__find_file` - Find files by name/mask pattern
- `mcp__serena__replace_regex` - Replace text using regular expressions
- `mcp__serena__search_for_pattern` - Flexible pattern search in codebase
- `mcp__serena__replace_symbol_body` - Replace entire symbol body
- `mcp__serena__insert_after_symbol` - Insert content after symbol
- `mcp__serena__insert_before_symbol` - Insert content before symbol
- `mcp__serena__write_memory` - Write project information to memory
- `mcp__serena__read_memory` - Read memory file contents
- `mcp__serena__list_memories` - List available memory files
- `mcp__serena__delete_memory` - Delete memory files
- `mcp__serena__activate_project` - Activate project by name/path
- `mcp__serena__remove_project` - Remove project from configuration
- `mcp__serena__switch_modes` - Switch between operation modes
- `mcp__serena__get_current_config` - Get current agent configuration
- `mcp__serena__check_onboarding_performed` - Check if onboarding completed
- `mcp__serena__onboarding` - Perform project onboarding
- `mcp__serena__think_about_collected_information` - Analyze collected information
- `mcp__serena__think_about_task_adherence` - Review task progress
- `mcp__serena__think_about_whether_you_are_done` - Evaluate task completion
- `mcp__serena__summarize_changes` - Summarize codebase changes
- `mcp__serena__prepare_for_new_conversation` - Prepare for new session
- `mcp__serena__initial_instructions` - Get initial project instructions
- `mcp__serena__jet_brains_find_symbol` - Find symbols/code entities
- `mcp__serena__jet_brains_find_referencing_symbols` - Find symbol references
- `mcp__serena__jet_brains_get_symbols_overview` - Get file symbol overview

### ConPort Knowledge Management
- `mcp__conport__get_product_context` - Retrieve project goals, features, and architecture
- `mcp__conport__update_product_context` - Update product context information
- `mcp__conport__get_active_context` - Get current working focus and recent changes
- `mcp__conport__update_active_context` - Update active context information
- `mcp__conport__log_decision` - Log architectural or implementation decisions
- `mcp__conport__get_decisions` - Retrieve logged decisions
- `mcp__conport__search_decisions_fts` - Full-text search across decisions
- `mcp__conport__log_progress` - Log progress entries or task status
- `mcp__conport__get_progress` - Retrieve progress entries
- `mcp__conport__update_progress` - Update existing progress entries
- `mcp__conport__delete_progress_by_id` - Delete progress entries
- `mcp__conport__log_system_pattern` - Log system/coding patterns
- `mcp__conport__get_system_patterns` - Retrieve system patterns
- `mcp__conport__log_custom_data` - Store custom key-value data
- `mcp__conport__get_custom_data` - Retrieve custom data
- `mcp__conport__delete_custom_data` - Delete custom data entries
- `mcp__conport__search_project_glossary_fts` - Search project glossary
- `mcp__conport__export_conport_to_markdown` - Export ConPort data to markdown
- `mcp__conport__import_markdown_to_conport` - Import markdown files to ConPort
- `mcp__conport__link_conport_items` - Create relationships between items
- `mcp__conport__get_linked_items` - Retrieve linked items
- `mcp__conport__semantic_search_conport` - Semantic search across ConPort data

## GitHub Repository

- GitHub integration is enabled for this repository via MCP `github` command.
- GitHub repo is at https://github.com/wangkanai/planet
- Work item backlogs are in the GitHub issues https://github.com/wangkanai/planet/issues
- Discussion board is at https://github.com/wangkanai/planet/discussions
- Project planning is at https://github.com/wangkanai/planet/projects
- CI/CD pipelines are configured in the GitHub Actions workflows https://github.com/wangkanai/planet/actions

## Code Quality

- SonarCube reposts are available via MCP `sonarqube` command.

## Serena Code Intelligence

Serena provides advanced code analysis and project understanding capabilities:

### Usage Guidelines
- **Always call `mcp__serena__initial_instructions`** before starting any coding task
- **Use `mcp__serena__check_onboarding_performed`** to verify project setup
- **Call `mcp__serena__think_about_collected_information`** after research phases
- **Use `mcp__serena__think_about_task_adherence`** before making code changes
- **Call `mcp__serena__think_about_whether_you_are_done`** when completing tasks
- **Use `mcp__serena__summarize_changes`** after completing coding work

### Code Analysis Tools
- **Symbol Search**: Use `mcp__serena__jet_brains_find_symbol` for finding classes, methods, properties
- **Reference Analysis**: Use `mcp__serena__jet_brains_find_referencing_symbols` to understand usage
- **File Overview**: Use `mcp__serena__jet_brains_get_symbols_overview` for file structure understanding
- **Pattern Search**: Use `mcp__serena__search_for_pattern` for flexible code searches
- **Regex Replace**: Use `mcp__serena__replace_regex` for complex text replacements

### Memory Management
- Use `mcp__serena__write_memory` to document important project insights
- Use `mcp__serena__read_memory` to access previously documented knowledge
- Keep memories focused and specific to enable efficient retrieval

## ConPort Knowledge Management

ConPort provides structured knowledge management for the Planet project:

### Project Context Management
- **Product Context**: Contains project vision, objectives, technical scope, and success metrics
- **Active Context**: Tracks current working focus, recent changes, and immediate priorities
- Use `mcp__conport__get_product_context` and `mcp__conport__get_active_context` to understand project state

### Decision Documentation
- **Log Architectural Decisions**: Use `mcp__conport__log_decision` for framework choices, design patterns, and technical decisions
- **Track Rationale**: Include reasoning, implementation details, and tags for categorization
- **Search Decisions**: Use `mcp__conport__search_decisions_fts` to find relevant past decisions

### Progress Tracking
- **Task Management**: Use `mcp__conport__log_progress` for tracking development tasks
- **Status Updates**: Update progress entries with `mcp__conport__update_progress`
- **Hierarchical Tasks**: Support parent-child task relationships for complex work

### System Patterns
- **Document Patterns**: Use `mcp__conport__log_system_pattern` for architectural patterns and best practices
- **Retrieve Patterns**: Use `mcp__conport__get_system_patterns` to find applicable patterns
- **Pattern Tagging**: Use tags to categorize patterns by domain (e.g., graphics, memory, performance)

### Custom Knowledge Storage
- **Flexible Data**: Use `mcp__conport__log_custom_data` for any structured project knowledge
- **Categories**: Organize data by categories like "GraphicsDocumentation", "PerformanceBenchmarks", "TechnicalSpecs"
- **Search Capabilities**: Full-text search across all custom data with `mcp__conport__search_custom_data_value_fts`

### Knowledge Relationships
- **Link Items**: Use `mcp__conport__link_conport_items` to create relationships between decisions, patterns, and progress
- **Knowledge Graph**: Build explicit connections between project knowledge items
- **Semantic Search**: Use `mcp__conport__semantic_search_conport` for intelligent knowledge retrieval

## Commands

### Build Commands
- `dotnet build -c Release -tl` - Build the entire solution in Release configuration
- `dotnet clean -c Release -tl` - Clean build artifacts
- `./build.ps1` - Full build script that includes clean, restore, build sequence

### Test Commands
- `dotnet test` - Run all tests across the solution
- `dotnet test --project <specific-test-project>` - Run tests for a specific project
- Tests use xUnit v3 framework with testing platform support enabled (check xunit.runner.json files in test projects)

### Benchmarking Commands
- `dotnet run --project Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks -c Release` - Run performance benchmarks for raster operations
- BenchmarkDotNet projects available for performance analysis and optimization

### Development Commands
- `dotnet restore` - Restore NuGet packages
- `dotnet run --project Portal/src/Server` - Run the Portal web application
- `dotnet run --project Engine/src/Console` - Run the Engine console application

### Database Commands (Portal)
- `./Portal/db.ps1 -add "<migration-name>"` - Add new Entity Framework migration
- `./Portal/db.ps1 -list` - List all migrations
- `./Portal/db.ps1 -remove` - Remove the last migration
- `./Portal/db.ps1 -update` - Update database to latest migration
- `./Portal/db.ps1 -clean` - Clean all migration files
- `./Portal/db.ps1 -reset` - Clean all migrations and create initial migration

### Engine Console Build
- `./Engine/src/Console/build.ps1` - Build and publish Engine console as 'tiler' executable

### Frontend Commands (Portal)
- `npm run build` - Build CSS from SCSS sources
- `npm run watch` - Watch and rebuild CSS on changes
- `npm run lib` - Copy library files to wwwroot
- `npm run clean` - Clean generated files
- `npm run deploy` - Full deployment build (clean, lib, build)

## Architecture

### Solution Structure
The Planet solution follows a modular architecture with these main components organized in separate libraries for clear separation of concerns:

**Portal** - Blazor Server/WASM hybrid web application with ASP.NET Core Identity
- Uses Clean Architecture patterns (Domain, Application, Infrastructure, Persistence layers)
- Client project for WebAssembly components
- Server project for Blazor Server hosting
- SQLite database with Entity Framework Core

**Engine** - Console application for map tile processing
- Domain layer for core business logic
- Console layer for CLI operations

**Spatial** - Geospatial data handling library (namespace: `Wangkanai.Spatial`)
- Root: Core coordinate systems (Geodetic, Mercator), map extent and tile calculations
- MbTiles: MBTiles format support with SQLite-based tile storage
- GeoPackages: GeoPackage format support for geospatial data containers
- GeoTiffs: GeoTIFF format support for georeferenced raster imagery
- ShapeFiles: Shapefile format support for vector geospatial data
- MtPkgs: Map tile package format support

**Providers** - External map service integrations
- Bing Maps provider
- Google Maps provider
- Each provider has corresponding test projects

**Graphics** - Graphics processing and image handling library (namespace: `Wangkanai.Graphics`)
- Abstractions: Core image processing interfaces and contracts
- Rasters: Raster image processing with multi-format support (TIFF, PNG, JPEG, WebP, AVIF, HEIF), metadata handling, and performance optimizations
- Vectors: Vector graphics processing and manipulation
- Includes comprehensive benchmarking and validation tools
- Format specifications documented in Graphics/GRAPHICS_FORMAT_SPECIFICATIONS.md

**Protocols** - Map service protocol implementations
- WMS (Web Map Service) protocol support
- Root protocol abstractions and utilities
- Protocol-specific implementations for serving map tiles

**Extensions** - Extension methods and utilities for the Planet ecosystem
- Datastore: Data storage extensions and utilities

### Key Technologies
- .NET 9.0 with nullable reference types enabled
- Blazor Server + WebAssembly (hybrid hosting model)
- ASP.NET Core Identity for authentication
- Entity Framework Core with SQLite and PostgreSQL support
- xUnit v3 for testing with testing platform support
- PowerShell scripts for automation
- Sass/SCSS for styling with Tabler UI framework
- NPM for frontend asset management
- Graphics processing with multi-format support (TIFF, PNG, JPEG, WebP, AVIF, HEIF) and performance benchmarking
- Geospatial data handling with multiple format support (MBTiles, GeoPackages, GeoTIFF, Shapefiles)
- Async disposal patterns for efficient resource management in graphics operations

### Database Context
- Portal uses `PlanetDbContext` with SQLite connection
- Identity system with custom `PlanetUser` and `PlanetRole` entities
- Migrations located in Portal/src/Persistence/Migrations

### Testing Strategy
- All major components have corresponding test projects
- Tests use xUnit v3 framework with testing platform support enabled
- Test projects follow naming convention: `<ProjectName>.Tests`
- BenchmarkDotNet projects for performance testing (e.g., Graphics.Rasters.Benchmarks)
- Unit test guidelines documented in module-specific GUIDELINES.md files

### Build Configuration
- Central package management via Directory.Packages.props
- Directory.Build.props defines common MSBuild properties
- Target framework: net9.0
- Solution uses .slnx format (new VS solution format)
- Frontend assets managed via NPM with Tabler UI components

### Project Structure Details
- **Engine/src/Console**: Console application for tile processing operations
- **Engine/src/Domain**: Engine domain logic
- **Extensions/Datastore/src**: Data storage extensions and utilities
- **Graphics/Abstractions/src**: Core graphics interfaces and abstractions
- **Graphics/Rasters/src/Root**: Raster image processing with multi-format support and metadata handling
- **Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks**: Performance benchmarking for raster operations
- **Graphics/Rasters/tests/Unit**: Unit tests with comprehensive testing guidelines
- **Graphics/Vectors/src/Root**: Vector graphics processing capabilities
- **Portal/src/Application**: Application layer with business logic and Identity configuration
- **Portal/src/Client**: Blazor WebAssembly client components
- **Portal/src/Domain**: Domain entities including custom Identity models
- **Portal/src/Infrastructure**: Infrastructure services and external integrations
- **Portal/src/Persistence**: Entity Framework data access with SQLite
- **Portal/src/Server**: Main Blazor Server application with hybrid WASM components
- **Protocols/src/Root**: Protocol abstractions and WMS implementations
- **Providers/src/Root**: Map service provider implementations
- **Spatial/src/GeoPackages**: GeoPackage format support
- **Spatial/src/GeoTiffs**: GeoTIFF format support with Graphics.Rasters integration
- **Spatial/src/MbTiles**: MBTiles format implementation
- **Spatial/src/MtPkgs**: Map tile package format support
- **Spatial/src/Root**: Core spatial data types and coordinate systems (namespace: `Wangkanai.Spatial`)
- **Spatial/src/ShapeFiles**: Shapefile format support
