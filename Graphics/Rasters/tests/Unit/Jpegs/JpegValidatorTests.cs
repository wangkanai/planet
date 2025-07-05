// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Jpegs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Jpegs;

public class JpegValidatorTests
{
	[Fact]
	public void Validate_WithValidJpeg_ShouldReturnValidResult()
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithNullJpeg_ShouldThrowArgumentNullException()
	{
		// Arrange
		IJpegRaster jpeg = null!;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => JpegValidator.Validate(jpeg));
	}

	[Theory]
	[InlineData(0, "Invalid width: 0. Width must be greater than 0.")]
	[InlineData(-100, "Invalid width: -100. Width must be greater than 0.")]
	public void Validate_WithInvalidWidth_ShouldReturnError(int width, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(width, 600, 75);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(0, "Invalid height: 0. Height must be greater than 0.")]
	[InlineData(-100, "Invalid height: -100. Height must be greater than 0.")]
	public void Validate_WithInvalidHeight_ShouldReturnError(int height, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(800, height, 75);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(65536, "Width exceeds maximum: 65536 > 65535.")]
	[InlineData(100000, "Width exceeds maximum: 100000 > 65535.")]
	public void Validate_WithWidthExceedingMaximum_ShouldReturnError(int width, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(width, 600, 75);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(65536, "Height exceeds maximum: 65536 > 65535.")]
	[InlineData(100000, "Height exceeds maximum: 100000 > 65535.")]
	public void Validate_WithHeightExceedingMaximum_ShouldReturnError(int height, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(800, height, 75);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1, "Invalid quality: -1. Quality must be between 0 and 100.")]
	[InlineData(101, "Invalid quality: 101. Quality must be between 0 and 100.")]
	[InlineData(200, "Invalid quality: 200. Quality must be between 0 and 100.")]
	public void Validate_WithInvalidQuality_ShouldReturnError(int quality, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.Quality = quality; // Set invalid quality directly to bypass constructor clamping

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(JpegColorMode.Grayscale, 1)]
	[InlineData(JpegColorMode.Rgb, 3)]
	[InlineData(JpegColorMode.YCbCr, 3)]
	[InlineData(JpegColorMode.Cmyk, 4)]
	public void Validate_WithValidColorModeAndSamples_ShouldReturnValid(JpegColorMode colorMode, int samplesPerPixel)
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.SetColorMode(colorMode);

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.True(result.IsValid);
		Assert.Equal(samplesPerPixel, jpeg.SamplesPerPixel);
	}

	[Fact]
	public void Validate_WithMismatchedSamplesPerPixel_ShouldReturnError()
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.ColorMode = JpegColorMode.Rgb;
		jpeg.SamplesPerPixel = 1; // Should be 3 for RGB

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains("Invalid samples per pixel: 1. Expected 3 for Rgb color mode.", result.Errors);
	}

	[Theory]
	[InlineData(7, "Invalid bits per sample: 7. JPEG supports only 8 bits per sample.")]
	[InlineData(16, "Invalid bits per sample: 16. JPEG supports only 8 bits per sample.")]
	public void Validate_WithInvalidBitsPerSample_ShouldReturnError(int bitsPerSample, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.BitsPerSample = bitsPerSample;

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(0, "Invalid compression ratio: 0. Compression ratio must be greater than 0.")]
	[InlineData(-5, "Invalid compression ratio: -5. Compression ratio must be greater than 0.")]
	public void Validate_WithInvalidCompressionRatio_ShouldReturnError(double compressionRatio, string expectedError)
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.CompressionRatio = compressionRatio;

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Fact]
	public void Validate_WithJpeg2000_ShouldReturnWarning()
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.Encoding = JpegEncoding.Jpeg2000;

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.True(result.IsValid);
		Assert.Contains("JPEG 2000 format has limited support in many applications.", result.Warnings);
	}

	[Fact]
	public void Validate_WithGrayscaleAndChromaSubsampling_ShouldReturnWarning()
	{
		// Arrange
		var jpeg = new JpegRaster(800, 600, 75);
		jpeg.SetColorMode(JpegColorMode.Grayscale);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.Both;

		// Act
		var result = JpegValidator.Validate(jpeg);

		// Assert
		Assert.True(result.IsValid);
		Assert.Contains("Chroma subsampling is not applicable for grayscale images.", result.Warnings);
	}

	[Theory]
	[InlineData(new byte[] { 0xFF, 0xD8 }, true)]
	[InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, true)]
	[InlineData(new byte[] { 0x89, 0x50 }, false)] // PNG signature
	[InlineData(new byte[] { 0x47, 0x49 }, false)] // GIF signature
	[InlineData(new byte[] { 0xFF }, false)] // Too short
	[InlineData(new byte[] { }, false)] // Empty
	public void IsValidJpegSignature_ShouldReturnCorrectResult(byte[] data, bool expected)
	{
		// Act
		var result = JpegValidator.IsValidJpegSignature(data);

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void ValidateMetadata_WithValidMetadata_ShouldReturnValid()
	{
		// Arrange
		var metadata = new JpegMetadata
		{
			XResolution = 300.0,
			YResolution = 300.0,
			GpsLatitude = 35.6762,
			GpsLongitude = 139.6503,
			ExposureTime = 1.0 / 125.0,
			FNumber = 5.6,
			IsoSpeedRating = 400,
			FocalLength = 85.0
		};

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void ValidateMetadata_WithNullMetadata_ShouldThrowArgumentNullException()
	{
		// Arrange
		JpegMetadata metadata = null!;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => JpegValidator.ValidateMetadata(metadata));
	}

	[Theory]
	[InlineData(-1.0, "Invalid X resolution: -1. Resolution must be greater than 0.")]
	[InlineData(0.0, "Invalid X resolution: 0. Resolution must be greater than 0.")]
	public void ValidateMetadata_WithInvalidXResolution_ShouldReturnError(double xResolution, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { XResolution = xResolution };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1.0, "Invalid Y resolution: -1. Resolution must be greater than 0.")]
	[InlineData(0.0, "Invalid Y resolution: 0. Resolution must be greater than 0.")]
	public void ValidateMetadata_WithInvalidYResolution_ShouldReturnError(double yResolution, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { YResolution = yResolution };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-91.0, "Invalid GPS latitude: -91. Latitude must be between -90 and 90.")]
	[InlineData(91.0, "Invalid GPS latitude: 91. Latitude must be between -90 and 90.")]
	public void ValidateMetadata_WithInvalidGpsLatitude_ShouldReturnError(double latitude, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { GpsLatitude = latitude };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-181.0, "Invalid GPS longitude: -181. Longitude must be between -180 and 180.")]
	[InlineData(181.0, "Invalid GPS longitude: 181. Longitude must be between -180 and 180.")]
	public void ValidateMetadata_WithInvalidGpsLongitude_ShouldReturnError(double longitude, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { GpsLongitude = longitude };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1.0, "Invalid exposure time: -1. Exposure time must be greater than 0.")]
	[InlineData(0.0, "Invalid exposure time: 0. Exposure time must be greater than 0.")]
	public void ValidateMetadata_WithInvalidExposureTime_ShouldReturnError(double exposureTime, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { ExposureTime = exposureTime };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1.0, "Invalid F-number: -1. F-number must be greater than 0.")]
	[InlineData(0.0, "Invalid F-number: 0. F-number must be greater than 0.")]
	public void ValidateMetadata_WithInvalidFNumber_ShouldReturnError(double fNumber, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { FNumber = fNumber };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1, "Invalid ISO speed rating: -1. ISO must be greater than 0.")]
	[InlineData(0, "Invalid ISO speed rating: 0. ISO must be greater than 0.")]
	public void ValidateMetadata_WithInvalidIsoSpeedRating_ShouldReturnError(int isoSpeedRating, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { IsoSpeedRating = isoSpeedRating };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}

	[Theory]
	[InlineData(-1.0, "Invalid focal length: -1. Focal length must be greater than 0.")]
	[InlineData(0.0, "Invalid focal length: 0. Focal length must be greater than 0.")]
	public void ValidateMetadata_WithInvalidFocalLength_ShouldReturnError(double focalLength, string expectedError)
	{
		// Arrange
		var metadata = new JpegMetadata { FocalLength = focalLength };

		// Act
		var result = JpegValidator.ValidateMetadata(metadata);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(expectedError, result.Errors);
	}
}

public class JpegValidationResultTests
{
	[Fact]
	public void Constructor_ShouldInitializeEmptyCollections()
	{
		// Act
		var result = new JpegValidationResult();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
		Assert.Empty(result.Warnings);
	}

	[Fact]
	public void AddError_ShouldAddErrorAndMakeInvalid()
	{
		// Arrange
		var result = new JpegValidationResult();
		const string error = "Test error";

		// Act
		result.AddError(error);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(error, result.Errors);
	}

	[Fact]
	public void AddWarning_ShouldAddWarningButRemainValid()
	{
		// Arrange
		var result = new JpegValidationResult();
		const string warning = "Test warning";

		// Act
		result.AddWarning(warning);

		// Assert
		Assert.True(result.IsValid);
		Assert.Contains(warning, result.Warnings);
	}

	[Fact]
	public void GetSummary_WithNoIssues_ShouldReturnNoIssuesMessage()
	{
		// Arrange
		var result = new JpegValidationResult();

		// Act
		var summary = result.GetSummary();

		// Assert
		Assert.Equal("No validation issues found.", summary);
	}

	[Fact]
	public void GetSummary_WithErrorsAndWarnings_ShouldReturnFormattedSummary()
	{
		// Arrange
		var result = new JpegValidationResult();
		result.AddError("Error 1");
		result.AddError("Error 2");
		result.AddWarning("Warning 1");

		// Act
		var summary = result.GetSummary();

		// Assert
		Assert.Contains("Errors (2):", summary);
		Assert.Contains("  - Error 1", summary);
		Assert.Contains("  - Error 2", summary);
		Assert.Contains("Warnings (1):", summary);
		Assert.Contains("  - Warning 1", summary);
	}

	[Fact]
	public void GetSummary_WithOnlyErrors_ShouldReturnOnlyErrors()
	{
		// Arrange
		var result = new JpegValidationResult();
		result.AddError("Error 1");

		// Act
		var summary = result.GetSummary();

		// Assert
		Assert.Contains("Errors (1):", summary);
		Assert.Contains("  - Error 1", summary);
		Assert.DoesNotContain("Warnings", summary);
	}

	[Fact]
	public void GetSummary_WithOnlyWarnings_ShouldReturnOnlyWarnings()
	{
		// Arrange
		var result = new JpegValidationResult();
		result.AddWarning("Warning 1");

		// Act
		var summary = result.GetSummary();

		// Assert
		Assert.Contains("Warnings (1):", summary);
		Assert.Contains("  - Warning 1", summary);
		Assert.DoesNotContain("Errors", summary);
	}
}
