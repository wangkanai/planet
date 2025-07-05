// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Provides validation functionality for JPEG raster images.</summary>
public static class JpegValidator
{
	/// <summary>Validates a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <returns>A validation result indicating if the image is valid and any errors.</returns>
	public static JpegValidationResult Validate(IJpegRaster jpeg)
	{
		ArgumentNullException.ThrowIfNull(jpeg);

		var result = new JpegValidationResult();
		
		// Validate dimensions
		if (jpeg.Width <= 0)
			result.AddError($"Invalid width: {jpeg.Width}. Width must be greater than 0.");
		
		if (jpeg.Height <= 0)
			result.AddError($"Invalid height: {jpeg.Height}. Height must be greater than 0.");
		
		if (jpeg.Width > JpegConstants.MaxDimension)
			result.AddError($"Width exceeds maximum: {jpeg.Width} > {JpegConstants.MaxDimension}.");
		
		if (jpeg.Height > JpegConstants.MaxDimension)
			result.AddError($"Height exceeds maximum: {jpeg.Height} > {JpegConstants.MaxDimension}.");

		// Validate quality
		if (jpeg.Quality < JpegConstants.MinQuality || jpeg.Quality > JpegConstants.MaxQuality)
			result.AddError($"Invalid quality: {jpeg.Quality}. Quality must be between {JpegConstants.MinQuality} and {JpegConstants.MaxQuality}.");

		// Validate color mode and samples per pixel
		var expectedSamples = jpeg.ColorMode switch
		{
			JpegColorMode.Grayscale => 1,
			JpegColorMode.Rgb => 3,
			JpegColorMode.YCbCr => 3,
			JpegColorMode.Cmyk => 4,
			_ => 0
		};

		if (expectedSamples == 0)
			result.AddError($"Invalid color mode: {jpeg.ColorMode}.");
		else if (jpeg.SamplesPerPixel != expectedSamples)
			result.AddError($"Invalid samples per pixel: {jpeg.SamplesPerPixel}. Expected {expectedSamples} for {jpeg.ColorMode} color mode.");

		// Validate bits per sample
		if (jpeg.BitsPerSample != JpegConstants.BitsPerSample)
			result.AddError($"Invalid bits per sample: {jpeg.BitsPerSample}. JPEG supports only {JpegConstants.BitsPerSample} bits per sample.");

		// Validate compression ratio
		if (jpeg.CompressionRatio <= 0)
			result.AddError($"Invalid compression ratio: {jpeg.CompressionRatio}. Compression ratio must be greater than 0.");

		// Validate encoding-specific constraints
		if (jpeg.Encoding == JpegEncoding.Jpeg2000)
			result.AddWarning("JPEG 2000 format has limited support in many applications.");

		// Validate chroma subsampling for color modes
		if (jpeg.ColorMode == JpegColorMode.Grayscale && jpeg.ChromaSubsampling != JpegChromaSubsampling.None)
			result.AddWarning("Chroma subsampling is not applicable for grayscale images.");

		return result;
	}

	/// <summary>Validates JPEG file signature.</summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data has a valid JPEG signature, false otherwise.</returns>
	public static bool IsValidJpegSignature(ReadOnlySpan<byte> data)
	{
		if (data.Length < 2) return false;
		
		// Check for SOI marker (Start of Image)
		return data[0] == 0xFF && data[1] == 0xD8;
	}

	/// <summary>Validates JPEG metadata.</summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>A validation result for the metadata.</returns>
	public static JpegValidationResult ValidateMetadata(JpegMetadata metadata)
	{
		ArgumentNullException.ThrowIfNull(metadata);

		var result = new JpegValidationResult();

		// Validate resolution values
		if (metadata.XResolution.HasValue && metadata.XResolution <= 0)
			result.AddError($"Invalid X resolution: {metadata.XResolution}. Resolution must be greater than 0.");

		if (metadata.YResolution.HasValue && metadata.YResolution <= 0)
			result.AddError($"Invalid Y resolution: {metadata.YResolution}. Resolution must be greater than 0.");

		// Validate GPS coordinates
		if (metadata.GpsLatitude.HasValue && (metadata.GpsLatitude < -90 || metadata.GpsLatitude > 90))
			result.AddError($"Invalid GPS latitude: {metadata.GpsLatitude}. Latitude must be between -90 and 90.");

		if (metadata.GpsLongitude.HasValue && (metadata.GpsLongitude < -180 || metadata.GpsLongitude > 180))
			result.AddError($"Invalid GPS longitude: {metadata.GpsLongitude}. Longitude must be between -180 and 180.");

		// Validate exposure time
		if (metadata.ExposureTime.HasValue && metadata.ExposureTime <= 0)
			result.AddError($"Invalid exposure time: {metadata.ExposureTime}. Exposure time must be greater than 0.");

		// Validate F-number
		if (metadata.FNumber.HasValue && metadata.FNumber <= 0)
			result.AddError($"Invalid F-number: {metadata.FNumber}. F-number must be greater than 0.");

		// Validate ISO speed rating
		if (metadata.IsoSpeedRating.HasValue && metadata.IsoSpeedRating <= 0)
			result.AddError($"Invalid ISO speed rating: {metadata.IsoSpeedRating}. ISO must be greater than 0.");

		// Validate focal length
		if (metadata.FocalLength.HasValue && metadata.FocalLength <= 0)
			result.AddError($"Invalid focal length: {metadata.FocalLength}. Focal length must be greater than 0.");

		return result;
	}
}

/// <summary>Represents the result of JPEG validation.</summary>
public class JpegValidationResult
{
	/// <summary>Gets a value indicating whether the validation passed.</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Gets the list of validation errors.</summary>
	public List<string> Errors { get; } = new();

	/// <summary>Gets the list of validation warnings.</summary>
	public List<string> Warnings { get; } = new();

	/// <summary>Adds an error to the validation result.</summary>
	/// <param name="error">The error message to add.</param>
	public void AddError(string error)
	{
		Errors.Add(error);
	}

	/// <summary>Adds a warning to the validation result.</summary>
	/// <param name="warning">The warning message to add.</param>
	public void AddWarning(string warning)
	{
		Warnings.Add(warning);
	}

	/// <summary>Gets a summary of all validation issues.</summary>
	/// <returns>A formatted string containing all errors and warnings.</returns>
	public string GetSummary()
	{
		var summary = new List<string>();
		
		if (Errors.Count > 0)
		{
			summary.Add($"Errors ({Errors.Count}):");
			summary.AddRange(Errors.Select(e => $"  - {e}"));
		}
		
		if (Warnings.Count > 0)
		{
			summary.Add($"Warnings ({Warnings.Count}):");
			summary.AddRange(Warnings.Select(w => $"  - {w}"));
		}
		
		return summary.Count > 0 ? string.Join(Environment.NewLine, summary) : "No validation issues found.";
	}
}