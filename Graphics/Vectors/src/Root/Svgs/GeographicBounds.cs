// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>
/// Represents a geographic bounding box defined by minimum and maximum latitude and longitude values.
/// Used for coordinate system transformations and geospatial SVG operations.
/// </summary>
public class GeographicBounds
{
	/// <summary>Gets or sets the minimum latitude (southern boundary).</summary>
	public double MinLatitude { get; set; }

	/// <summary>Gets or sets the maximum latitude (northern boundary).</summary>
	public double MaxLatitude { get; set; }

	/// <summary>Gets or sets the minimum longitude (western boundary).</summary>
	public double MinLongitude { get; set; }

	/// <summary>Gets or sets the maximum longitude (eastern boundary).</summary>
	public double MaxLongitude { get; set; }

	/// <summary>Initializes a new instance of the GeographicBounds class.</summary>
	public GeographicBounds() { }

	/// <summary>Initializes a new instance of the GeographicBounds class with specified bounds.</summary>
	/// <param name="minLatitude">The minimum latitude (southern boundary).</param>
	/// <param name="maxLatitude">The maximum latitude (northern boundary).</param>
	/// <param name="minLongitude">The minimum longitude (western boundary).</param>
	/// <param name="maxLongitude">The maximum longitude (eastern boundary).</param>
	public GeographicBounds(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
	{
		MinLatitude  = minLatitude;
		MaxLatitude  = maxLatitude;
		MinLongitude = minLongitude;
		MaxLongitude = maxLongitude;
	}

	/// <summary>Gets the width of the bounding box in degrees.</summary>
	public double Width
		=> MaxLongitude - MinLongitude;

	/// <summary>Gets the height of the bounding box in degrees.</summary>
	public double Height
		=> MaxLatitude - MinLatitude;

	/// <summary>Gets the center point of the bounding box.</summary>
	public (double Latitude, double Longitude) Center
		=> ((MinLatitude + MaxLatitude) / 2, (MinLongitude + MaxLongitude) / 2);

	/// <summary>Determines whether the specified point is within this bounding box.</summary>
	/// <param name="latitude">The latitude to check.</param>
	/// <param name="longitude">The longitude to check.</param>
	/// <returns>True if the point is within the bounds; otherwise, false.</returns>
	public bool Contains(double latitude, double longitude)
		=> latitude >= MinLatitude && latitude <= MaxLatitude &&
		   longitude >= MinLongitude && longitude <= MaxLongitude;

	/// <summary>Expands the bounding box to include the specified point.</summary>
	/// <param name="latitude">The latitude of the point to include.</param>
	/// <param name="longitude">The longitude of the point to include.</param>
	public void Expand(double latitude, double longitude)
	{
		MinLatitude  = Math.Min(MinLatitude, latitude);
		MaxLatitude  = Math.Max(MaxLatitude, latitude);
		MinLongitude = Math.Min(MinLongitude, longitude);
		MaxLongitude = Math.Max(MaxLongitude, longitude);
	}

	/// <summary>Validates that the bounding box is correctly formed.</summary>
	/// <returns>True if the bounds are valid; otherwise, false.</returns>
	public bool IsValid()
		=> MinLatitude <= MaxLatitude &&
		   MinLongitude <= MaxLongitude &&
		   MinLatitude >= -90 && MaxLatitude <= 90 &&
		   MinLongitude >= -180 && MaxLongitude <= 180;

	/// <summary>Returns a string representation of the geographic bounds.</summary>
	/// <returns>A string in the format "MinLat,MinLon,MaxLat,MaxLon".</returns>
	public override string ToString()
		=> $"{MinLatitude:F5},{MinLongitude:F5},{MaxLatitude:F5},{MaxLongitude:F5}";

	/// <summary>Creates a GeographicBounds from a comma-separated string.</summary>
	/// <param name="boundsString">A string in the format "MinLat,MinLon,MaxLat,MaxLon".</param>
	/// <returns>A new GeographicBounds instance.</returns>
	/// <exception cref="ArgumentException">Thrown when the string format is invalid.</exception>
	public static GeographicBounds Parse(string boundsString)
	{
		if (string.IsNullOrWhiteSpace(boundsString))
			throw new ArgumentException("Bounds string cannot be null or empty.", nameof(boundsString));

		var parts = boundsString.Split(',');
		if (parts.Length != 4)
			throw new ArgumentException("Bounds string must contain exactly 4 comma-separated values.", nameof(boundsString));

		if (!double.TryParse(parts[0], out var minLat) ||
		    !double.TryParse(parts[1], out var minLon) ||
		    !double.TryParse(parts[2], out var maxLat) ||
		    !double.TryParse(parts[3], out var maxLon))
		{
			throw new ArgumentException("All parts of the bounds string must be valid numbers.", nameof(boundsString));
		}

		return new GeographicBounds(minLat, maxLat, minLon, maxLon);
	}

	/// <summary>World bounds covering the entire Earth.</summary>
	public static GeographicBounds World
		=> new(-90, 90, -180, 180);

	/// <summary>Web Mercator bounds (the typical bounds for web mapping).</summary>
	public static GeographicBounds WebMercator
		=> new(-85.0511, 85.0511, -180, 180);
}
