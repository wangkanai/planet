// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Extension methods for PngMetadata providing PNG-specific utility functions.
/// </summary>
public static class PngMetadataExtensions
{
	/// <summary>
	/// Determines if the PNG has text chunks.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if any text chunks are present.</returns>
	public static bool HasTextChunks(this PngMetadata metadata)
	{
		return metadata.TextChunks.Count > 0 || 
		       metadata.CompressedTextChunks.Count > 0 || 
		       metadata.InternationalTextChunks.Count > 0;
	}

	/// <summary>
	/// Determines if the PNG has transparency information.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if transparency data is present.</returns>
	public static bool HasTransparency(this PngMetadata metadata)
	{
		return !metadata.TransparencyData.IsEmpty;
	}

	/// <summary>
	/// Determines if the PNG has gamma correction information.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if gamma value is set.</returns>
	public static bool HasGamma(this PngMetadata metadata)
	{
		return metadata.Gamma.HasValue;
	}

	/// <summary>
	/// Determines if the PNG has chromaticity information.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if chromaticity data is present.</returns>
	public static bool HasChromaticity(this PngMetadata metadata)
	{
		return metadata.WhitePoint.HasValue || 
		       metadata.RedPrimary.HasValue || 
		       metadata.GreenPrimary.HasValue || 
		       metadata.BluePrimary.HasValue;
	}

	/// <summary>
	/// Gets the total size of all text chunks in bytes.
	/// </summary>
	/// <param name="metadata">The PNG metadata to measure.</param>
	/// <returns>Total text size in bytes.</returns>
	public static int GetTotalTextSize(this PngMetadata metadata)
	{
		var size = 0;

		// Text chunks
		foreach (var text in metadata.TextChunks.Values)
		{
			size += System.Text.Encoding.UTF8.GetByteCount(text);
		}

		// Compressed text chunks
		foreach (var text in metadata.CompressedTextChunks.Values)
		{
			size += System.Text.Encoding.UTF8.GetByteCount(text);
		}

		// International text chunks
		foreach (var (languageTag, translatedKeyword, text) in metadata.InternationalTextChunks.Values)
		{
			size += System.Text.Encoding.UTF8.GetByteCount(text);
			if (!string.IsNullOrEmpty(languageTag))
				size += System.Text.Encoding.UTF8.GetByteCount(languageTag);
			if (!string.IsNullOrEmpty(translatedKeyword))
				size += System.Text.Encoding.UTF8.GetByteCount(translatedKeyword);
		}

		return size;
	}

	/// <summary>
	/// Gets the number of custom chunks.
	/// </summary>
	/// <param name="metadata">The PNG metadata to count.</param>
	/// <returns>Number of custom chunks.</returns>
	public static int GetCustomChunkCount(this PngMetadata metadata)
	{
		return metadata.CustomChunks.Count;
	}

	/// <summary>
	/// Gets the total size of custom chunks in bytes.
	/// </summary>
	/// <param name="metadata">The PNG metadata to measure.</param>
	/// <returns>Total custom chunk size in bytes.</returns>
	public static long GetCustomChunkSize(this PngMetadata metadata)
	{
		return metadata.CustomChunks.Values.Sum(chunk => chunk.Length);
	}

	/// <summary>
	/// Determines if the PNG has a background color set.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if background color is set.</returns>
	public static bool HasBackgroundColor(this PngMetadata metadata)
	{
		return metadata.BackgroundColor.HasValue;
	}

	/// <summary>
	/// Determines if the PNG has sRGB rendering intent set.
	/// </summary>
	/// <param name="metadata">The PNG metadata to check.</param>
	/// <returns>True if sRGB rendering intent is set.</returns>
	public static bool HasSrgbIntent(this PngMetadata metadata)
	{
		return metadata.SrgbRenderingIntent.HasValue;
	}

	/// <summary>
	/// Gets the sRGB rendering intent as a descriptive string.
	/// </summary>
	/// <param name="metadata">The PNG metadata to describe.</param>
	/// <returns>Rendering intent description.</returns>
	public static string GetSrgbIntentDescription(this PngMetadata metadata)
	{
		if (!metadata.SrgbRenderingIntent.HasValue)
			return "Not specified";

		return metadata.SrgbRenderingIntent.Value switch
		{
			0 => "Perceptual",
			1 => "Relative colorimetric",
			2 => "Saturation",
			3 => "Absolute colorimetric",
			_ => $"Unknown ({metadata.SrgbRenderingIntent.Value})"
		};
	}

	/// <summary>
	/// Adds a text chunk to the PNG metadata.
	/// </summary>
	/// <param name="metadata">The PNG metadata to modify.</param>
	/// <param name="keyword">The text chunk keyword.</param>
	/// <param name="text">The text content.</param>
	/// <exception cref="ArgumentException">Thrown if keyword is invalid.</exception>
	public static void AddTextChunk(this PngMetadata metadata, string keyword, string text)
	{
		if (string.IsNullOrWhiteSpace(keyword))
			throw new ArgumentException("Keyword cannot be null or empty.", nameof(keyword));

		if (keyword.Length > 79)
			throw new ArgumentException("Keyword cannot exceed 79 characters.", nameof(keyword));

		metadata.TextChunks[keyword] = text ?? string.Empty;
	}

	/// <summary>
	/// Adds an international text chunk to the PNG metadata.
	/// </summary>
	/// <param name="metadata">The PNG metadata to modify.</param>
	/// <param name="keyword">The text chunk keyword.</param>
	/// <param name="text">The text content.</param>
	/// <param name="languageTag">Optional language tag.</param>
	/// <param name="translatedKeyword">Optional translated keyword.</param>
	public static void AddInternationalTextChunk(this PngMetadata metadata, string keyword, string text, 
		string? languageTag = null, string? translatedKeyword = null)
	{
		if (string.IsNullOrWhiteSpace(keyword))
			throw new ArgumentException("Keyword cannot be null or empty.", nameof(keyword));

		metadata.InternationalTextChunks[keyword] = (languageTag, translatedKeyword, text ?? string.Empty);
	}

	/// <summary>
	/// Sets the chromaticity values for the PNG.
	/// </summary>
	/// <param name="metadata">The PNG metadata to modify.</param>
	/// <param name="whitePointX">White point X coordinate.</param>
	/// <param name="whitePointY">White point Y coordinate.</param>
	/// <param name="redX">Red primary X coordinate.</param>
	/// <param name="redY">Red primary Y coordinate.</param>
	/// <param name="greenX">Green primary X coordinate.</param>
	/// <param name="greenY">Green primary Y coordinate.</param>
	/// <param name="blueX">Blue primary X coordinate.</param>
	/// <param name="blueY">Blue primary Y coordinate.</param>
	public static void SetChromaticity(this PngMetadata metadata, 
		uint whitePointX, uint whitePointY,
		uint redX, uint redY, 
		uint greenX, uint greenY, 
		uint blueX, uint blueY)
	{
		metadata.WhitePoint = (whitePointX, whitePointY);
		metadata.RedPrimary = (redX, redY);
		metadata.GreenPrimary = (greenX, greenY);
		metadata.BluePrimary = (blueX, blueY);
	}

	/// <summary>
	/// Clears all chromaticity information.
	/// </summary>
	/// <param name="metadata">The PNG metadata to modify.</param>
	public static void ClearChromaticity(this PngMetadata metadata)
	{
		metadata.WhitePoint = null;
		metadata.RedPrimary = null;
		metadata.GreenPrimary = null;
		metadata.BluePrimary = null;
	}

	/// <summary>
	/// Determines if the PNG is optimized for web use.
	/// </summary>
	/// <param name="metadata">The PNG metadata to analyze.</param>
	/// <returns>True if optimized for web.</returns>
	public static bool IsWebOptimized(this PngMetadata metadata)
	{
		// Check for sRGB intent and reasonable resolution
		var hasSrgb = metadata.HasSrgbIntent();
		var reasonableResolution = !metadata.HasResolution() || 
		                          (metadata.GetResolutionInDpi() >= 72 && metadata.GetResolutionInDpi() <= 300);
		
		return hasSrgb && reasonableResolution && !metadata.HasCustomChunks();
	}

	/// <summary>
	/// Gets a summary of the PNG's color characteristics.
	/// </summary>
	/// <param name="metadata">The PNG metadata to analyze.</param>
	/// <returns>Color characteristics summary.</returns>
	public static string GetColorCharacteristics(this PngMetadata metadata)
	{
		var characteristics = new List<string>();

		if (metadata.HasGamma())
			characteristics.Add($"Gamma: {metadata.Gamma:F2}");

		if (metadata.HasSrgbIntent())
			characteristics.Add($"sRGB: {metadata.GetSrgbIntentDescription()}");

		if (metadata.HasChromaticity())
			characteristics.Add("Custom chromaticity");

		if (metadata.HasBackgroundColor())
			characteristics.Add("Background color set");

		if (metadata.HasTransparency())
			characteristics.Add("Transparency data");

		return characteristics.Count > 0 ? string.Join(", ", characteristics) : "Standard RGB";
	}

	/// <summary>
	/// Validates all PNG chunks and returns comprehensive results.
	/// </summary>
	/// <param name="metadata">The PNG metadata to validate.</param>
	/// <returns>Detailed validation results.</returns>
	public static PngValidationResult ValidateAllChunks(this PngMetadata metadata)
	{
		// Use the existing validation method and add chunk-specific validation
		var result = metadata.ValidateMetadata();
		
		// Additional chunk validation can be added here
		if (metadata.GetTotalTextSize() > 1000000) // 1MB of text data
		{
			result.AddWarning("Large amount of text data may impact performance.");
		}

		if (metadata.GetCustomChunkCount() > 50)
		{
			result.AddWarning("Large number of custom chunks may impact compatibility.");
		}

		return result;
	}

	/// <summary>
	/// Creates a simplified copy of the PNG metadata with only essential information.
	/// </summary>
	/// <param name="metadata">The source PNG metadata.</param>
	/// <returns>Simplified PNG metadata.</returns>
	public static PngMetadata CreateSimplified(this PngMetadata metadata)
	{
		var simplified = new PngMetadata();
		
		// Copy only essential properties
		simplified.Width = metadata.Width;
		simplified.Height = metadata.Height;
		simplified.Title = metadata.Title;
		simplified.BitDepth = metadata.BitDepth;
		
		// Copy resolution if present
		if (metadata.HasResolution())
		{
			simplified.XResolution = metadata.XResolution;
			simplified.YResolution = metadata.YResolution;
			simplified.ResolutionUnit = metadata.ResolutionUnit;
		}

		// Copy sRGB intent if present
		if (metadata.HasSrgbIntent())
		{
			simplified.SrgbRenderingIntent = metadata.SrgbRenderingIntent;
		}

		return simplified;
	}
}