// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Provides validation functionality for AVIF images and format compliance.
/// </summary>
public static class AvifValidator
{
	/// <summary>
	/// Validates an AVIF raster image and returns detailed validation results.
	/// </summary>
	/// <param name="avif">The AVIF raster to validate.</param>
	/// <returns>A validation result containing any errors or warnings.</returns>
	public static AvifValidationResult Validate(IAvifRaster avif)
	{
		var result = new AvifValidationResult();

		ValidateDimensions(avif, result);
		ValidateBitDepth(avif, result);
		ValidateColorSpace(avif, result);
		ValidateQualitySettings(avif, result);
		ValidateChromaSubsampling(avif, result);
		ValidateHdrSettings(avif, result);
		ValidateMetadata(avif, result);
		ValidateEncodingSettings(avif, result);
		ValidateMemoryConstraints(avif, result);

		return result;
	}

	/// <summary>Validates image dimensions.</summary>
	private static void ValidateDimensions(IAvifRaster avif, AvifValidationResult result)
	{
		if (avif.Width <= 0)
			result.AddError($"Invalid width: {avif.Width}. Width must be greater than 0.");

		if (avif.Height <= 0)
			result.AddError($"Invalid height: {avif.Height}. Height must be greater than 0.");

		if (avif.Width > AvifConstants.MaxDimension)
			result.AddError($"Width exceeds maximum: {avif.Width} > {AvifConstants.MaxDimension}.");

		if (avif.Height > AvifConstants.MaxDimension)
			result.AddError($"Height exceeds maximum: {avif.Height} > {AvifConstants.MaxDimension}.");

		// Check for extremely large images
		var totalPixels = (long)avif.Width * avif.Height;
		if (totalPixels > 100_000_000) // 100 megapixels
			result.AddWarning($"Very large image: {totalPixels:N0} pixels. Consider using tiling for better performance.");

		// Check aspect ratio
		var aspectRatio = (double)avif.Width / avif.Height;
		if (aspectRatio > 16.0 || aspectRatio < 0.0625)
			result.AddWarning($"Extreme aspect ratio: {aspectRatio:F2}. May cause compatibility issues.");
	}

	/// <summary>Validates bit depth settings.</summary>
	private static void ValidateBitDepth(IAvifRaster avif, AvifValidationResult result)
	{
		if (avif.BitDepth != 8 && avif.BitDepth != 10 && avif.BitDepth != 12)
			result.AddError($"Invalid bit depth: {avif.BitDepth}. Must be 8, 10, or 12.");

		// Check HDR requirements
		if (avif.HasHdrMetadata && avif.BitDepth < 10)
			result.AddWarning("HDR content typically requires 10-bit or higher bit depth.");

		// Check color space compatibility
		if (avif.ColorSpace == AvifColorSpace.Bt2100Pq || avif.ColorSpace == AvifColorSpace.Bt2100Hlg)
		{
			if (avif.BitDepth < 10)
				result.AddError("BT.2100 color spaces require 10-bit or higher bit depth.");
		}

		// Platform support
		var features = avif.GetSupportedFeatures();
		if (avif.BitDepth == 10 && !features.HasFlag(AvifFeatures.TenBitDepth))
			result.AddError("10-bit depth is not supported on this platform.");

		if (avif.BitDepth == 12 && !features.HasFlag(AvifFeatures.TwelveBitDepth))
			result.AddError("12-bit depth is not supported on this platform.");
	}

	/// <summary>Validates color space settings.</summary>
	private static void ValidateColorSpace(IAvifRaster avif, AvifValidationResult result)
	{
		if (!Enum.IsDefined(typeof(AvifColorSpace), avif.ColorSpace))
			result.AddError($"Invalid color space: {avif.ColorSpace}.");

		// Validate HDR color spaces
		if (avif.ColorSpace == AvifColorSpace.Bt2100Pq || avif.ColorSpace == AvifColorSpace.Bt2100Hlg)
		{
			if (!avif.HasHdrMetadata)
				result.AddWarning("HDR color space specified but no HDR metadata present.");
		}

		// Validate wide gamut color spaces
		if (avif.ColorSpace == AvifColorSpace.DisplayP3 || avif.ColorSpace == AvifColorSpace.Bt2020Ncl)
		{
			if (avif.BitDepth < 10)
				result.AddWarning("Wide gamut color spaces benefit from 10-bit or higher bit depth.");
		}
	}

	/// <summary>Validates quality settings.</summary>
	private static void ValidateQualitySettings(IAvifRaster avif, AvifValidationResult result)
	{
		if (avif.Quality < AvifConstants.MinQuality || avif.Quality > AvifConstants.MaxQuality)
			result.AddError($"Invalid quality: {avif.Quality}. Must be between {AvifConstants.MinQuality} and {AvifConstants.MaxQuality}.");

		if (avif.IsLossless && avif.Quality != AvifConstants.QualityPresets.Lossless)
			result.AddWarning("Lossless mode typically uses quality 100.");

		if (!avif.IsLossless && avif.Quality == AvifConstants.QualityPresets.Lossless)
			result.AddWarning("Quality 100 with lossy mode may not provide true lossless compression.");

		if (avif.Speed < AvifConstants.MinSpeed || avif.Speed > AvifConstants.MaxSpeed)
			result.AddError($"Invalid speed: {avif.Speed}. Must be between {AvifConstants.MinSpeed} and {AvifConstants.MaxSpeed}.");

		// Speed vs quality trade-off
		if (avif.Speed >= 8 && avif.Quality >= 90)
			result.AddWarning("High speed with high quality may not achieve optimal compression.");
	}

	/// <summary>Validates chroma subsampling settings.</summary>
	private static void ValidateChromaSubsampling(IAvifRaster avif, AvifValidationResult result)
	{
		if (!Enum.IsDefined(typeof(AvifChromaSubsampling), avif.ChromaSubsampling))
			result.AddError($"Invalid chroma subsampling: {avif.ChromaSubsampling}.");

		// Lossless should use 4:4:4
		if (avif.IsLossless && avif.ChromaSubsampling != AvifChromaSubsampling.Yuv444)
			result.AddWarning("Lossless compression should use YUV 4:4:4 chroma subsampling.");

		// Monochrome validation
		if (avif.ChromaSubsampling == AvifChromaSubsampling.Yuv400)
		{
			if (avif.ColorSpace != AvifColorSpace.Unknown && avif.ColorSpace != AvifColorSpace.Bt601)
				result.AddWarning("Monochrome (YUV 4:0:0) with color space other than grayscale.");
		}

		// HDR should use minimal subsampling
		if (avif.HasHdrMetadata && avif.ChromaSubsampling == AvifChromaSubsampling.Yuv420)
			result.AddWarning("HDR content typically benefits from YUV 4:2:2 or 4:4:4 chroma subsampling.");
	}

	/// <summary>Validates HDR settings.</summary>
	private static void ValidateHdrSettings(IAvifRaster avif, AvifValidationResult result)
	{
		if (!avif.HasHdrMetadata)
			return;

		var hdr = avif.Metadata.HdrInfo!;

		if (hdr.MaxLuminance <= hdr.MinLuminance)
			result.AddError("HDR maximum luminance must be greater than minimum luminance.");

		if (hdr.MinLuminance < 0)
			result.AddError("HDR minimum luminance cannot be negative.");

		if (hdr.MaxLuminance > 10000)
			result.AddWarning("HDR maximum luminance exceeds typical display capabilities (10,000 nits).");

		if (hdr.MaxContentLightLevel > hdr.MaxLuminance)
			result.AddError("Max content light level cannot exceed maximum luminance.");

		if (hdr.MaxFrameAverageLightLevel > hdr.MaxContentLightLevel)
			result.AddError("Max frame average light level cannot exceed max content light level.");

		// Validate HDR format
		if (!Enum.IsDefined(typeof(HdrFormat), hdr.Format))
			result.AddError($"Invalid HDR format: {hdr.Format}.");
	}

	/// <summary>Validates metadata consistency.</summary>
	private static void ValidateMetadata(IAvifRaster avif, AvifValidationResult result)
	{
		var metadata = avif.Metadata;

		// Basic consistency checks
		if (metadata.Width != avif.Width)
			result.AddError($"Width mismatch: Raster={avif.Width}, Metadata={metadata.Width}.");

		if (metadata.Height != avif.Height)
			result.AddError($"Height mismatch: Raster={avif.Height}, Metadata={metadata.Height}.");

		if (metadata.BitDepth != avif.BitDepth)
			result.AddError($"Bit depth mismatch: Raster={avif.BitDepth}, Metadata={metadata.BitDepth}.");

		// Validate EXIF data
		if (metadata.ExifData != null && metadata.ExifData.Length > 0)
		{
			if (metadata.ExifData.Length < 6) // Minimum EXIF header size
				result.AddWarning("EXIF data appears too small to be valid.");
		}

		// Validate ICC profile
		if (metadata.IccProfile != null && metadata.IccProfile.Length > 0)
		{
			if (metadata.IccProfile.Length < 128) // Minimum ICC profile size
				result.AddWarning("ICC profile data appears too small to be valid.");
		}

		// Validate pixel aspect ratio
		if (metadata.PixelAspectRatio <= 0)
			result.AddError("Pixel aspect ratio must be positive.");

		// Validate rotation
		if (metadata.Rotation != 0 && metadata.Rotation != 90 && metadata.Rotation != 180 && metadata.Rotation != 270)
			result.AddError($"Invalid rotation: {metadata.Rotation}. Must be 0, 90, 180, or 270.");

		// Validate clean aperture
		if (metadata.CleanAperture != null)
		{
			var ca = metadata.CleanAperture;
			if (ca.Width <= 0 || ca.Height <= 0)
				result.AddError("Clean aperture dimensions must be positive.");

			if (ca.Width + Math.Abs(ca.HorizontalOffset) > avif.Width ||
				ca.Height + Math.Abs(ca.VerticalOffset) > avif.Height)
				result.AddError("Clean aperture extends outside image bounds.");
		}
	}

	/// <summary>Validates encoding settings.</summary>
	private static void ValidateEncodingSettings(IAvifRaster avif, AvifValidationResult result)
	{
		if (avif.ThreadCount < 0 || avif.ThreadCount > AvifConstants.Memory.MaxThreads)
			result.AddError($"Invalid thread count: {avif.ThreadCount}. Must be between 0 and {AvifConstants.Memory.MaxThreads}.");

		// Film grain validation
		if (avif.EnableFilmGrain)
		{
			var features = avif.GetSupportedFeatures();
			if (!features.HasFlag(AvifFeatures.FilmGrain))
				result.AddError("Film grain synthesis is not supported on this platform.");

			if (avif.IsLossless)
				result.AddWarning("Film grain synthesis with lossless compression may increase file size significantly.");
		}

		// Alpha channel validation
		if (avif.HasAlpha)
		{
			var features = avif.GetSupportedFeatures();
			if (!features.HasFlag(AvifFeatures.AlphaChannel))
				result.AddError("Alpha channel is not supported on this platform.");

			if (avif.Metadata.AlphaPremultiplied)
				result.AddWarning("Premultiplied alpha may cause compatibility issues with some decoders.");
		}
	}

	/// <summary>Validates memory constraints.</summary>
	private static void ValidateMemoryConstraints(IAvifRaster avif, AvifValidationResult result)
	{
		// Calculate memory requirements
		var pixelMemory = (long)avif.Width * avif.Height * (avif.HasAlpha ? 4 : 3);
		if (avif.BitDepth > 8)
			pixelMemory = pixelMemory * avif.BitDepth / 8;

		if (pixelMemory > (long)AvifConstants.Memory.MaxPixelBufferSizeMB * 1024 * 1024)
			result.AddWarning($"Large image size ({pixelMemory / (1024 * 1024)} MB) may cause memory issues.");

		// Check metadata size
		var metadataSize = avif.Metadata.EstimatedMetadataSize;
		if (metadataSize > ImageConstants.VeryLargeMetadataThreshold)
			result.AddWarning($"Very large metadata size ({metadataSize / (1024 * 1024)} MB) may impact performance.");

		// Estimate encoded size
		var estimatedSize = avif.GetEstimatedFileSize();
		if (estimatedSize > 50 * 1024 * 1024) // 50 MB
			result.AddWarning($"Large estimated file size ({estimatedSize / (1024 * 1024)} MB). Consider reducing quality or dimensions.");
	}

	/// <summary>
	/// Validates AVIF file signature from raw data.
	/// </summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data contains a valid AVIF signature.</returns>
	public static bool IsValidAvifSignature(byte[] data)
	{
		if (data == null || data.Length < 12)
			return false;

		return IsValidAvifSignature(data.AsSpan());
	}

	/// <summary>
	/// Validates AVIF file signature from raw data span.
	/// </summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data contains a valid AVIF signature.</returns>
	public static bool IsValidAvifSignature(ReadOnlySpan<byte> data)
	{
		if (data.Length < 12) // Minimum for ftyp box
			return false;

		// Read box size (big-endian)
		var boxSize = (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
		if (boxSize < 20) // Minimum ftyp box size
			return false;

		// Check ftyp box type
		if (!data.Slice(4, 4).SequenceEqual(AvifConstants.FileTypeBoxType.AsSpan()))
			return false;

		// Check major brand (avif or avis)
		var majorBrand = data.Slice(8, 4);
		return majorBrand.SequenceEqual(AvifConstants.AvifBrand.AsSpan()) ||
			   majorBrand.SequenceEqual(AvifConstants.AvisBrand.AsSpan());
	}

	/// <summary>
	/// Detects the AVIF variant from file data.
	/// </summary>
	/// <param name="data">The file data to analyze.</param>
	/// <returns>The detected variant ("avif" for still images, "avis" for sequences, or empty if invalid).</returns>
	public static string DetectAvifVariant(ReadOnlySpan<byte> data)
	{
		if (!IsValidAvifSignature(data))
			return string.Empty;

		// Major brand is at offset 8
		var majorBrand = data.Slice(8, 4);
		
		if (majorBrand.SequenceEqual(AvifConstants.AvifBrand.AsSpan()))
			return "avif";
		
		if (majorBrand.SequenceEqual(AvifConstants.AvisBrand.AsSpan()))
			return "avis";

		return string.Empty;
	}

	/// <summary>
	/// Checks if the data contains compatible brands.
	/// </summary>
	/// <param name="data">The file data to check.</param>
	/// <param name="brand">The brand to look for.</param>
	/// <returns>True if the brand is found in compatible brands list.</returns>
	public static bool HasCompatibleBrand(ReadOnlySpan<byte> data, string brand)
	{
		if (!IsValidAvifSignature(data) || data.Length < 24)
			return false;

		// Box size
		var boxSize = (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
		if (boxSize > data.Length)
			return false;

		var brandBytes = System.Text.Encoding.ASCII.GetBytes(brand);
		
		// Check major brand first
		if (data.Slice(8, 4).SequenceEqual(brandBytes))
			return true;

		// Check compatible brands (start at offset 16)
		for (var i = 16; i + 4 <= boxSize && i + 4 <= data.Length; i += 4)
		{
			if (data.Slice(i, 4).SequenceEqual(brandBytes))
				return true;
		}

		return false;
	}
}