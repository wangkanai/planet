// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Planet.Spatial;

/// <summary>Represents a GeoTIFF raster with geospatial metadata and TIFF specifications.</summary>
public class GeoTiffRaster : TiffRaster, IGeoTiffRaster
{
	/// <inheritdoc />
	public string? CoordinateReferenceSystem { get; set; }

	/// <inheritdoc />
	public MapExtent? Extent { get; set; }

	/// <inheritdoc />
	public double[]? GeoTransform { get; set; }

	/// <inheritdoc />
	public bool IsGeoreferenced =>
		!string.IsNullOrEmpty(CoordinateReferenceSystem) ||
		GeoTransform != null ||
		Extent != null;

	/// <inheritdoc />
	public double? PixelSizeX => GeoTransform?.Length >= 6 ? GeoTransform[1] : null;

	/// <inheritdoc />
	public double? PixelSizeY => GeoTransform?.Length >= 6 ? GeoTransform[5] : null;

	/// <summary>Initializes a new instance of the <see cref="GeoTiffRaster"/> class.</summary>
	public GeoTiffRaster() : base()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="GeoTiffRaster"/> class with specified dimensions.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	public GeoTiffRaster(int width, int height) : base(width, height)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="GeoTiffRaster"/> class with geospatial parameters.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="crs">The coordinate reference system.</param>
	/// <param name="extent">The geographic extent.</param>
	public GeoTiffRaster(int width, int height, string crs, MapExtent extent) : base(width, height)
	{
		CoordinateReferenceSystem = crs;
		Extent = extent;
	}
}
