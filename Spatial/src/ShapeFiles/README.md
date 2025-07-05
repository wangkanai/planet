## Wangkanai Spatial ShapeFiles

**Namespace:** `Wangkanai.Spatial.ShapeFiles`

Shapefile format support for vector geospatial data, providing comprehensive functionality to read, write, and manipulate ESRI Shapefile format within the Wangkanai Spatial ecosystem.

## Features

- **Shapefile Standard**: Complete implementation of ESRI Shapefile format
- **Vector Data Support**: Handle point, line, and polygon geometries
- **Attribute Management**: Support for .dbf attribute tables
- **Spatial Indexing**: Efficient spatial queries with .shx index files
- **Projection Support**: Coordinate reference system handling with .prj files
- **Multi-File Format**: Manages the complete Shapefile component set

## Shapefile Components

The Shapefile format consists of multiple related files:

- **.shp** - Main geometry file containing shape records
- **.shx** - Index file for efficient geometry access
- **.dbf** - Attribute table in dBASE format
- **.prj** - Projection file with coordinate reference system
- **.cpg** - Code page file for character encoding
- **.sbn/.sbx** - Spatial index files (optional)

## Supported Geometry Types

- **Point** - Single coordinate locations
- **Polyline** - Connected line segments
- **Polygon** - Closed area geometries
- **MultiPoint** - Collection of point geometries
- **PointZ** - 3D point coordinates
- **PolylineZ** - 3D line geometries
- **PolygonZ** - 3D polygon geometries

## Usage

```csharp
using Wangkanai.Spatial.ShapeFiles;

// Work with Shapefile data
// Implementation classes will be available here
```

## Standards Compliance

This implementation follows:
- **ESRI Shapefile** technical specification
- **dBASE** file format for attributes
- **OGC Simple Features** for geometry handling
- **EPSG** coordinate reference systems

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations
- **System.Text.Encoding** - Character encoding support
