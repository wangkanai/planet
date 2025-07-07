// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Provides validation functionality for JPEG raster images.</summary>
public static class JpegValidator
{
	/// <summary>Validates a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <returns>A validation result indicating if the image is valid and any errors.</returns>
	public static JpegValidationResult Validate(this IJpegRaster jpeg)
	{
		ArgumentNullException.ThrowIfNull(jpeg);

		var result = new JpegValidationResult();

		// TODO: Validate the Jpeg raster properties and the metadata in 2 separate methods.
		jpeg.ValidateDimensions(result);
		jpeg.ValidateQuality(result);
		jpeg.ValidateColorModeAndSamples(result);
		jpeg.ValidateBitsPerSample(result);
		jpeg.ValidateCompressionRatio(result);
		jpeg.ValidateEncodingConstraints(result);
		jpeg.ValidateChromaSubsampling(result);

		jpeg.Metadata.ValidateMetadata();

		return result;
	}

	/// <summary>Validates the dimensions of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateDimensions(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.Width <= 0)
			result.AddError($"Invalid width: {jpeg.Width}. Width must be greater than 0.");

		if (jpeg.Height <= 0)
			result.AddError($"Invalid height: {jpeg.Height}. Height must be greater than 0.");

		if (jpeg.Width > JpegConstants.MaxDimension)
			result.AddError($"Width exceeds maximum: {jpeg.Width} > {JpegConstants.MaxDimension}.");

		if (jpeg.Height > JpegConstants.MaxDimension)
			result.AddError($"Height exceeds maximum: {jpeg.Height} > {JpegConstants.MaxDimension}.");
	}

	/// <summary>Validates the quality setting of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateQuality(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.Quality < JpegConstants.MinQuality || jpeg.Quality > JpegConstants.MaxQuality)
			result.AddError($"Invalid quality: {jpeg.Quality}. Quality must be between {JpegConstants.MinQuality} and {JpegConstants.MaxQuality}.");
	}

	/// <summary>Validates the color mode and samples per pixel of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateColorModeAndSamples(this IJpegRaster jpeg, JpegValidationResult result)
	{
		var expectedSamples = jpeg.ColorMode switch
		{
			JpegColorMode.Grayscale => 1,
			JpegColorMode.Rgb       => 3,
			JpegColorMode.YCbCr     => 3,
			JpegColorMode.Cmyk      => 4,
			_                       => 0
		};

		if (expectedSamples == 0)
			result.AddError($"Invalid color mode: {jpeg.ColorMode}.");
		else if (jpeg.SamplesPerPixel != expectedSamples)
			result.AddError($"Invalid samples per pixel: {jpeg.SamplesPerPixel}. Expected {expectedSamples} for {jpeg.ColorMode} color mode.");
	}

	/// <summary>Validates the bits per sample of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateBitsPerSample(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.BitsPerSample != JpegConstants.BitsPerSample)
			result.AddError($"Invalid bits per sample: {jpeg.BitsPerSample}. JPEG supports only {JpegConstants.BitsPerSample} bits per sample.");
	}

	/// <summary>Validates the compression ratio of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateCompressionRatio(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.CompressionRatio <= 0)
			result.AddError($"Invalid compression ratio: {jpeg.CompressionRatio}. Compression ratio must be greater than 0.");
	}

	/// <summary>Validates the encoding constraints of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateEncodingConstraints(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.Encoding == JpegEncoding.Jpeg2000)
			result.AddWarning("JPEG 2000 format has limited support in many applications.");
	}

	/// <summary>Validates the chroma subsampling of a JPEG raster image.</summary>
	/// <param name="jpeg">The JPEG raster to validate.</param>
	/// <param name="result">The validation result to add errors to.</param>
	private static void ValidateChromaSubsampling(this IJpegRaster jpeg, JpegValidationResult result)
	{
		if (jpeg.ColorMode == JpegColorMode.Grayscale && jpeg.ChromaSubsampling != JpegChromaSubsampling.None)
			result.AddWarning("Chroma subsampling is not applicable for grayscale images.");
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
	public static JpegValidationResult ValidateMetadata(this JpegMetadata metadata)
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
