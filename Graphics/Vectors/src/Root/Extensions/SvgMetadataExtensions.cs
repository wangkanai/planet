// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;

namespace Wangkanai.Graphics.Vectors.Extensions;

/// <summary>
/// Extension methods for SvgMetadata providing SVG-specific utility functions.
/// </summary>
public static class SvgMetadataExtensions
{
	/// <summary>
	/// Determines if the SVG is considered large based on metadata size.
	/// </summary>
	/// <param name="metadata">The SVG metadata to check.</param>
	/// <returns>True if the SVG is large.</returns>
	public static bool IsLargeSvg(this SvgMetadata metadata)
	{
		return metadata.IsLargeSvg;
	}

	/// <summary>
	/// Determines if the SVG requires performance optimization.
	/// </summary>
	/// <param name="metadata">The SVG metadata to analyze.</param>
	/// <returns>True if optimization is recommended.</returns>
	public static bool RequiresOptimization(this SvgMetadata metadata)
	{
		return metadata.RequiresOptimization;
	}

	/// <summary>
	/// Determines if the SVG has custom namespaces defined.
	/// </summary>
	/// <param name="metadata">The SVG metadata to check.</param>
	/// <returns>True if custom namespaces are present.</returns>
	public static bool HasNamespaces(this SvgMetadata metadata)
	{
		return metadata.Namespaces.Count > 0;
	}

	/// <summary>
	/// Determines if the SVG has custom properties.
	/// </summary>
	/// <param name="metadata">The SVG metadata to check.</param>
	/// <returns>True if custom properties are present.</returns>
	public static bool HasCustomProperties(this SvgMetadata metadata)
	{
		return metadata.CustomProperties.Count > 0;
	}

	/// <summary>
	/// Determines if the SVG is compressed.
	/// </summary>
	/// <param name="metadata">The SVG metadata to check.</param>
	/// <returns>True if the SVG is compressed.</returns>
	public static bool IsCompressed(this SvgMetadata metadata)
	{
		return metadata.IsCompressed;
	}

	/// <summary>
	/// Gets the complexity level of the SVG based on various factors.
	/// </summary>
	/// <param name="metadata">The SVG metadata to analyze.</param>
	/// <returns>SVG complexity level.</returns>
	public static SvgComplexityLevel GetComplexityLevel(this SvgMetadata metadata)
	{
		var score = 0;

		// Element count scoring
		if (metadata.ElementCount > 10000) score += 4;
		else if (metadata.ElementCount > 1000) score += 3;
		else if (metadata.ElementCount > 100) score += 2;
		else if (metadata.ElementCount > 10) score += 1;

		// Path length scoring
		if (metadata.TotalPathLength > 100000) score += 3;
		else if (metadata.TotalPathLength > 10000) score += 2;
		else if (metadata.TotalPathLength > 1000) score += 1;

		// Additional complexity factors
		if (metadata.HasNamespaces() && metadata.Namespaces.Count > 5) score += 1;
		if (metadata.HasCustomProperties() && metadata.CustomProperties.Count > 20) score += 1;
		if (metadata.IsCompressed) score += 1;

		return score switch
		{
			<= 2 => SvgComplexityLevel.Simple,
			<= 5 => SvgComplexityLevel.Moderate,
			<= 8 => SvgComplexityLevel.Complex,
			_ => SvgComplexityLevel.VeryComplex
		};
	}

	/// <summary>
	/// Gets the viewBox aspect ratio.
	/// </summary>
	/// <param name="metadata">The SVG metadata to calculate from.</param>
	/// <returns>ViewBox aspect ratio, or 1.0 if viewBox height is 0.</returns>
	public static double GetViewBoxAspectRatio(this SvgMetadata metadata)
	{
		return metadata.ViewBox.Height > 0 ? metadata.ViewBox.Width / metadata.ViewBox.Height : 1.0;
	}

	/// <summary>
	/// Gets the viewport aspect ratio.
	/// </summary>
	/// <param name="metadata">The SVG metadata to calculate from.</param>
	/// <returns>Viewport aspect ratio, or 1.0 if viewport height is 0.</returns>
	public static double GetViewportAspectRatio(this SvgMetadata metadata)
	{
		return metadata.ViewportHeight > 0 ? metadata.ViewportWidth / metadata.ViewportHeight : 1.0;
	}

	/// <summary>
	/// Determines if the viewBox and viewport have matching aspect ratios.
	/// </summary>
	/// <param name="metadata">The SVG metadata to check.</param>
	/// <param name="tolerance">Tolerance for aspect ratio comparison (default: 0.01).</param>
	/// <returns>True if aspect ratios match within tolerance.</returns>
	public static bool HasMatchingAspectRatios(this SvgMetadata metadata, double tolerance = 0.01)
	{
		var viewBoxRatio = metadata.GetViewBoxAspectRatio();
		var viewportRatio = metadata.GetViewportAspectRatio();
		
		return Math.Abs(viewBoxRatio - viewportRatio) <= tolerance;
	}

	/// <summary>
	/// Gets the estimated file size in bytes based on metadata.
	/// </summary>
	/// <param name="metadata">The SVG metadata to estimate from.</param>
	/// <returns>Estimated file size in bytes.</returns>
	public static long GetEstimatedFileSize(this SvgMetadata metadata)
	{
		var baseSize = 1000L; // Base SVG structure
		var elementSize = metadata.ElementCount * 50L; // Average 50 bytes per element
		var pathSize = (long)(metadata.TotalPathLength * 8); // 8 bytes per path unit
		var namespaceSize = metadata.Namespaces.Count * 100L;
		var customPropertySize = metadata.CustomProperties.Count * 50L;

		var totalSize = baseSize + elementSize + pathSize + namespaceSize + customPropertySize;

		// Apply compression factor if compressed
		if (metadata.IsCompressed)
		{
			var compressionRatio = metadata.CompressionLevel switch
			{
				<= 3 => 0.8,
				<= 6 => 0.6,
				<= 9 => 0.4,
				_ => 0.3
			};
			totalSize = (long)(totalSize * compressionRatio);
		}

		return totalSize;
	}

	/// <summary>
	/// Determines if the SVG is suitable for web use.
	/// </summary>
	/// <param name="metadata">The SVG metadata to analyze.</param>
	/// <returns>True if suitable for web deployment.</returns>
	public static bool IsSuitableForWeb(this SvgMetadata metadata)
	{
		var fileSize = metadata.GetEstimatedFileSize();
		var complexity = metadata.GetComplexityLevel();
		
		// Check web suitability criteria
		return fileSize < 1000000 && // < 1MB
		       complexity <= SvgComplexityLevel.Moderate &&
		       metadata.ElementCount < 5000 &&
		       !metadata.HasCustomProperties(); // Custom properties may not be widely supported
	}

	/// <summary>
	/// Determines if the SVG is suitable for printing.
	/// </summary>
	/// <param name="metadata">The SVG metadata to analyze.</param>
	/// <returns>True if suitable for print output.</returns>
	public static bool IsSuitableForPrint(this SvgMetadata metadata)
	{
		// Check if it's a vector (scalable) and has reasonable complexity
		return metadata.ElementCount > 0 &&
		       metadata.TotalPathLength > 0 &&
		       metadata.GetComplexityLevel() <= SvgComplexityLevel.Complex;
	}

	/// <summary>
	/// Gets the color space as an enumeration value.
	/// </summary>
	/// <param name="metadata">The SVG metadata to read from.</param>
	/// <returns>SVG color space enumeration.</returns>
	public static SvgColorSpace GetColorSpaceEnum(this SvgMetadata metadata)
	{
		return metadata.ColorSpace;
	}

	/// <summary>
	/// Sets the color space using an enumeration value.
	/// </summary>
	/// <param name="metadata">The SVG metadata to modify.</param>
	/// <param name="colorSpace">Color space to set.</param>
	public static void SetColorSpace(this SvgMetadata metadata, SvgColorSpace colorSpace)
	{
		metadata.ColorSpace = colorSpace;
	}

	/// <summary>
	/// Adds a custom namespace to the SVG.
	/// </summary>
	/// <param name="metadata">The SVG metadata to modify.</param>
	/// <param name="prefix">Namespace prefix.</param>
	/// <param name="uri">Namespace URI.</param>
	public static void AddNamespace(this SvgMetadata metadata, string prefix, string uri)
	{
		if (string.IsNullOrWhiteSpace(prefix))
			throw new ArgumentException("Prefix cannot be null or empty.", nameof(prefix));
		
		if (string.IsNullOrWhiteSpace(uri))
			throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

		metadata.Namespaces[prefix] = uri;
	}

	/// <summary>
	/// Removes a namespace from the SVG.
	/// </summary>
	/// <param name="metadata">The SVG metadata to modify.</param>
	/// <param name="prefix">Namespace prefix to remove.</param>
	/// <returns>True if the namespace was removed.</returns>
	public static bool RemoveNamespace(this SvgMetadata metadata, string prefix)
	{
		return metadata.Namespaces.Remove(prefix);
	}

	/// <summary>
	/// Adds a custom property to the SVG.
	/// </summary>
	/// <param name="metadata">The SVG metadata to modify.</param>
	/// <param name="key">Property key.</param>
	/// <param name="value">Property value.</param>
	public static void AddCustomProperty(this SvgMetadata metadata, string key, object value)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Key cannot be null or empty.", nameof(key));

		metadata.CustomProperties[key] = value;
	}

	/// <summary>
	/// Gets a custom property value with type casting.
	/// </summary>
	/// <typeparam name="T">Type to cast the value to.</typeparam>
	/// <param name="metadata">The SVG metadata to read from.</param>
	/// <param name="key">Property key.</param>
	/// <returns>Property value cast to type T, or default(T) if not found.</returns>
	public static T? GetCustomProperty<T>(this SvgMetadata metadata, string key)
	{
		if (metadata.CustomProperties.TryGetValue(key, out var value) && value is T typedValue)
			return typedValue;
		
		return default;
	}

	/// <summary>
	/// Removes a custom property from the SVG.
	/// </summary>
	/// <param name="metadata">The SVG metadata to modify.</param>
	/// <param name="key">Property key to remove.</param>
	/// <returns>True if the property was removed.</returns>
	public static bool RemoveCustomProperty(this SvgMetadata metadata, string key)
	{
		return metadata.CustomProperties.Remove(key);
	}

	/// <summary>
	/// Optimizes the SVG metadata for performance.
	/// </summary>
	/// <param name="metadata">The SVG metadata to optimize.</param>
	/// <returns>Optimized SVG metadata.</returns>
	public static SvgMetadata CreateOptimized(this SvgMetadata metadata)
	{
		var optimized = (SvgMetadata)metadata.Clone();
		
		// Enable compression if not already compressed and the SVG is large
		if (!optimized.IsCompressed && optimized.IsLargeSvg)
		{
			optimized.IsCompressed = true;
			optimized.CompressionLevel = 6; // Balanced compression
		}

		// Remove non-essential custom properties for performance
		if (optimized.CustomProperties.Count > 50)
		{
			optimized.CustomProperties.Clear();
		}

		return optimized;
	}

	/// <summary>
	/// Creates a web-optimized copy of the SVG metadata.
	/// </summary>
	/// <param name="metadata">The source SVG metadata.</param>
	/// <returns>Web-optimized SVG metadata.</returns>
	public static SvgMetadata CreateWebOptimized(this SvgMetadata metadata)
	{
		var webOptimized = metadata.CreateOptimized();
		
		// Ensure sRGB color space for web compatibility
		webOptimized.SetColorSpace(SvgColorSpace.sRGB);
		
		// Remove custom namespaces that might not be web-compatible
		var standardNamespaces = new[] { "svg", "xlink", "xml" };
		var namespacesToRemove = webOptimized.Namespaces.Keys
			.Where(key => !standardNamespaces.Contains(key.ToLowerInvariant()))
			.ToList();
		
		foreach (var ns in namespacesToRemove)
		{
			webOptimized.RemoveNamespace(ns);
		}

		return webOptimized;
	}

	/// <summary>
	/// Gets a summary description of the SVG's characteristics.
	/// </summary>
	/// <param name="metadata">The SVG metadata to describe.</param>
	/// <returns>Descriptive summary string.</returns>
	public static string GetCharacteristicsSummary(this SvgMetadata metadata)
	{
		var characteristics = new List<string>();
		
		characteristics.Add($"{metadata.ElementCount:N0} elements");
		characteristics.Add($"{metadata.TotalPathLength:N0} path units");
		characteristics.Add(metadata.GetComplexityLevel().ToString().ToLowerInvariant());
		
		if (metadata.IsCompressed)
			characteristics.Add("compressed");
		
		if (metadata.HasNamespaces())
			characteristics.Add($"{metadata.Namespaces.Count} namespaces");
		
		if (metadata.HasCustomProperties())
			characteristics.Add($"{metadata.CustomProperties.Count} custom properties");

		return string.Join(", ", characteristics);
	}
}

/// <summary>
/// SVG complexity level enumeration.
/// </summary>
public enum SvgComplexityLevel
{
	/// <summary>Simple SVG with few elements.</summary>
	Simple,
	
	/// <summary>Moderate complexity SVG.</summary>
	Moderate,
	
	/// <summary>Complex SVG with many elements.</summary>
	Complex,
	
	/// <summary>Very complex SVG requiring optimization.</summary>
	VeryComplex
}