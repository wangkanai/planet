// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Spatial.Coordinates;

/// <summary>Represents a geographic coordinate in the WGS84 datum (latitude/longitude)</summary>
public class Geodetic
{
	/// <summary>Gets or sets the latitude in degrees (-90 to 90)</summary>
	public double Latitude { get; set; }

	/// <summary>Gets or sets the longitude in degrees (-180 to 180)</summary>
	public double Longitude { get; set; }

	/// <summary>Creates a new Geodetic coordinate</summary>
	public Geodetic()
	{
		Latitude  = 0;
		Longitude = 0;
	}

	/// <summary>Creates a new Geodetic coordinate with specified latitude and longitude</summary>
	/// <param name="latitude">Latitude in degrees (-90 to 90)</param>
	/// <param name="longitude">Longitude in degrees (-180 to 180)</param>
	public Geodetic(double latitude, double longitude)
	{
		Latitude  = latitude;
		Longitude = longitude;
	}

	/// <summary>Converts this geographic coordinate to a Mercator projection (meters)</summary>
	/// <returns>A CoordinatePair representing the position in Mercator meters</returns>
	public Coordinate ToMeters()
	{
		var mercator = new Mercator();
		return mercator.LatLonToMeters(Longitude, Latitude);
	}

	/// <summary>Creates a Geodetic coordinate from Mercator meters</summary>
	/// <param name="meters">Coordinates in Mercator projection (meters)</param>
	/// <returns>A new Geodetic coordinate</returns>
	public static Geodetic FromMeters(Coordinate meters)
	{
		var mercator = new Mercator();
		var latLon   = mercator.MetersToLatLon(meters.X, meters.Y);
		return new Geodetic(latLon.Y, latLon.X);
	}

	/// <summary>Returns a string representation of the coordinate</summary>
	public override string ToString() => $"{Latitude:F5}°, {Longitude:F5}°";
}
