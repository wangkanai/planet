// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text.Json;

namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Extension methods for converting metadata between different formats and representations.
/// </summary>
public static class MetadataConversionExtensions
{
	/// <summary>
	/// Converts metadata to a dictionary of properties.
	/// </summary>
	/// <param name="metadata">The metadata to convert.</param>
	/// <returns>Dictionary containing all metadata properties.</returns>
	public static Dictionary<string, object?> ToPropertyDictionary(this IMetadata metadata)
	{
		var properties = new Dictionary<string, object?>
		{
			[nameof(metadata.Width)] = metadata.Width,
			[nameof(metadata.Height)] = metadata.Height,
			[nameof(metadata.Title)] = metadata.Title,
			[nameof(metadata.Orientation)] = metadata.Orientation,
			[nameof(metadata.EstimatedMetadataSize)] = metadata.EstimatedMetadataSize,
			[nameof(metadata.HasLargeMetadata)] = metadata.HasLargeMetadata
		};

		// Add type-specific properties
		if (metadata is IRasterMetadata raster)
		{
			properties[nameof(raster.BitDepth)] = raster.BitDepth;
			properties[nameof(raster.XResolution)] = raster.XResolution;
			properties[nameof(raster.YResolution)] = raster.YResolution;
			properties[nameof(raster.ResolutionUnit)] = raster.ResolutionUnit;
			properties[nameof(raster.ColorSpace)] = raster.ColorSpace;
			properties[nameof(raster.GpsLatitude)] = raster.GpsLatitude;
			properties[nameof(raster.GpsLongitude)] = raster.GpsLongitude;
			properties["HasExifData"] = raster.HasExifData();
			properties["HasXmpData"] = raster.HasXmpData();
			properties["HasIccProfile"] = raster.HasIccProfile();
		}

		if (metadata is IVectorMetadata vector)
		{
			properties[nameof(vector.CoordinateReferenceSystem)] = vector.CoordinateReferenceSystem;
			properties[nameof(vector.ColorSpace)] = vector.ColorSpace;
			properties[nameof(vector.ElementCount)] = vector.ElementCount;
		}

		return properties;
	}

	/// <summary>
	/// Converts metadata to a JSON string representation.
	/// </summary>
	/// <param name="metadata">The metadata to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>JSON string representation of the metadata.</returns>
	public static string ToJsonString(this IMetadata metadata, bool indented = true)
	{
		var properties = metadata.ToPropertyDictionary();
		var options = new JsonSerializerOptions
		{
			WriteIndented = indented,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		return JsonSerializer.Serialize(properties, options);
	}

	/// <summary>
	/// Converts metadata to a compact string representation.
	/// </summary>
	/// <param name="metadata">The metadata to convert.</param>
	/// <returns>Compact string representation.</returns>
	public static string ToCompactString(this IMetadata metadata)
	{
		var parts = new List<string>
		{
			$"{metadata.Width}x{metadata.Height}"
		};

		if (metadata.HasTitle())
			parts.Add($"\"{metadata.Title}\"");

		if (metadata.HasOrientation())
			parts.Add($"orientation:{metadata.Orientation}");

		if (metadata is IRasterMetadata raster)
		{
			parts.Add($"depth:{raster.BitDepth}");
			
			if (raster.HasResolution())
				parts.Add($"res:{raster.XResolution}x{raster.YResolution}");
			
			if (raster.HasGpsCoordinates())
				parts.Add($"gps:{raster.GpsLatitude:F6},{raster.GpsLongitude:F6}");
		}

		if (metadata is IVectorMetadata vector)
		{
			parts.Add($"elements:{vector.ElementCount}");
			
			if (vector.HasCoordinateSystem())
				parts.Add($"crs:{vector.CoordinateReferenceSystem}");
		}

		return string.Join(" ", parts);
	}

	/// <summary>
	/// Converts metadata to XML representation.
	/// </summary>
	/// <param name="metadata">The metadata to convert.</param>
	/// <returns>XML string representation.</returns>
	public static string ToXmlString(this IMetadata metadata)
	{
		var properties = metadata.ToPropertyDictionary();
		var xml = new System.Text.StringBuilder();
		
		xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		xml.AppendLine("<metadata>");
		xml.AppendLine($"  <type>{metadata.GetType().Name}</type>");
		
		foreach (var (key, value) in properties)
		{
			if (value != null)
			{
				xml.AppendLine($"  <{key}>{System.Security.SecurityElement.Escape(value.ToString()!)}</{key}>");
			}
		}
		
		xml.AppendLine("</metadata>");
		
		return xml.ToString();
	}

	/// <summary>
	/// Converts metadata to a human-readable description.
	/// </summary>
	/// <param name="metadata">The metadata to describe.</param>
	/// <returns>Human-readable description.</returns>
	public static string ToHumanReadableString(this IMetadata metadata)
	{
		var description = new System.Text.StringBuilder();
		
		// Basic information
		description.AppendLine($"Image: {metadata.Width} × {metadata.Height} pixels");
		
		if (metadata.HasTitle())
			description.AppendLine($"Title: {metadata.Title}");

		// Aspect ratio and orientation
		var aspectRatio = metadata.GetAspectRatio();
		var orientation = metadata.IsLandscape() ? "landscape" : 
		                 metadata.IsPortrait() ? "portrait" : "square";
		description.AppendLine($"Format: {orientation} ({aspectRatio:F2}:1)");

		// Size information
		var pixelCount = metadata.GetPixelCount();
		description.AppendLine($"Resolution: {pixelCount:N0} pixels ({pixelCount / 1000000.0:F1} megapixels)");

		// Type-specific information
		if (metadata is IRasterMetadata raster)
		{
			description.AppendLine($"Bit depth: {raster.BitDepth} bits per channel");
			
			if (raster.HasResolution())
			{
				var dpi = raster.GetResolutionInDpi();
				description.AppendLine($"Print resolution: {dpi:F0} DPI");
			}
			
			if (raster.HasGpsCoordinates())
			{
				description.AppendLine($"Location: {raster.GpsLatitude:F6}°, {raster.GpsLongitude:F6}°");
			}
			
			var features = new List<string>();
			if (raster.HasExifData()) features.Add("EXIF");
			if (raster.HasXmpData()) features.Add("XMP");
			if (raster.HasIccProfile()) features.Add("ICC Profile");
			
			if (features.Any())
				description.AppendLine($"Metadata: {string.Join(", ", features)}");
		}

		if (metadata is IVectorMetadata vector)
		{
			description.AppendLine($"Elements: {vector.ElementCount:N0}");
			description.AppendLine($"Complexity: {vector.GetComplexityLevel()}");
			
			if (vector.HasCoordinateSystem())
				description.AppendLine($"Coordinate system: {vector.CoordinateReferenceSystem}");
		}

		// Performance characteristics
		var sizeInfo = $"Metadata size: {metadata.GetEstimatedSizeInKB():F1} KB";
		if (metadata.HasLargeMetadata)
			sizeInfo += " (large)";
		description.AppendLine(sizeInfo);

		return description.ToString().TrimEnd();
	}

	/// <summary>
	/// Converts metadata to CSV format for tabular analysis.
	/// </summary>
	/// <param name="metadata">The metadata to convert.</param>
	/// <param name="includeHeaders">Whether to include CSV headers.</param>
	/// <returns>CSV representation.</returns>
	public static string ToCsvString(this IMetadata metadata, bool includeHeaders = true)
	{
		var properties = metadata.ToPropertyDictionary();
		var csv = new System.Text.StringBuilder();
		
		if (includeHeaders)
		{
			csv.AppendLine(string.Join(",", properties.Keys));
		}
		
		var values = properties.Values.Select(v => 
			v?.ToString()?.Replace(",", ";") ?? string.Empty);
		csv.AppendLine(string.Join(",", values));
		
		return csv.ToString();
	}

	/// <summary>
	/// Creates a metadata summary object with key statistics.
	/// </summary>
	/// <param name="metadata">The metadata to summarize.</param>
	/// <returns>Metadata summary object.</returns>
	public static MetadataSummary CreateSummary(this IMetadata metadata)
	{
		return new MetadataSummary
		{
			Type = metadata.GetType().Name,
			Dimensions = metadata.GetDimensions(),
			PixelCount = metadata.GetPixelCount(),
			AspectRatio = metadata.GetAspectRatio(),
			MetadataSize = metadata.EstimatedMetadataSize,
			IsLarge = metadata.HasLargeMetadata,
			HasTitle = metadata.HasTitle(),
			HasOrientation = metadata.HasOrientation(),
			
			// Raster-specific
			BitDepth = (metadata as IRasterMetadata)?.BitDepth,
			HasResolution = (metadata as IRasterMetadata)?.HasResolution() ?? false,
			HasGpsData = (metadata as IRasterMetadata)?.HasGpsCoordinates() ?? false,
			HasColorProfile = (metadata as IRasterMetadata)?.HasIccProfile() ?? false,
			
			// Vector-specific
			ElementCount = (metadata as IVectorMetadata)?.ElementCount,
			HasCoordinateSystem = (metadata as IVectorMetadata)?.HasCoordinateSystem() ?? false,
			ComplexityLevel = (metadata as IVectorMetadata)?.GetComplexityLevel().ToString()
		};
	}

	/// <summary>
	/// Converts a collection of metadata to a comparison table.
	/// </summary>
	/// <param name="metadataCollection">Collection of metadata to compare.</param>
	/// <returns>Comparison table as CSV string.</returns>
	public static string ToComparisonTable(this IEnumerable<IMetadata> metadataCollection)
	{
		var metadataList = metadataCollection.ToList();
		if (!metadataList.Any())
			return string.Empty;

		// Get all unique property keys
		var allProperties = metadataList
			.SelectMany(m => m.ToPropertyDictionary().Keys)
			.Distinct()
			.OrderBy(k => k)
			.ToList();

		var csv = new System.Text.StringBuilder();
		
		// Headers
		csv.AppendLine("Index," + string.Join(",", allProperties));
		
		// Data rows
		for (int i = 0; i < metadataList.Count; i++)
		{
			var properties = metadataList[i].ToPropertyDictionary();
			var values = allProperties.Select(key => 
				properties.TryGetValue(key, out var value) && value != null 
					? value.ToString()?.Replace(",", ";") ?? string.Empty 
					: string.Empty);
			
			csv.AppendLine($"{i + 1},{string.Join(",", values)}");
		}
		
		return csv.ToString();
	}
}

/// <summary>
/// Metadata summary structure for quick analysis.
/// </summary>
public record MetadataSummary
{
	public string Type { get; init; } = string.Empty;
	public (int width, int height) Dimensions { get; init; }
	public long PixelCount { get; init; }
	public float AspectRatio { get; init; }
	public long MetadataSize { get; init; }
	public bool IsLarge { get; init; }
	public bool HasTitle { get; init; }
	public bool HasOrientation { get; init; }
	
	// Raster-specific properties
	public int? BitDepth { get; init; }
	public bool HasResolution { get; init; }
	public bool HasGpsData { get; init; }
	public bool HasColorProfile { get; init; }
	
	// Vector-specific properties
	public int? ElementCount { get; init; }
	public bool HasCoordinateSystem { get; init; }
	public string? ComplexityLevel { get; init; }
}