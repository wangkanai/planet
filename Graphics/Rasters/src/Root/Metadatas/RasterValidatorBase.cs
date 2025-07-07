// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Provides base validation functionality for raster image formats.
/// </summary>
public static class RasterValidatorBase
{
	/// <summary>
	/// Validates image dimensions.
	/// </summary>
	/// <param name="width">The image width to validate.</param>
	/// <param name="height">The image height to validate.</param>
	/// <param name="minDimension">The minimum allowed dimension.</param>
	/// <param name="maxDimension">The maximum allowed dimension.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the dimensions are valid, false otherwise.</returns>
	public static bool ValidateDimensions(int width, int height, int minDimension, int maxDimension, out string? error)
	{
		error = null;

		if (width < minDimension)
		{
			error = $"Invalid width: {width}. Width must be at least {minDimension}.";
			return false;
		}

		if (height < minDimension)
		{
			error = $"Invalid height: {height}. Height must be at least {minDimension}.";
			return false;
		}

		if (width > maxDimension)
		{
			error = $"Width exceeds maximum: {width} > {maxDimension}.";
			return false;
		}

		if (height > maxDimension)
		{
			error = $"Height exceeds maximum: {height} > {maxDimension}.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates bit depth for a specific format.
	/// </summary>
	/// <param name="bitDepth">The bit depth to validate.</param>
	/// <param name="validBitDepths">Array of valid bit depths for the format.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the bit depth is valid, false otherwise.</returns>
	public static bool ValidateBitDepth(int bitDepth, int[] validBitDepths, out string? error)
	{
		error = null;

		if (!validBitDepths.Contains(bitDepth))
		{
			error = $"Invalid bit depth: {bitDepth}. Valid bit depths are: {string.Join(", ", validBitDepths)}.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates quality setting.
	/// </summary>
	/// <param name="quality">The quality value to validate.</param>
	/// <param name="minQuality">The minimum allowed quality.</param>
	/// <param name="maxQuality">The maximum allowed quality.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the quality is valid, false otherwise.</returns>
	public static bool ValidateQuality(int quality, int minQuality, int maxQuality, out string? error)
	{
		error = null;

		if (quality < minQuality || quality > maxQuality)
		{
			error = $"Invalid quality: {quality}. Quality must be between {minQuality} and {maxQuality}.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates speed setting.
	/// </summary>
	/// <param name="speed">The speed value to validate.</param>
	/// <param name="minSpeed">The minimum allowed speed.</param>
	/// <param name="maxSpeed">The maximum allowed speed.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the speed is valid, false otherwise.</returns>
	public static bool ValidateSpeed(int speed, int minSpeed, int maxSpeed, out string? error)
	{
		error = null;

		if (speed < minSpeed || speed > maxSpeed)
		{
			error = $"Invalid speed: {speed}. Speed must be between {minSpeed} and {maxSpeed}.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates memory constraints.
	/// </summary>
	/// <param name="bufferSizeMB">The buffer size in megabytes to validate.</param>
	/// <param name="minSizeMB">The minimum allowed size in megabytes.</param>
	/// <param name="maxSizeMB">The maximum allowed size in megabytes.</param>
	/// <param name="bufferType">The type of buffer being validated.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the memory constraint is valid, false otherwise.</returns>
	public static bool ValidateMemoryConstraints(int bufferSizeMB, int minSizeMB, int maxSizeMB, string bufferType, out string? error)
	{
		error = null;

		if (bufferSizeMB < minSizeMB || bufferSizeMB > maxSizeMB)
		{
			error = $"Invalid {bufferType} buffer size: {bufferSizeMB}MB. Must be between {minSizeMB}MB and {maxSizeMB}MB.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates file signature for a specific format.
	/// </summary>
	/// <param name="data">The file data to validate.</param>
	/// <param name="signature">The expected file signature.</param>
	/// <returns>True if the data has the expected signature, false otherwise.</returns>
	public static bool ValidateFileSignature(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
	{
		if (data.Length < signature.Length)
			return false;

		return data[..signature.Length].SequenceEqual(signature);
	}

	/// <summary>
	/// Validates file signature with multiple possible signatures.
	/// </summary>
	/// <param name="data">The file data to validate.</param>
	/// <param name="signatures">Array of possible file signatures.</param>
	/// <returns>True if the data matches any of the signatures, false otherwise.</returns>
	public static bool ValidateFileSignature(ReadOnlySpan<byte> data, params byte[][] signatures)
	{
		foreach (var signature in signatures)
		{
			if (ValidateFileSignature(data, signature))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Validates resolution values.
	/// </summary>
	/// <param name="xResolution">The X resolution to validate.</param>
	/// <param name="yResolution">The Y resolution to validate.</param>
	/// <param name="minResolution">The minimum allowed resolution.</param>
	/// <param name="maxResolution">The maximum allowed resolution.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the resolutions are valid, false otherwise.</returns>
	public static bool ValidateResolution(double? xResolution, double? yResolution, double minResolution, double maxResolution, out string? error)
	{
		error = null;

		if (xResolution.HasValue)
		{
			if (xResolution <= 0 || xResolution < minResolution || xResolution > maxResolution)
			{
				error = $"Invalid X resolution: {xResolution}. Resolution must be between {minResolution} and {maxResolution}.";
				return false;
			}
		}

		if (yResolution.HasValue)
		{
			if (yResolution <= 0 || yResolution < minResolution || yResolution > maxResolution)
			{
				error = $"Invalid Y resolution: {yResolution}. Resolution must be between {minResolution} and {maxResolution}.";
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Validates GPS coordinates.
	/// </summary>
	/// <param name="latitude">The latitude to validate.</param>
	/// <param name="longitude">The longitude to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the coordinates are valid, false otherwise.</returns>
	public static bool ValidateGpsCoordinates(double? latitude, double? longitude, out string? error)
	{
		error = null;

		if (latitude.HasValue && (latitude < -90 || latitude > 90))
		{
			error = $"Invalid GPS latitude: {latitude}. Latitude must be between -90 and 90.";
			return false;
		}

		if (longitude.HasValue && (longitude < -180 || longitude > 180))
		{
			error = $"Invalid GPS longitude: {longitude}. Longitude must be between -180 and 180.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates thread count.
	/// </summary>
	/// <param name="threadCount">The thread count to validate.</param>
	/// <param name="maxThreads">The maximum allowed threads.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the thread count is valid, false otherwise.</returns>
	public static bool ValidateThreadCount(int threadCount, int maxThreads, out string? error)
	{
		error = null;

		if (threadCount < 0 || threadCount > maxThreads)
		{
			error = $"Invalid thread count: {threadCount}. Must be between 0 and {maxThreads}.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates compression ratio.
	/// </summary>
	/// <param name="compressionRatio">The compression ratio to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the compression ratio is valid, false otherwise.</returns>
	public static bool ValidateCompressionRatio(double compressionRatio, out string? error)
	{
		error = null;

		if (compressionRatio <= 0)
		{
			error = $"Invalid compression ratio: {compressionRatio}. Compression ratio must be greater than 0.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates file size constraints.
	/// </summary>
	/// <param name="fileSize">The file size in bytes.</param>
	/// <param name="maxFileSize">The maximum allowed file size in bytes.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the file size is valid, false otherwise.</returns>
	public static bool ValidateFileSize(long fileSize, long maxFileSize, out string? error)
	{
		error = null;

		if (fileSize > maxFileSize)
		{
			error = $"File size exceeds maximum: {fileSize:N0} bytes > {maxFileSize:N0} bytes.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates exposure time.
	/// </summary>
	/// <param name="exposureTime">The exposure time to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the exposure time is valid, false otherwise.</returns>
	public static bool ValidateExposureTime(double? exposureTime, out string? error)
	{
		error = null;

		if (exposureTime.HasValue && exposureTime <= 0)
		{
			error = $"Invalid exposure time: {exposureTime}. Exposure time must be greater than 0.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates F-number (aperture).
	/// </summary>
	/// <param name="fNumber">The F-number to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the F-number is valid, false otherwise.</returns>
	public static bool ValidateFNumber(double? fNumber, out string? error)
	{
		error = null;

		if (fNumber.HasValue && fNumber <= 0)
		{
			error = $"Invalid F-number: {fNumber}. F-number must be greater than 0.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates ISO speed rating.
	/// </summary>
	/// <param name="isoSpeed">The ISO speed to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the ISO speed is valid, false otherwise.</returns>
	public static bool ValidateIsoSpeed(int? isoSpeed, out string? error)
	{
		error = null;

		if (isoSpeed.HasValue && isoSpeed <= 0)
		{
			error = $"Invalid ISO speed rating: {isoSpeed}. ISO must be greater than 0.";
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates focal length.
	/// </summary>
	/// <param name="focalLength">The focal length to validate.</param>
	/// <param name="error">The error message if validation fails.</param>
	/// <returns>True if the focal length is valid, false otherwise.</returns>
	public static bool ValidateFocalLength(double? focalLength, out string? error)
	{
		error = null;

		if (focalLength.HasValue && focalLength <= 0)
		{
			error = $"Invalid focal length: {focalLength}. Focal length must be greater than 0.";
			return false;
		}

		return true;
	}
}