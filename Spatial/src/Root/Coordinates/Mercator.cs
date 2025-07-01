// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial.Coordinates;

/// <summary>
/// Represents a Mercator coordinate system, commonly used in mapping applications,
/// based on the Spherical Mercator projection (EPSG:3857).
/// Provides properties and methods to calculate coordinates and conversions.
/// </summary>
public class Mercator
{
	/// <summary>Gets the size of the Mercator coordinate.</summary>
	public int TileSize { get; private set; }

	/// <summary>Gets the resolution (meters / pixel) of the Mercator coordinate. </summary>
	public double InitialResolution { get; private set; }

	/// <summary>Gets the maximum extent of the Mercator coordinate.</summary>
	public double OriginShift { get; private set; }

	/// <summary>
	/// Represents a Mercator coordinate system, commonly used in mapping applications,
	/// that is based on the Spherical Mercator projection (EPSG:3857).
	/// Provides properties to calculate properties like resolution and size.
	/// </summary>
	public Mercator(int size = 512)
	{
		TileSize          = size;
		InitialResolution = 2 * Math.PI * MapExtent.Max / TileSize;
		OriginShift       = 2 * Math.PI * MapExtent.Max / 2.0;
	}

	/// <summary>
	/// Converts Mercator meters coordinates to latitude/longitude in degrees
	/// </summary>
	/// <param name="mx">Mercator X coordinate (in meters)</param>
	/// <param name="my">Mercator Y coordinate (in meters)</param>
	/// <returns>A CoordinatePair containing longitude (X) and latitude (Y) in degrees</returns>
	public Coordinate MetersToLatLon(double mx, double my)
	{
		// Convert from Spherical Mercator to longitude
		double lon = (mx / OriginShift) * 180.0;

		// Convert from Spherical Mercator to latitude
		double lat = (my / OriginShift) * 180.0;
		lat = 180.0 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);

		return new Coordinate(lon, lat);
	}

	/// <summary>
	/// Converts latitude/longitude coordinates (in degrees) to Mercator meters
	/// </summary>
	/// <param name="lon">Longitude in degrees</param>
	/// <param name="lat">Latitude in degrees</param>
	/// <returns>A CoordinatePair containing X and Y coordinates in Spherical Mercator (meters)</returns>
	public Coordinate LatLonToMeters(double lon, double lat)
	{
		// Clamp latitude to the valid range (-85.05112878, 85.05112878)
		// This prevents infinite values near the poles
		lat = Math.Max(Math.Min(lat, 85.05112878), -85.05112878);

		// Convert longitude to Spherical Mercator
		double mx = lon * OriginShift / 180.0;

		// Convert latitude to Spherical Mercator
		double my = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) / (Math.PI / 180.0);
		my = my * OriginShift / 180.0;

		return new Coordinate(mx, my);
	}

	/// <summary>Converts pixel coordinates to meters in the Mercator coordinate system.</summary>
	/// <param name="px">The x-coordinate in pixels.</param>
	/// <param name="py">The y-coordinate in pixels.</param>
	/// <param name="zoom">The zoom level.</param>
	/// <returns>A Coordinate object representing the meter coordinates.</returns>
	public Coordinate PixelToMeters(double px, double py, int zoom)
	{
		var coordinate = new Coordinate();
		try
		{
			var resolution = Resolution(zoom);
			coordinate.X = px * resolution - OriginShift;
			coordinate.Y = py * resolution - OriginShift;
			return coordinate;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	/// <summary>Converts Mercator meters coordinates to pixel coordinates.</summary>
	/// <param name="mx">The x-coordinate in Mercator meters.</param>
	/// <param name="my">The y-coordinate in Mercator meters.</param>
	/// <param name="zoom">The zoom level.</param>
	/// <returns>A Coordinate containing the pixel x and y coordinates.</returns>
	public Coordinate MetersToPixels(double mx, double my, int zoom)
	{
		var coordinate = new Coordinate();
		try
		{
			var resolution = Resolution(zoom);
			coordinate.X = (mx + OriginShift) / resolution;
			coordinate.Y = (my + OriginShift) / resolution;
			return coordinate;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	/// <summary>Gets the resolution at a specified zoom level.</summary>
	/// <param name="zoom">The zoom level.</param>
	/// <returns>The resolution at the specified zoom level.</returns>
	private double Resolution(int zoom)
		=> InitialResolution / (1 << zoom);
}
