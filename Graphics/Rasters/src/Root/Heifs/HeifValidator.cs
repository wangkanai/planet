// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Provides validation functionality for HEIF raster images and encoding options.
/// </summary>
public static class HeifValidator
{
	/// <summary>
	/// Validates a HEIF raster image configuration.
	/// </summary>
	/// <param name="raster">The HEIF raster to validate.</param>
	/// <returns>Validation result with success status and any errors.</returns>
	public static HeifValidationResult ValidateRaster(IHeifRaster raster)
	{
		if (raster == null)
			return HeifValidationResult.CreateFailure("Raster cannot be null.");

		var errors = new List<string>();

		// Validate dimensions
		if (raster.Width <= 0)
			errors.Add("Width must be greater than 0.");
		
		if (raster.Height <= 0)
			errors.Add("Height must be greater than 0.");

		if (raster.Width > HeifConstants.MaxDimension)
			errors.Add($"Width cannot exceed {HeifConstants.MaxDimension} pixels.");

		if (raster.Height > HeifConstants.MaxDimension)
			errors.Add($"Height cannot exceed {HeifConstants.MaxDimension} pixels.");

		// Validate quality
		if (raster.Quality < HeifConstants.MinQuality || raster.Quality > HeifConstants.MaxQuality)
			errors.Add($"Quality must be between {HeifConstants.MinQuality} and {HeifConstants.MaxQuality}.");

		// Validate speed
		if (raster.Speed < HeifConstants.MinSpeed || raster.Speed > HeifConstants.MaxSpeed)
			errors.Add($"Speed must be between {HeifConstants.MinSpeed} and {HeifConstants.MaxSpeed}.");

		// Validate bit depth
		if (raster.BitDepth < HeifConstants.MinBitDepth || raster.BitDepth > HeifConstants.MaxBitDepth)
			errors.Add($"Bit depth must be between {HeifConstants.MinBitDepth} and {HeifConstants.MaxBitDepth}.");

		// Validate bit depth values
		if (raster.BitDepth != 8 && raster.BitDepth != 10 && raster.BitDepth != 12 && raster.BitDepth != 16)
			errors.Add("Bit depth must be 8, 10, 12, or 16.");

		// Validate thread count
		if (raster.ThreadCount < 0)
			errors.Add("Thread count cannot be negative.");

		if (raster.ThreadCount > HeifConstants.Memory.MaxThreads)
			errors.Add($"Thread count cannot exceed {HeifConstants.Memory.MaxThreads}.");

		// Validate lossless constraints
		if (raster.IsLossless && raster.Quality != HeifConstants.QualityPresets.Lossless)
			errors.Add("Lossless mode requires quality to be 100.");

		// Validate compression constraints
		if (raster.Compression == HeifCompression.Jpeg && raster.IsLossless)
			errors.Add("JPEG compression cannot be used with lossless mode.");

		// Validate profile constraints
		if (raster.Profile == HeifProfile.Main10 && raster.BitDepth < 10)
			errors.Add("Main 10 profile requires bit depth of at least 10.");

		if (raster.ChromaSubsampling == HeifChromaSubsampling.Yuv400 && raster.HasAlpha)
			errors.Add("Monochrome (YUV 4:0:0) subsampling cannot be used with alpha channel.");

		// Validate HDR constraints
		if (raster.HasHdrMetadata && raster.BitDepth < 10)
			errors.Add("HDR metadata requires bit depth of at least 10.");

		// Validate large image constraints
		var pixelCount = (long)raster.Width * raster.Height;
		if (pixelCount > HeifConstants.Memory.LargeImageThreshold && raster.ThreadCount == 1)
			errors.Add("Large images should use multiple threads for better performance.");

		return errors.Count == 0 
			? HeifValidationResult.CreateSuccess() 
			: HeifValidationResult.CreateFailure(errors);
	}

	/// <summary>
	/// Validates HEIF encoding options.
	/// </summary>
	/// <param name="options">The encoding options to validate.</param>
	/// <returns>Validation result with success status and any errors.</returns>
	public static HeifValidationResult ValidateEncodingOptions(HeifEncodingOptions options)
	{
		if (options == null)
			return HeifValidationResult.CreateFailure("Encoding options cannot be null.");

		if (!options.Validate(out var error))
			return HeifValidationResult.CreateFailure(error!);

		return HeifValidationResult.CreateSuccess();
	}

	/// <summary>
	/// Validates HEIF file data.
	/// </summary>
	/// <param name="data">The HEIF file data to validate.</param>
	/// <returns>Validation result with success status and any errors.</returns>
	public static HeifValidationResult ValidateFileData(byte[] data)
	{
		if (data == null)
			return HeifValidationResult.CreateFailure("File data cannot be null.");

		if (data.Length < HeifConstants.BoxHeaderSize)
			return HeifValidationResult.CreateFailure("File data is too small to be a valid HEIF file.");

		// Check for valid file type box
		if (data.Length >= 12)
		{
			// Check for 'ftyp' box at the beginning
			var hasValidHeader = data[4] == 'f' && data[5] == 't' && data[6] == 'y' && data[7] == 'p';
			if (!hasValidHeader)
				return HeifValidationResult.CreateFailure("File does not start with a valid HEIF file type box.");

			// Check for HEIF brands
			var hasHeifBrand = CheckForBrand(data, 8, "heic") || 
			                   CheckForBrand(data, 8, "heis") || 
			                   CheckForBrand(data, 8, "hevc") ||
			                   CheckForBrand(data, 8, "avif");

			if (!hasHeifBrand)
				return HeifValidationResult.CreateFailure("File does not contain a recognized HEIF brand.");
		}

		return HeifValidationResult.CreateSuccess();
	}

	/// <summary>
	/// Validates HEIF metadata.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Validation result with success status and any errors.</returns>
	public static HeifValidationResult ValidateMetadata(HeifMetadata metadata)
	{
		if (metadata == null)
			return HeifValidationResult.CreateFailure("Metadata cannot be null.");

		var errors = new List<string>();

		// Validate EXIF data
		if (metadata.ExifData != null && metadata.ExifData.Length > 0)
		{
			if (metadata.ExifData.Length < 6)
				errors.Add("EXIF data is too small to be valid.");
		}

		// Validate XMP data
		if (!string.IsNullOrEmpty(metadata.XmpData))
		{
			if (metadata.XmpData.Length < 10)
				errors.Add("XMP data is too small to be valid.");
		}

		// Validate ICC profile
		if (metadata.IccProfile != null && metadata.IccProfile.Length > 0)
		{
			if (metadata.IccProfile.Length < 128)
				errors.Add("ICC profile data is too small to be valid.");
		}

		// Validate GPS coordinates
		if (metadata.GpsCoordinates != null)
		{
			if (metadata.GpsCoordinates.Latitude < -90 || metadata.GpsCoordinates.Latitude > 90)
				errors.Add("GPS latitude must be between -90 and 90 degrees.");

			if (metadata.GpsCoordinates.Longitude < -180 || metadata.GpsCoordinates.Longitude > 180)
				errors.Add("GPS longitude must be between -180 and 180 degrees.");
		}

		// Validate camera settings
		if (metadata.CameraMetadata != null)
		{
			if (metadata.CameraMetadata.Aperture.HasValue && metadata.CameraMetadata.Aperture.Value <= 0)
				errors.Add("Aperture value must be positive.");

			if (metadata.CameraMetadata.ExposureTime.HasValue && metadata.CameraMetadata.ExposureTime.Value <= 0)
				errors.Add("Exposure time must be positive.");

			if (metadata.CameraMetadata.FocalLength.HasValue && metadata.CameraMetadata.FocalLength.Value <= 0)
				errors.Add("Focal length must be positive.");

			if (metadata.CameraMetadata.IsoSensitivity.HasValue && metadata.CameraMetadata.IsoSensitivity.Value <= 0)
				errors.Add("ISO sensitivity must be positive.");
				
			if (metadata.CameraMetadata.XResolution.HasValue && metadata.CameraMetadata.XResolution.Value <= 0)
				errors.Add("Pixel density must be positive.");
				
			if (metadata.CameraMetadata.YResolution.HasValue && metadata.CameraMetadata.YResolution.Value <= 0)
				errors.Add("Pixel density must be positive.");
		}

		return errors.Count == 0 
			? HeifValidationResult.CreateSuccess() 
			: HeifValidationResult.CreateFailure(errors);
	}

	/// <summary>
	/// Performs comprehensive validation of all HEIF components.
	/// </summary>
	/// <param name="raster">The HEIF raster to validate.</param>
	/// <param name="options">The encoding options to validate.</param>
	/// <returns>Validation result with success status and any errors.</returns>
	public static HeifValidationResult ValidateComplete(IHeifRaster raster, HeifEncodingOptions? options = null)
	{
		var errors = new List<string>();

		// Validate raster
		var rasterResult = ValidateRaster(raster);
		if (!rasterResult.IsValid)
			errors.AddRange(rasterResult.Errors);

		// Validate encoding options if provided
		if (options != null)
		{
			var optionsResult = ValidateEncodingOptions(options);
			if (!optionsResult.IsValid)
				errors.AddRange(optionsResult.Errors);
		}

		// Validate metadata
		var metadataResult = ValidateMetadata(raster.Metadata);
		if (!metadataResult.IsValid)
			errors.AddRange(metadataResult.Errors);

		return errors.Count == 0 
			? HeifValidationResult.CreateSuccess() 
			: HeifValidationResult.CreateFailure(errors);
	}

	private static bool CheckForBrand(byte[] data, int offset, string brand)
	{
		if (offset + 4 > data.Length)
			return false;

		var brandBytes = System.Text.Encoding.ASCII.GetBytes(brand);
		for (var i = 0; i < 4; i++)
		{
			if (data[offset + i] != brandBytes[i])
				return false;
		}

		return true;
	}
}