# Wangkanai Planet 🌎

Planetary distributed map service server cross-platform built with C#.NET.
Lightweight and easy to use to serve raster and vector map tiles through the browser.
Also supports map service protocols like WMTS, WMS, and XYZ Tiles.

[![NuGet Version](https://img.shields.io/nuget/v/wangkanai.planet)](https://www.nuget.org/packages/wangkanai.planet)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/wangkanai.planet)](https://www.nuget.org/packages/wangkanai.planet)

[![planet-ci](https://github.com/wangkanai/planet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/wangkanai/caster/actions/workflows/dotnet.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=wangkanai_planet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=wangkanai_planet)

[![Open Collective](https://img.shields.io/badge/open%20collective-support%20me-3385FF.svg)](https://opencollective.com/wangkanai)
[![Patreon](https://img.shields.io/badge/patreon-support%20me-d9643a.svg)](https://www.patreon.com/wangkanai)
[![GitHub](https://img.shields.io/github/license/wangkanai/caster)](https://github.com/wangkanai/caster/blob/main/LICENSE)

## Features 🌟

- Engine for rendering raster geotiff to indexable map tiles
- Portal for viewing raster and vector map tiles
- Administration portal for managing map tiles

## Solution Structure 🏗️

- **[Wangkanai.Planet.Portal](Portal)** - Web portal for viewing map tiles
- **[Wangkanai.Planet.Engine](Engine)** - Engine for rendering and processing map tiles
- **[Wangkanai.Planet.Spatial](Spatial)** - Library for rendering raster GeoTiff to map tiles
- **[Wangkanai.Planet.Providers](Providers)** - Providers for different map storage standards
- **[Wangkanai.Planet.Extensions](Extensions)** - Extensions for integrating with various map services and protocols
- **[Wangkanai.Planet.Protocols](Protocols)** - Protocols for serving map tiles through different services

## Map storage standards ️🗺️

- MBTiles    (SQLite) - Standardized SQLite database for storing raster and vector tiles
- MTPKG      (SQLite) - Multi-layer package for storing raster and vector tiles
- GeoPackage (SQLite) - OGC standard for storing raster and vector data in a single file
- GeoTiff    (TIFF)   - Standardized format for georeferenced raster imagery
- TileJSON   (JSON)   - Standardized JSON format for tile metadata and access

## Map services protocols 📡

- Web (Static Raster Tiles)    - Simple HTTP-based tile access
- TMS (Tile Map Service)       - Simple HTTP-based tile access
- WMTS (Web Map Tile Service)  - OGC standard for tile services
- WMS  (Web Map Service)       - OGC standard for map services
- WMSC (Web Map Service Cache) - OGC standard for cached map services
- XYZ Tiles (TileJSON)         - Standardized tile access using XYZ coordinates

## Supported tiles formats 🗜️

- Raster .mbtiles
- Vector .mbtiles
- Raster .geopackage
- Raster and vector .mtpkg
- Quantized mesh terrain (3D) .geopackage
- Vector tiles from PostGIS geometries

## Supported raster formats 🗺️

- GeoTIFF (raster imagery)
- JPEG (compressed raster imagery)
- JPEG2000 (compressed raster imagery)
- PNG (lossless raster imagery)
- WebP (compressed raster imagery)
- HEIF (compressed raster imagery)

## Supported vector formats 📊

- GeoJSON (vector data in JSON format)
- TopoJSON (vector data in JSON format with topology)
- Shapefile (vector data in ESRI format)
- KML (Keyhole Markup Language for vector data)
- GML (Geography Markup Language for vector data)
- GPX (GPS Exchange Format for vector data)
- WKT (Well-Known Text for vector geometries)
- WKB (Well-Known Binary for vector geometries)
- PostGIS (vector data in PostgreSQL/PostGIS format)

## Supported CRS 🏁

- Complete EPSG database + custom defined SRS via Proj4. Over 6000 systems world-wide
- Base map mercator only

## Desktop viewers 🖥️

- [QGIS](https://www.qgis.org/en/site/)
- [ArcGIS for Desktop](https://www.esri.com/en-us/arcgis/products/arcgis-desktop/overview)
- [Google Earth](https://www.google.com/earth/)
- [Tableau](https://www.tableau.com/)
- any viewer supporting WMTS

## Mobile viewers 📱

- Google Maps SDK for iOS
- Google Maps SDK for Android
- MapLibre iOS SDK
- MapLibre Android SDK
- Apple MapKit
- RouteMe
- OSMDroid
- any viewer supporting WMTS, or TileJSON

## Sponsoring ❤️

If you like this project and want to support its development,
please consider sponsoring it through [Open Collective](https://opencollective.com/wangkanai)
or [Patreon](https://www.patreon.com/wangkanai).
Your support helps keep the project alive and thriving!

## Contributing 🤝

We welcome contributions to Wangkanai Planet!
If you have ideas, suggestions, or improvements, please feel free to open an issue or submit a pull request.
