// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Spatial;

/// <summary>Represents a GeoTIFF image that extends TIFF with geospatial capabilities.</summary>
public interface IGeoTiffRaster : ITiffRaster
{
	/// <summary>Gets the coordinate reference system (CRS) of the geospatial data.</summary>
	string? CoordinateReferenceSystem { get; }

	/// <summary>Gets the geographic extent of the image.</summary>
	MapExtent? Extent { get; }

	/// <summary>Gets the transformation matrix for converting pixel coordinates to geographic coordinates.</summary>
	double[]? GeoTransform { get; }

	/// <summary>Gets a value indicating whether the image contains geospatial metadata.</summary>
	bool IsGeoreferenced { get; }

	/// <summary>Gets the pixel size in geographic units.</summary>
	double? PixelSizeX { get; }

	/// <summary>Gets the pixel size in geographic units.</summary>
	double? PixelSizeY { get; }
}
