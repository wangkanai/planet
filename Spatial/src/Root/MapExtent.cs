// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

/// <summary>
/// Represents the geographical extent of a map canvas in Spherical Mercator projection (EPSG:3857).
/// Boundaries are expressed in meters.
/// </summary>
public class MapExtent
{
	/// <summary>Maximum extent of the Spherical Mercator projection in meters</summary>
	public const double Max = 6378137;

	/// <summary>Creates a new map extent with default values (full Spherical Mercator extent)</summary>
	public MapExtent()
	{
		North = Math.PI * Max;
		East  = Math.PI * Max;
		South = Math.PI * -Max;
		West  = Math.PI * -Max;
	}

	/// <summary>Creates a new map extent with specified boundaries in meters</summary>
	/// <param name="north">Northern boundary (max Y in meters)</param>
	/// <param name="east">Eastern boundary (max X in meters)</param>
	/// <param name="south">Southern boundary (min Y in meters)</param>
	/// <param name="west">Western boundary (min X in meters)</param>
	public MapExtent(double north, double east, double south, double west)
	{
		North = north;
		East  = east;
		South = south;
		West  = west;
	}

	/// <summary>Northern boundary of the map extent (maximum Y in meters)</summary>
	public double North { get; set; }

	/// <summary>Eastern boundary of the map extent (maximum X in meters)</summary>
	public double East { get; set; }

	/// <summary>Southern boundary of the map extent (minimum Y in meters)</summary>
	public double South { get; set; }

	/// <summary>Western boundary of the map extent (minimum X in meters)</summary>
	public double West { get; set; }

	/// <summary>Width of the extent in meters</summary>
	public double Width => East - West;

	/// <summary>Height of the extent in meters</summary>
	public double Height => North - South;

	/// <summary>Gets the center point of the map extent</summary>
	/// <returns>A CoordinatePair representing the center point in meters</returns>
	public Coordinate GetCenter()
		=> new((East + West) / 2.0, (North + South) / 2.0);

	/// <summary>Checks if a coordinate pair is within this map extent</summary>
	/// <param name="point">The coordinate pair to check</param>
	/// <returns>True if the point is within the extent, false otherwise</returns>
	public bool Contains(Coordinate point)
		=> point.X >= West && point.X <= East &&
		   point.Y >= South && point.Y <= North;

	/// <summary>Validates if the extent is within the valid range for Spherical Mercator</summary>
	/// <returns>True if valid, false otherwise</returns>
	public bool IsValid()
		=> West >= -Max && East <= Max &&
		   South >= -Max && North <= Max &&
		   West < East && South < North;

	/// <summary>Returns a string representation of the map extent in meters</summary>
	public override string ToString()
		=> $"N:{North}m, E:{East}m, S:{South}m, W:{West}m";
}
