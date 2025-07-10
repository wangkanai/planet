// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Extensions;

namespace Wangkanai.Graphics.Vectors.Extensions;

/// <summary>
/// Extension methods for IVectorMetadata interface providing vector-specific utility functions.
/// </summary>
public static class VectorMetadataExtensions
{
	/// <summary>
	/// Determines if the metadata has a coordinate reference system defined.
	/// </summary>
	/// <param name="metadata">The vector metadata to check.</param>
	/// <returns>True if coordinate reference system is set.</returns>
	public static bool HasCoordinateSystem(this IVectorMetadata metadata)
	{
		return !string.IsNullOrWhiteSpace(metadata.CoordinateReferenceSystem);
	}

	/// <summary>
	/// Determines if the coordinate reference system is valid.
	/// </summary>
	/// <param name="metadata">The vector metadata to validate.</param>
	/// <returns>True if coordinate reference system is valid or null.</returns>
	public static bool IsValidCoordinateSystem(this IVectorMetadata metadata)
	{
		if (!metadata.HasCoordinateSystem())
			return true; // Null CRS is considered valid

		var crs = metadata.CoordinateReferenceSystem!;
		
		// Check for common CRS formats
		return crs.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) ||
		       crs.StartsWith("urn:ogc:def:crs:", StringComparison.OrdinalIgnoreCase) ||
		       crs.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
		       crs.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
		       crs.Equals("WGS84", StringComparison.OrdinalIgnoreCase) ||
		       crs.Equals("CRS84", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Determines if the metadata has vector elements.
	/// </summary>
	/// <param name="metadata">The vector metadata to check.</param>
	/// <returns>True if element count is greater than 0.</returns>
	public static bool HasElements(this IVectorMetadata metadata)
	{
		return metadata.ElementCount > 0;
	}

	/// <summary>
	/// Determines if the vector is complex based on element count.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <param name="threshold">Element count threshold for complexity (default: 1000).</param>
	/// <returns>True if the vector is considered complex.</returns>
	public static bool IsComplexVector(this IVectorMetadata metadata, int threshold = 1000)
	{
		return metadata.ElementCount > threshold;
	}

	/// <summary>
	/// Calculates a complexity score based on element count and other factors.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <returns>Complexity score from 0.0 (simple) to 1.0 (very complex).</returns>
	public static double GetComplexityScore(this IVectorMetadata metadata)
	{
		var score = 0.0;

		// Base score from element count
		if (metadata.ElementCount > 0)
		{
			score += Math.Min(metadata.ElementCount / 10000.0, 0.6); // Max 0.6 from element count
		}

		// Additional complexity from coordinate system
		if (metadata.HasCoordinateSystem())
		{
			score += 0.1;
		}

		// Additional complexity from color space
		if (!string.IsNullOrWhiteSpace(metadata.ColorSpace))
		{
			score += 0.1;
		}

		// Additional complexity from dimensions
		var pixelCount = (long)metadata.Width * metadata.Height;
		if (pixelCount > 1000000) // > 1 megapixel
		{
			score += 0.2;
		}

		return Math.Min(score, 1.0);
	}

	/// <summary>
	/// Determines if the vector has a color space defined.
	/// </summary>
	/// <param name="metadata">The vector metadata to check.</param>
	/// <returns>True if color space is set.</returns>
	public static bool HasColorSpace(this IVectorMetadata metadata)
	{
		return !string.IsNullOrWhiteSpace(metadata.ColorSpace);
	}

	/// <summary>
	/// Determines if the color space is a standard color space.
	/// </summary>
	/// <param name="metadata">The vector metadata to validate.</param>
	/// <returns>True if color space is a recognized standard.</returns>
	public static bool IsStandardColorSpace(this IVectorMetadata metadata)
	{
		if (!metadata.HasColorSpace())
			return true; // Null color space is considered valid

		var colorSpace = metadata.ColorSpace!.ToLowerInvariant();
		
		return colorSpace is "srgb" or "rgb" or "cmyk" or "gray" or "lab" or 
		       "xyz" or "rec2020" or "p3" or "adobe-rgb" or "prophoto-rgb";
	}

	/// <summary>
	/// Gets the estimated complexity level of the vector.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <returns>Complexity level enumeration.</returns>
	public static VectorComplexityLevel GetComplexityLevel(this IVectorMetadata metadata)
	{
		var score = metadata.GetComplexityScore();

		return score switch
		{
			< 0.2 => VectorComplexityLevel.Simple,
			< 0.5 => VectorComplexityLevel.Moderate,
			< 0.8 => VectorComplexityLevel.Complex,
			_ => VectorComplexityLevel.VeryComplex
		};
	}

	/// <summary>
	/// Determines if the vector requires performance optimization.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <returns>True if optimization is recommended.</returns>
	public static bool RequiresOptimization(this IVectorMetadata metadata)
	{
		return metadata.GetComplexityLevel() >= VectorComplexityLevel.Complex;
	}

	/// <summary>
	/// Gets the estimated render time based on complexity.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <returns>Estimated render time in milliseconds.</returns>
	public static double GetEstimatedRenderTime(this IVectorMetadata metadata)
	{
		var baseTime = 10.0; // Base render time in ms
		var complexity = metadata.GetComplexityScore();
		var elementMultiplier = Math.Log10(Math.Max(metadata.ElementCount, 1)) * 5;
		
		return baseTime + (complexity * 100) + elementMultiplier;
	}

	/// <summary>
	/// Determines if the vector is suitable for real-time rendering.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <param name="maxRenderTime">Maximum acceptable render time in milliseconds (default: 16.67ms for 60fps).</param>
	/// <returns>True if suitable for real-time rendering.</returns>
	public static bool IsSuitableForRealTimeRendering(this IVectorMetadata metadata, double maxRenderTime = 16.67)
	{
		return metadata.GetEstimatedRenderTime() <= maxRenderTime;
	}

	/// <summary>
	/// Gets the memory footprint estimate for the vector in bytes.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <returns>Estimated memory usage in bytes.</returns>
	public static long GetEstimatedMemoryFootprint(this IVectorMetadata metadata)
	{
		var baseSize = metadata.EstimatedMetadataSize;
		var elementSize = metadata.ElementCount * 64; // Estimated 64 bytes per element
		var dimensionSize = (long)metadata.Width * metadata.Height * 4; // 4 bytes per pixel for RGBA
		
		return baseSize + elementSize + (dimensionSize / 100); // Vectors don't store full pixel data
	}

	/// <summary>
	/// Determines if the vector is memory-intensive.
	/// </summary>
	/// <param name="metadata">The vector metadata to analyze.</param>
	/// <param name="threshold">Memory threshold in MB (default: 10MB).</param>
	/// <returns>True if memory usage exceeds threshold.</returns>
	public static bool IsMemoryIntensive(this IVectorMetadata metadata, double threshold = 10.0)
	{
		var memoryMB = metadata.GetEstimatedMemoryFootprint() / (1024.0 * 1024.0);
		return memoryMB > threshold;
	}

	/// <summary>
	/// Creates a copy of the metadata with specified coordinate reference system.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="crs">New coordinate reference system.</param>
	/// <returns>A cloned metadata with new CRS.</returns>
	public static IVectorMetadata WithCoordinateSystem(this IVectorMetadata metadata, string crs)
	{
		var clone = (IVectorMetadata)metadata.Clone();
		clone.CoordinateReferenceSystem = crs;
		return clone;
	}

	/// <summary>
	/// Creates a copy of the metadata with specified color space.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="colorSpace">New color space.</param>
	/// <returns>A cloned metadata with new color space.</returns>
	public static IVectorMetadata WithColorSpace(this IVectorMetadata metadata, string colorSpace)
	{
		var clone = (IVectorMetadata)metadata.Clone();
		clone.ColorSpace = colorSpace;
		return clone;
	}

	/// <summary>
	/// Creates a copy of the metadata with specified element count.
	/// </summary>
	/// <param name="metadata">The source metadata.</param>
	/// <param name="elementCount">New element count.</param>
	/// <returns>A cloned metadata with new element count.</returns>
	public static IVectorMetadata WithElementCount(this IVectorMetadata metadata, int elementCount)
	{
		var clone = (IVectorMetadata)metadata.Clone();
		clone.ElementCount = elementCount;
		return clone;
	}

	/// <summary>
	/// Gets a human-readable description of the vector's characteristics.
	/// </summary>
	/// <param name="metadata">The vector metadata to describe.</param>
	/// <returns>Descriptive string summarizing the vector.</returns>
	public static string GetDescription(this IVectorMetadata metadata)
	{
		var complexity = metadata.GetComplexityLevel();
		var elements = metadata.ElementCount;
		var dimensions = metadata.GetDimensions();
		
		return $"{complexity} vector with {elements:N0} elements at {dimensions.width}×{dimensions.height}";
	}
}

/// <summary>
/// Enumeration of vector complexity levels.
/// </summary>
public enum VectorComplexityLevel
{
	/// <summary>Simple vector with few elements.</summary>
	Simple,
	
	/// <summary>Moderate complexity vector.</summary>
	Moderate,
	
	/// <summary>Complex vector with many elements.</summary>
	Complex,
	
	/// <summary>Very complex vector requiring optimization.</summary>
	VeryComplex
}