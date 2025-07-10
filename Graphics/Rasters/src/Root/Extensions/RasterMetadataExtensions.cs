// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Extension methods for IRasterMetadata interface providing raster-specific utility functions.
/// </summary>
public static class RasterMetadataExtensions
{
	/// <summary>
	/// Determines if the metadata has GPS coordinates.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if both latitude and longitude are set.</returns>
	public static bool HasGpsCoordinates(this IRasterMetadata metadata)
	{
		return metadata.GpsLatitude.HasValue && metadata.GpsLongitude.HasValue;
	}

	/// <summary>
	/// Determines if the GPS coordinates are valid.
	/// </summary>
	/// <param name="metadata">The raster metadata to validate.</param>
	/// <returns>True if GPS coordinates are within valid ranges.</returns>
	public static bool IsValidGpsCoordinates(this IRasterMetadata metadata)
	{
		if (!metadata.HasGpsCoordinates())
			return true; // Null coordinates are considered valid

		var lat = metadata.GpsLatitude!.Value;
		var lng = metadata.GpsLongitude!.Value;

		return lat >= -90.0 && lat <= 90.0 && lng >= -180.0 && lng <= 180.0;
	}

	/// <summary>
	/// Gets the GPS coordinates as a tuple.
	/// </summary>
	/// <param name="metadata">The raster metadata to read from.</param>
	/// <returns>A tuple containing latitude and longitude, or null if not set.</returns>
	public static (double latitude, double longitude)? GetGpsCoordinates(this IRasterMetadata metadata)
	{
		if (!metadata.HasGpsCoordinates())
			return null;

		return (metadata.GpsLatitude!.Value, metadata.GpsLongitude!.Value);
	}

	/// <summary>
	/// Sets the GPS coordinates.
	/// </summary>
	/// <param name="metadata">The raster metadata to modify.</param>
	/// <param name="latitude">Latitude in decimal degrees.</param>
	/// <param name="longitude">Longitude in decimal degrees.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are out of valid range.</exception>
	public static void SetGpsCoordinates(this IRasterMetadata metadata, double latitude, double longitude)
	{
		if (latitude < -90.0 || latitude > 90.0)
			throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

		if (longitude < -180.0 || longitude > 180.0)
			throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

		metadata.GpsLatitude = latitude;
		metadata.GpsLongitude = longitude;
	}

	/// <summary>
	/// Clears the GPS coordinates.
	/// </summary>
	/// <param name="metadata">The raster metadata to modify.</param>
	public static void ClearGpsCoordinates(this IRasterMetadata metadata)
	{
		metadata.GpsLatitude = null;
		metadata.GpsLongitude = null;
	}

	/// <summary>
	/// Calculates the distance from the image location to a specified point.
	/// </summary>
	/// <param name="metadata">The raster metadata with GPS coordinates.</param>
	/// <param name="latitude">Target latitude.</param>
	/// <param name="longitude">Target longitude.</param>
	/// <returns>Distance in kilometers, or null if GPS coordinates are not available.</returns>
	public static double? GetDistanceFrom(this IRasterMetadata metadata, double latitude, double longitude)
	{
		if (!metadata.HasGpsCoordinates())
			return null;

		return CalculateHaversineDistance(
			metadata.GpsLatitude!.Value, metadata.GpsLongitude!.Value,
			latitude, longitude);
	}

	/// <summary>
	/// Determines if the metadata has resolution information.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if both X and Y resolution are set.</returns>
	public static bool HasResolution(this IRasterMetadata metadata)
	{
		return metadata.XResolution.HasValue && metadata.YResolution.HasValue;
	}

	/// <summary>
	/// Determines if the resolution values are valid.
	/// </summary>
	/// <param name="metadata">The raster metadata to validate.</param>
	/// <returns>True if resolution values are positive and reasonable.</returns>
	public static bool IsValidResolution(this IRasterMetadata metadata)
	{
		if (!metadata.HasResolution())
			return true; // Null resolution is considered valid

		var xRes = metadata.XResolution!.Value;
		var yRes = metadata.YResolution!.Value;

		return xRes > 0 && yRes > 0 && xRes <= 10000 && yRes <= 10000;
	}

	/// <summary>
	/// Gets the resolution as a tuple.
	/// </summary>
	/// <param name="metadata">The raster metadata to read from.</param>
	/// <returns>A tuple containing X and Y resolution, or null if not set.</returns>
	public static (double x, double y)? GetResolution(this IRasterMetadata metadata)
	{
		if (!metadata.HasResolution())
			return null;

		return (metadata.XResolution!.Value, metadata.YResolution!.Value);
	}

	/// <summary>
	/// Sets the resolution values.
	/// </summary>
	/// <param name="metadata">The raster metadata to modify.</param>
	/// <param name="x">X resolution.</param>
	/// <param name="y">Y resolution.</param>
	/// <param name="unit">Resolution unit (2 = inches, 3 = centimeters).</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if resolution values are invalid.</exception>
	public static void SetResolution(this IRasterMetadata metadata, double x, double y, int unit = 2)
	{
		if (x <= 0)
			throw new ArgumentOutOfRangeException(nameof(x), "X resolution must be positive.");

		if (y <= 0)
			throw new ArgumentOutOfRangeException(nameof(y), "Y resolution must be positive.");

		metadata.XResolution = x;
		metadata.YResolution = y;
		metadata.ResolutionUnit = unit;
	}

	/// <summary>
	/// Gets the resolution in DPI (dots per inch).
	/// </summary>
	/// <param name="metadata">The raster metadata to read from.</param>
	/// <returns>Average resolution in DPI, or null if not available.</returns>
	public static double? GetResolutionInDpi(this IRasterMetadata metadata)
	{
		if (!metadata.HasResolution())
			return null;

		var xRes = metadata.XResolution!.Value;
		var yRes = metadata.YResolution!.Value;

		// Convert to DPI if unit is centimeters
		if (metadata.ResolutionUnit == 3)
		{
			xRes *= 2.54; // cm to inches
			yRes *= 2.54;
		}

		return (xRes + yRes) / 2.0;
	}

	/// <summary>
	/// Gets the resolution in PPI (pixels per inch).
	/// </summary>
	/// <param name="metadata">The raster metadata to read from.</param>
	/// <returns>Average resolution in PPI, or null if not available.</returns>
	public static double? GetResolutionInPpi(this IRasterMetadata metadata) => metadata.GetResolutionInDpi();

	/// <summary>
	/// Determines if the metadata has an ICC color profile.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if ICC profile data is present.</returns>
	public static bool HasIccProfile(this IRasterMetadata metadata)
	{
		return metadata.IccProfile != null && metadata.IccProfile.Length > 0;
	}

	/// <summary>
	/// Determines if the metadata has EXIF data.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if EXIF data is present.</returns>
	public static bool HasExifData(this IRasterMetadata metadata)
	{
		return metadata.ExifData != null && metadata.ExifData.Length > 0;
	}

	/// <summary>
	/// Determines if the metadata has XMP data.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if XMP data is present.</returns>
	public static bool HasXmpData(this IRasterMetadata metadata)
	{
		return !string.IsNullOrWhiteSpace(metadata.XmpData);
	}

	/// <summary>
	/// Gets the size of the ICC profile in bytes.
	/// </summary>
	/// <param name="metadata">The raster metadata to measure.</param>
	/// <returns>ICC profile size in bytes, or 0 if not present.</returns>
	public static long GetIccProfileSize(this IRasterMetadata metadata)
	{
		return metadata.IccProfile?.Length ?? 0;
	}

	/// <summary>
	/// Gets the size of the EXIF data in bytes.
	/// </summary>
	/// <param name="metadata">The raster metadata to measure.</param>
	/// <returns>EXIF data size in bytes, or 0 if not present.</returns>
	public static long GetExifDataSize(this IRasterMetadata metadata)
	{
		return metadata.ExifData?.Length ?? 0;
	}

	/// <summary>
	/// Gets a description of the color space.
	/// </summary>
	/// <param name="metadata">The raster metadata to describe.</param>
	/// <returns>Color space description string.</returns>
	public static string GetColorSpaceDescription(this IRasterMetadata metadata)
	{
		return metadata.ColorSpace switch
		{
			1 => "sRGB",
			2 => "Adobe RGB",
			3 => "Wide Gamut RGB",
			65535 => "Uncalibrated",
			_ => metadata.ColorSpace?.ToString() ?? "Unknown"
		};
	}

	/// <summary>
	/// Determines if the image is likely photographic based on metadata.
	/// </summary>
	/// <param name="metadata">The raster metadata to analyze.</param>
	/// <returns>True if the image appears to be photographic.</returns>
	public static bool IsPhotographic(this IRasterMetadata metadata)
	{
		return metadata.HasExifData() || metadata.HasGpsCoordinates() || metadata.ColorSpace == 1;
	}

	/// <summary>
	/// Determines if the image is likely a graphic/illustration.
	/// </summary>
	/// <param name="metadata">The raster metadata to analyze.</param>
	/// <returns>True if the image appears to be a graphic.</returns>
	public static bool IsGraphic(this IRasterMetadata metadata)
	{
		return !metadata.IsPhotographic() && metadata.BitDepth <= 8;
	}

	/// <summary>
	/// Determines if the image has transparency support.
	/// </summary>
	/// <param name="metadata">The raster metadata to check.</param>
	/// <returns>True if the format supports transparency.</returns>
	public static bool HasTransparency(this IRasterMetadata metadata)
	{
		// This would need to be implemented per format
		// For now, check if it's a format that commonly supports transparency
		return metadata.GetType().Name.Contains("Png") || 
		       metadata.GetType().Name.Contains("WebP") || 
		       metadata.GetType().Name.Contains("Tiff");
	}

	/// <summary>
	/// Determines if the image is an HDR image based on bit depth and color space.
	/// </summary>
	/// <param name="metadata">The raster metadata to analyze.</param>
	/// <returns>True if the image appears to be HDR.</returns>
	public static bool IsHdrImage(this IRasterMetadata metadata)
	{
		return metadata.BitDepth > 8 && (metadata.ColorSpace == 1 || metadata.HasIccProfile());
	}

	/// <summary>
	/// Creates a copy of the metadata with specified resolution.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="x">X resolution.</param>
	/// <param name="y">Y resolution.</param>
	/// <param name="unit">Resolution unit.</param>
	/// <returns>A cloned metadata with new resolution.</returns>
	public static IRasterMetadata WithResolution(this IRasterMetadata metadata, double x, double y, int unit = 2)
	{
		var clone = (IRasterMetadata)metadata.Clone();
		clone.SetResolution(x, y, unit);
		return clone;
	}

	/// <summary>
	/// Creates a copy of the metadata with specified GPS coordinates.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="latitude">Latitude in decimal degrees.</param>
	/// <param name="longitude">Longitude in decimal degrees.</param>
	/// <returns>A cloned metadata with new GPS coordinates.</returns>
	public static IRasterMetadata WithGpsCoordinates(this IRasterMetadata metadata, double latitude, double longitude)
	{
		var clone = (IRasterMetadata)metadata.Clone();
		clone.SetGpsCoordinates(latitude, longitude);
		return clone;
	}

	private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
	{
		const double R = 6371; // Earth's radius in kilometers
		var dLat = ToRadians(lat2 - lat1);
		var dLon = ToRadians(lon2 - lon1);
		var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
		        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
		        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
		return R * c;
	}

	private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}