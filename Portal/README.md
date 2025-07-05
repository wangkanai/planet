## Wangkanai Planet Portal

**Namespace:** `Wangkanai.Planet.Portal`

A modern web application providing a user-friendly interface for viewing and managing map tiles. Built with Blazor Server/WASM hybrid architecture, featuring ASP.NET Core Identity for authentication and a comprehensive administration portal for geospatial data management.

## Features

- **Hybrid Architecture**: Blazor Server + WebAssembly for optimal performance and user experience
- **Authentication System**: ASP.NET Core Identity with custom user and role management
- **Map Visualization**: Interactive map viewing with tile layer management
- **Administration Portal**: Comprehensive admin interface for map tile management
- **Responsive Design**: Modern UI with Tabler framework and SCSS styling
- **Real-Time Updates**: Server-side rendering with client-side interactivity
- **Multi-Database Support**: SQLite and PostgreSQL database support

## Architecture

The Portal follows Clean Architecture principles:

```
Portal.Server (Blazor Server Host)
    ↓
Portal.Client (Blazor WASM Components)
    ↓
Portal.Application (Business Logic)
    ↓
Portal.Infrastructure (External Services)
    ↓
Portal.Persistence (Data Access)
    ↓
Portal.Domain (Core Entities)
```

## Components

### Server Application
- **Blazor Server Hosting**: Main application host with hybrid WASM support
- **Authentication**: ASP.NET Core Identity integration
- **API Endpoints**: RESTful APIs for map data and user management
- **Real-Time Communication**: SignalR for live updates

### Client Application  
- **Blazor WebAssembly**: Client-side components for interactive features
- **Progressive Web App**: PWA capabilities for offline support
- **Interactive Maps**: Client-side map rendering and interaction

### Application Layer
- **Business Logic**: Core application services and workflows
- **Identity Configuration**: Custom user and role management
- **Validation**: Input validation and business rule enforcement
- **Commands/Queries**: CQRS pattern for data operations

### Infrastructure Layer
- **External Integrations**: Map service provider integrations
- **File Storage**: Map tile and asset storage management
- **Logging**: Structured logging and monitoring
- **Caching**: Performance optimization through caching

### Persistence Layer
- **Entity Framework Core**: Database access with Code First approach
- **Identity Storage**: User and role data persistence
- **Migration Support**: Database schema versioning
- **Multi-Database**: SQLite for development, PostgreSQL for production

### Domain Layer
- **Core Entities**: User, role, and map-related domain models
- **Value Objects**: Immutable data structures
- **Domain Services**: Core business logic and rules

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js (for frontend asset management)
- Database (SQLite for development, PostgreSQL for production)

### Development Setup

1. **Restore Dependencies**:
   ```bash
   dotnet restore
   npm install
   ```

2. **Database Setup**:
   ```bash
   ./Portal/db.ps1 -update
   ```

3. **Build Assets**:
   ```bash
   npm run deploy
   ```

4. **Run Application**:
   ```bash
   dotnet run --project Portal/src/Server
   ```

### Database Commands

```bash
# Add new migration
./Portal/db.ps1 -add "MigrationName"

# Update database
./Portal/db.ps1 -update

# List migrations
./Portal/db.ps1 -list

# Remove last migration
./Portal/db.ps1 -remove

# Reset database
./Portal/db.ps1 -reset
```

### Frontend Commands

```bash
# Build CSS from SCSS
npm run build

# Watch for changes
npm run watch

# Copy library files
npm run lib

# Full deployment build
npm run deploy

# Clean generated files
npm run clean
```

## User Management

### Custom Identity
- **PlanetUser**: Extended user entity with custom properties
- **PlanetRole**: Custom role system with permissions
- **PlanetPermissions**: Granular permission system
- **PlanetTheme**: User theme preferences

### Authentication Features
- **Registration/Login**: Standard authentication flows
- **Two-Factor Authentication**: Enhanced security options
- **External Providers**: Social media login integration
- **Account Management**: Profile and preference management

## UI Framework

### Tabler Integration
- **Modern Design**: Professional admin dashboard styling
- **Responsive Layout**: Mobile-first responsive design
- **Component Library**: Rich set of UI components
- **Icon System**: Comprehensive icon library
- **Theme Support**: Dark/light theme switching

### SCSS Architecture
```
scss/
├── base/          # Base styles and resets
├── configs/       # SCSS configuration and variables
└── _index.scss    # Main entry point
```

## Map Features

- **Interactive Maps**: Pan, zoom, and layer management
- **Tile Layers**: Support for multiple map tile sources
- **Overlay Management**: Vector overlays and annotations
- **Export Capabilities**: Map export in various formats
- **Measurement Tools**: Distance and area calculations

## Administration

- **User Management**: Create, edit, and manage users
- **Role Administration**: Configure roles and permissions
- **Map Management**: Upload and organize map tiles
- **System Monitoring**: Performance and usage analytics
- **Configuration**: System settings and preferences

## Dependencies

- **.NET 9.0** - Target framework
- **Blazor Server/WASM** - Web framework
- **ASP.NET Core Identity** - Authentication system
- **Entity Framework Core** - Data access
- **Tabler** - UI framework
- **SCSS** - Styling
- **NPM** - Frontend package management

## Testing

Comprehensive testing includes:
- Unit tests for business logic
- Integration tests for web APIs
- UI tests for Blazor components
- Authentication flow testing
- Database operation testing
