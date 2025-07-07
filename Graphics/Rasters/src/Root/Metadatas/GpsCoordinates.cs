// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Represents GPS coordinates for geotagged images.
/// </summary>
public class GpsCoordinates
{
	/// <summary>Gets or sets the latitude in decimal degrees.</summary>
	public double Latitude { get; set; }

	/// <summary>Gets or sets the longitude in decimal degrees.</summary>
	public double Longitude { get; set; }

	/// <summary>Gets or sets the altitude in meters.</summary>
	public double? Altitude { get; set; }

	/// <summary>Gets or sets the GPS timestamp.</summary>
	public DateTimeOffset? Timestamp { get; set; }

	/// <summary>Creates a copy of the GPS coordinates.</summary>
	public GpsCoordinates Clone()
		=> new()
		   {
			   Latitude  = Latitude,
			   Longitude = Longitude,
			   Altitude  = Altitude,
			   Timestamp = Timestamp
		   };
}
