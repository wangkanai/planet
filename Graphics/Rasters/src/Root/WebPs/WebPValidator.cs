// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Provides validation functionality for WebP raster images with performance optimizations.</summary>
public static class WebPValidator
{
	/// <summary>Validates a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <returns>A validation result indicating if the image is valid and any errors.</returns>
	public static WebPValidationResult Validate(this IWebPRaster webp)
	{
		ArgumentNullException.ThrowIfNull(webp);

		var result = new WebPValidationResult();

		// Validate core properties in order of importance for performance
		webp.ValidateDimensions(result);
		webp.ValidateQualitySettings(result);
		webp.ValidateFormatConsistency(result);
		webp.ValidateColorModeConstraints(result);
		webp.ValidateAnimationProperties(result);
		webp.ValidateMetadata(result);

		return result;
	}

	/// <summary>Validates the dimensions of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateDimensions(this IWebPRaster webp, WebPValidationResult result)
	{
		if (webp.Width <= 0)
			result.AddError($"Invalid width: {webp.Width}. Width must be greater than 0.");

		if (webp.Height <= 0)
			result.AddError($"Invalid height: {webp.Height}. Height must be greater than 0.");

		if (webp.Width > WebPConstants.MaxWidth)
			result.AddError($"Width exceeds maximum: {webp.Width} > {WebPConstants.MaxWidth}.");

		if (webp.Height > WebPConstants.MaxHeight)
			result.AddError($"Height exceeds maximum: {webp.Height} > {WebPConstants.MaxHeight}.");

		// Performance check: warn about very large images
		var pixelCount = (long)webp.Width * webp.Height;
		if (pixelCount > 100_000_000) // 100 megapixels
			result.AddWarning($"Very large image ({pixelCount:N0} pixels) may impact performance.");
	}

	/// <summary>Validates the quality and compression settings of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateQualitySettings(this IWebPRaster webp, WebPValidationResult result)
	{
		if (webp.Quality < WebPConstants.MinQuality || webp.Quality > WebPConstants.MaxQuality)
			result.AddError($"Invalid quality: {webp.Quality}. Quality must be between {WebPConstants.MinQuality} and {WebPConstants.MaxQuality}.");

		if (webp.CompressionLevel < WebPConstants.MinCompressionLevel || webp.CompressionLevel > WebPConstants.MaxCompressionLevel)
			result.AddError($"Invalid compression level: {webp.CompressionLevel}. Level must be between {WebPConstants.MinCompressionLevel} and {WebPConstants.MaxCompressionLevel}.");

		// Performance warnings
		if (webp.IsLossless && webp.CompressionLevel > 6)
			result.AddWarning("High compression levels (>6) for lossless WebP may significantly impact encoding performance.");

		if (!webp.IsLossless && webp.Quality < 10)
			result.AddWarning("Very low quality settings (<10) may produce visually poor results.");
	}

	/// <summary>Validates the format and compression consistency of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateFormatConsistency(this IWebPRaster webp, WebPValidationResult result)
	{
		switch (webp.Format)
		{
			case WebPFormat.Simple when webp.Compression != WebPCompression.VP8:
				result.AddError($"Simple format requires VP8 compression, but {webp.Compression} is specified.");
				break;
			case WebPFormat.Lossless when webp.Compression != WebPCompression.VP8L:
				result.AddError($"Lossless format requires VP8L compression, but {webp.Compression} is specified.");
				break;
			case WebPFormat.Extended when !webp.Metadata.IsExtended:
				result.AddWarning("Extended format specified but no extended features are enabled.");
				break;
		}

		// Validate compression ratio
		if (webp.CompressionRatio <= 0)
			result.AddError($"Invalid compression ratio: {webp.CompressionRatio}. Ratio must be greater than 0.");
	}

	/// <summary>Validates the color mode constraints of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateColorModeConstraints(this IWebPRaster webp, WebPValidationResult result)
	{
		var expectedChannels = webp.ColorMode switch
		{
			WebPColorMode.Rgb  => WebPConstants.RgbChannels,
			WebPColorMode.Rgba => WebPConstants.RgbaChannels,
			_                  => 0
		};

		if (expectedChannels == 0)
			result.AddError($"Invalid color mode: {webp.ColorMode}.");
		else if (webp.Channels != expectedChannels)
			result.AddError($"Invalid channel count: {webp.Channels}. Expected {expectedChannels} for {webp.ColorMode} color mode.");

		// Alpha channel consistency
		if (webp.HasAlpha && webp.ColorMode == WebPColorMode.Rgb && !webp.Metadata.HasAlpha)
			result.AddWarning("HasAlpha is true but color mode is RGB and no separate alpha channel is defined.");

		if (!webp.HasAlpha && webp.ColorMode == WebPColorMode.Rgba)
			result.AddWarning("RGBA color mode specified but HasAlpha is false.");
	}

	/// <summary>Validates the animation properties of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateAnimationProperties(this IWebPRaster webp, WebPValidationResult result)
	{
		// Check for animation flags and frame consistency
		if (webp.Metadata.HasAnimation)
		{
			if (webp.Format != WebPFormat.Extended)
				result.AddError("Animated WebP requires Extended format.");

			if (webp.Metadata.AnimationFrames.Count == 0)
				result.AddError("IsAnimated is true but no animation frames are defined.");
		}

		if (webp.IsAnimated)
		{
			// Validate each frame
			for (var i = 0; i < webp.Metadata.AnimationFrames.Count; i++)
			{
				var frame = webp.Metadata.AnimationFrames[i];
				if (frame.Width == 0 || frame.Height == 0)
					result.AddError($"Animation frame {i} has invalid dimensions: {frame.Width}x{frame.Height}.");

				if (frame.OffsetX + frame.Width > webp.Width)
					result.AddError($"Animation frame {i} extends beyond image width.");

				if (frame.OffsetY + frame.Height > webp.Height)
					result.AddError($"Animation frame {i} extends beyond image height.");

				if (frame.Duration == 0)
					result.AddWarning($"Animation frame {i} has zero duration, which may cause playback issues.");
			}

			// Performance warning for complex animations
			if (webp.Metadata.AnimationFrames.Count > 100)
				result.AddWarning("Large number of animation frames (>100) may impact performance and file size.");
		}
		else if (webp.Metadata.AnimationFrames.Count > 0) result.AddWarning("Animation frames are defined but IsAnimated is false.");
	}

	/// <summary>Validates the metadata of a WebP raster image.</summary>
	/// <param name="webp">The WebP raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateMetadata(this IWebPRaster webp, WebPValidationResult result)
	{
		var metadata = webp.Metadata;

		// Validate metadata consistency flags
		if (metadata.HasIccProfile && (metadata.IccProfile == null || metadata.IccProfile.Length == 0))
			result.AddWarning("HasIccProfile is true but IccProfile data is empty.");

		if (metadata.HasExif && (metadata.ExifData == null || metadata.ExifData.Length == 0))
			result.AddWarning("HasExif is true but ExifData is empty.");

		if (metadata.HasXmp && string.IsNullOrEmpty(metadata.XmpData))
			result.AddWarning("HasXmp is true but XmpData is empty.");

		// Validate extended features requirement
		if ((metadata.HasIccProfile || metadata.HasExif || metadata.HasXmp || metadata.HasAnimation)
		    && webp.Format != WebPFormat.Extended)
			result.AddWarning("Metadata features require Extended format for optimal compatibility.");

		// Validate custom chunks
		foreach (var chunk in metadata.CustomChunks)
		{
			if (chunk.Key.Length != 4)
				result.AddError($"Invalid chunk ID '{chunk.Key}': must be exactly 4 characters.");

			if (chunk.Value.IsEmpty)
				result.AddWarning($"Custom chunk '{chunk.Key}' has empty data.");
		}

		// Performance warning for large metadata
		var totalMetadataSize = (metadata.IccProfile?.Length ?? 0) + (metadata.ExifData?.Length ?? 0) + (metadata.XmpData?.Length ?? 0);
		if (totalMetadataSize > ImageConstants.LargeMetadataThreshold)
			result.AddWarning($"Large metadata size ({totalMetadataSize:N0} bytes) may impact performance.");
	}

	/// <summary>Validates WebP file signature.</summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data has a valid WebP signature, false otherwise.</returns>
	public static bool IsValidWebPSignature(ReadOnlySpan<byte> data)
	{
		if (data.Length < WebPConstants.RiffHeaderSize)
			return false;

		// Check RIFF header
		if (!data[..4].SequenceEqual(WebPConstants.Signature.AsSpan()))
			return false;

		// Check WEBP format identifier
		return data.Slice(8, 4).SequenceEqual(WebPConstants.FormatId.AsSpan());
	}

	/// <summary>Gets the WebP format type from file data.</summary>
	/// <param name="data">The file data to analyze.</param>
	/// <returns>The detected WebP format type.</returns>
	public static WebPFormat DetectFormat(ReadOnlySpan<byte> data)
	{
		if (!IsValidWebPSignature(data))
			throw new ArgumentException("Invalid WebP file signature.", nameof(data));

		if (data.Length < WebPConstants.RiffHeaderSize + WebPConstants.ChunkHeaderSize)
			return WebPFormat.Simple;

		var chunkId = data.Slice(WebPConstants.RiffHeaderSize, 4);

		if (chunkId.SequenceEqual(WebPConstants.VP8ChunkId.AsSpan()))
			return WebPFormat.Simple;

		if (chunkId.SequenceEqual(WebPConstants.VP8LChunkId.AsSpan()))
			return WebPFormat.Lossless;

		if (chunkId.SequenceEqual(WebPConstants.VP8XChunkId.AsSpan()))
			return WebPFormat.Extended;

		return WebPFormat.Simple;
	}
}
