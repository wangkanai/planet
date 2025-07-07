// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Jpegs;

namespace Wangkanai.Graphics.Rasters.Jpegs;

public class JpegRasterTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaultValues()
	{
		// Arrange & Act
		var jpegRaster = new JpegRaster();

		// Assert
		Assert.Equal(JpegColorMode.Rgb, jpegRaster.ColorMode);
		Assert.Equal(JpegConstants.DefaultQuality, jpegRaster.Quality);
		Assert.Equal(JpegEncoding.Baseline, jpegRaster.Encoding);
		Assert.Equal(3, jpegRaster.SamplesPerPixel);
		Assert.Equal(JpegConstants.BitsPerSample, jpegRaster.BitsPerSample);
		Assert.Equal(JpegChromaSubsampling.Both, jpegRaster.ChromaSubsampling);
		Assert.False(jpegRaster.IsProgressive);
		Assert.False(jpegRaster.IsOptimized);
		Assert.Equal(10.0, jpegRaster.CompressionRatio);
		Assert.NotNull(jpegRaster.Metadata);
	}

	[Fact]
	public void Constructor_WithDimensions_ShouldSetWidthAndHeight()
	{
		// Arrange
		const int width = 1920;
		const int height = 1080;

		// Act
		var jpegRaster = new JpegRaster(width, height);

		// Assert
		Assert.Equal(width, jpegRaster.Width);
		Assert.Equal(height, jpegRaster.Height);
	}

	[Fact]
	public void Constructor_WithDimensionsAndQuality_ShouldSetAllProperties()
	{
		// Arrange
		const int width = 800;
		const int height = 600;
		const int quality = 85;

		// Act
		var jpegRaster = new JpegRaster(width, height, quality);

		// Assert
		Assert.Equal(width, jpegRaster.Width);
		Assert.Equal(height, jpegRaster.Height);
		Assert.Equal(quality, jpegRaster.Quality);
	}

	[Theory]
	[InlineData(JpegColorMode.Grayscale, 1)]
	[InlineData(JpegColorMode.Rgb, 3)]
	[InlineData(JpegColorMode.YCbCr, 3)]
	[InlineData(JpegColorMode.Cmyk, 4)]
	public void SetColorMode_ShouldUpdateColorModeAndSamplesPerPixel(JpegColorMode colorMode, int expectedSamples)
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act
		jpegRaster.SetColorMode(colorMode);

		// Assert
		Assert.Equal(colorMode, jpegRaster.ColorMode);
		Assert.Equal(expectedSamples, jpegRaster.SamplesPerPixel);
	}

	[Theory]
	[InlineData(0, 0, 50.0)]
	[InlineData(10, 10, 40.0)]
	[InlineData(25, 25, 30.0)]
	[InlineData(50, 50, 16.0)]
	[InlineData(75, 75, 8.0)]
	[InlineData(85, 85, 5.0)]
	[InlineData(95, 95, 3.0)]
	[InlineData(100, 100, 3.0)]
	public void SetQuality_ShouldUpdateQualityAndCompressionRatio(int inputQuality, int expectedQuality, double expectedRatio)
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act
		jpegRaster.SetQuality(inputQuality);

		// Assert
		Assert.Equal(expectedQuality, jpegRaster.Quality);
		Assert.Equal(expectedRatio, jpegRaster.CompressionRatio);
	}

	[Theory]
	[InlineData(-10, 0)] // Below minimum
	[InlineData(150, 100)] // Above maximum
	[InlineData(50, 50)] // Valid range
	public void SetQuality_ShouldClampToValidRange(int input, int expected)
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act
		jpegRaster.SetQuality(input);

		// Assert
		Assert.Equal(expected, jpegRaster.Quality);
	}

	[Theory]
	[InlineData(1920, 1080, 75, true)]
	[InlineData(0, 100, 75, false)] // Invalid width
	[InlineData(100, 0, 75, false)] // Invalid height
	[InlineData(65536, 100, 75, false)] // Width exceeds maximum
	[InlineData(100, 65536, 75, false)] // Height exceeds maximum
	[InlineData(100, 100, -1, false)] // Invalid quality
	[InlineData(100, 100, 101, false)] // Invalid quality
	public void IsValid_ShouldReturnCorrectValidationResult(int width, int height, int quality, bool expected)
	{
		// Arrange
		var jpegRaster = new JpegRaster(width, height, 75);
		if (quality != 75) // Set quality directly if it's different from constructor default to bypass clamping
		{
			jpegRaster.Quality = quality;
		}

		// Act
		var isValid = jpegRaster.IsValid();

		// Assert
		Assert.Equal(expected, isValid);
	}

	[Fact]
	public void GetEstimatedFileSize_WithValidImage_ShouldReturnPositiveValue()
	{
		// Arrange
		var jpegRaster = new JpegRaster(800, 600, 75);

		// Act
		var fileSize = jpegRaster.GetEstimatedFileSize();

		// Assert
		Assert.True(fileSize > 0);
	}

	[Fact]
	public void GetEstimatedFileSize_WithInvalidImage_ShouldReturnZero()
	{
		// Arrange
		var jpegRaster = new JpegRaster(0, 0, 75);

		// Act
		var fileSize = jpegRaster.GetEstimatedFileSize();

		// Assert
		Assert.Equal(0, fileSize);
	}

	[Theory]
	[InlineData(JpegColorMode.Grayscale, 8)]
	[InlineData(JpegColorMode.Rgb, 24)]
	[InlineData(JpegColorMode.YCbCr, 24)]
	[InlineData(JpegColorMode.Cmyk, 32)]
	public void GetColorDepth_ShouldReturnCorrectBitDepth(JpegColorMode colorMode, int expectedDepth)
	{
		// Arrange
		var jpegRaster = new JpegRaster();
		jpegRaster.SetColorMode(colorMode);

		// Act
		var colorDepth = jpegRaster.GetColorDepth();

		// Assert
		Assert.Equal(expectedDepth, colorDepth);
	}

	[Fact]
	public void Metadata_CanBeModified()
	{
		// Arrange
		var jpegRaster = new JpegRaster();
		var metadata = new JpegMetadata
		{
			ImageDescription = "Test JPEG Image",
			Make = "Test Camera",
			Model = "Test Model",
			Software = "Test Software",
			Artist = "Test Artist"
		};

		// Act
		jpegRaster.Metadata = metadata;

		// Assert
		Assert.Equal("Test JPEG Image", jpegRaster.Metadata.ImageDescription);
		Assert.Equal("Test Camera", jpegRaster.Metadata.Make);
		Assert.Equal("Test Model", jpegRaster.Metadata.Model);
		Assert.Equal("Test Software", jpegRaster.Metadata.Software);
		Assert.Equal("Test Artist", jpegRaster.Metadata.Artist);
	}

	[Fact]
	public void Properties_CanBeModified()
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act
		jpegRaster.Encoding = JpegEncoding.Progressive;
		jpegRaster.IsProgressive = true;
		jpegRaster.IsOptimized = true;
		jpegRaster.ChromaSubsampling = JpegChromaSubsampling.None;

		// Assert
		Assert.Equal(JpegEncoding.Progressive, jpegRaster.Encoding);
		Assert.True(jpegRaster.IsProgressive);
		Assert.True(jpegRaster.IsOptimized);
		Assert.Equal(JpegChromaSubsampling.None, jpegRaster.ChromaSubsampling);
	}

	[Fact]
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act & Assert
		var exception = Record.Exception(() => jpegRaster.Dispose());
		Assert.Null(exception);
	}

	[Fact]
	public void BitsPerSample_ShouldBe8Bits()
	{
		// Arrange
		var jpegRaster = new JpegRaster();

		// Act & Assert
		Assert.Equal(8, jpegRaster.BitsPerSample);
	}

	[Theory]
	[InlineData(1, 1)]
	[InlineData(65535, 65535)]
	[InlineData(1920, 1080)]
	public void MaximumDimensions_ShouldBeSupported(int width, int height)
	{
		// Arrange & Act
		var jpegRaster = new JpegRaster(width, height);

		// Assert
		Assert.True(jpegRaster.IsValid());
		Assert.Equal(width, jpegRaster.Width);
		Assert.Equal(height, jpegRaster.Height);
	}
}
