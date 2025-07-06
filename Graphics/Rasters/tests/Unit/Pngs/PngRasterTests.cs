// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Pngs;

public class PngRasterTests
{
	[Fact]
	public void Constructor_Default_ShouldSetDefaultValues()
	{
		// Act
		var png = new PngRaster();

		// Assert
		Assert.Equal(1, png.Width);
		Assert.Equal(1, png.Height);
		Assert.Equal(PngColorType.Truecolor, png.ColorType);
		Assert.Equal(8, png.BitDepth);
		Assert.Equal(PngCompression.Deflate, png.Compression);
		Assert.Equal(PngFilterMethod.Standard, png.FilterMethod);
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.Equal(6, png.CompressionLevel);
		Assert.NotNull(png.Metadata);
		Assert.False(png.UsesPalette);
		Assert.False(png.HasTransparency);
		Assert.False(png.HasAlphaChannel);
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1920, 1080)]
	[InlineData(100, 100)]
	public void Constructor_WithDimensions_ShouldSetCorrectDimensions(int width, int height)
	{
		// Act
		var png = new PngRaster(width, height);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
	}

	[Theory]
	[InlineData(0, 1)]
	[InlineData(-10, 1)]
	public void Constructor_WithInvalidDimensions_ShouldClampToMinimum(int width, int expectedWidth)
	{
		// Act
		var png = new PngRaster(width, 100);

		// Assert
		Assert.Equal(expectedWidth, png.Width);
	}

	[Theory]
	[InlineData(PngColorType.Grayscale, 1)]
	[InlineData(PngColorType.Truecolor, 3)]
	[InlineData(PngColorType.IndexedColor, 1)]
	[InlineData(PngColorType.GrayscaleWithAlpha, 2)]
	[InlineData(PngColorType.TruecolorWithAlpha, 4)]
	public void SamplesPerPixel_ShouldReturnCorrectValueForColorType(PngColorType colorType, int expectedSamples)
	{
		// Arrange
		var png = new PngRaster { ColorType = colorType };

		// Act & Assert
		Assert.Equal(expectedSamples, png.SamplesPerPixel);
	}

	[Theory]
	[InlineData(PngColorType.IndexedColor, true, false)]
	[InlineData(PngColorType.Truecolor, false, false)]
	[InlineData(PngColorType.GrayscaleWithAlpha, false, true)]
	[InlineData(PngColorType.TruecolorWithAlpha, false, true)]
	public void ColorType_ShouldUpdateDependentProperties(PngColorType colorType, bool expectedUsesPalette, bool expectedHasAlpha)
	{
		// Arrange
		var png = new PngRaster();

		// Act
		png.ColorType = colorType;

		// Assert
		Assert.Equal(expectedUsesPalette, png.UsesPalette);
		Assert.Equal(expectedHasAlpha, png.HasAlphaChannel);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(5, 5)]
	[InlineData(9, 9)]
	[InlineData(10, 9)]// Should clamp to max
	[InlineData(-1, 0)]// Should clamp to min
	public void CompressionLevel_ShouldClampToValidRange(int level, int expected)
	{
		// Arrange
		var png = new PngRaster();

		// Act
		png.CompressionLevel = level;

		// Assert
		Assert.Equal(expected, png.CompressionLevel);
	}

	[Theory]
	[InlineData(1, 1)]
	[InlineData(8, 8)]
	[InlineData(16, 16)]
	[InlineData(0, 1)]  // Should clamp to min
	[InlineData(32, 16)]// Should clamp to max
	public void BitDepth_ShouldClampToValidRange(byte depth, byte expected)
	{
		// Arrange
		var png = new PngRaster();

		// Act
		png.BitDepth = depth;

		// Assert
		Assert.Equal(expected, png.BitDepth);
	}

	[Fact]
	public void IsValid_WithValidData_ShouldReturnTrue()
	{
		// Arrange
		var png = new PngRaster(800, 600)
		          {
			          ColorType        = PngColorType.Truecolor,
			          BitDepth         = 8,
			          CompressionLevel = 6
		          };

		// Act & Assert
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(0, 600)]
	[InlineData(800, 0)]
	[InlineData(-1, 600)]
	[InlineData(800, -1)]
	public void IsValid_WithInvalidDimensions_ShouldReturnFalse(int width, int height)
	{
		// Arrange
		var png = new PngRaster { Width = width, Height = height };

		// Act & Assert
		Assert.False(png.IsValid());
	}

	[Fact]
	public void CompressionLevel_WithInvalidValues_ShouldClampToValidRange()
	{
		// Arrange
		var png = new PngRaster(800, 600);

		// Act & Assert - Test that clamping works and result is valid
		png.CompressionLevel = -1;
		Assert.Equal(0, png.CompressionLevel);
		Assert.True(png.IsValid());

		png.CompressionLevel = 10;
		Assert.Equal(9, png.CompressionLevel);
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(PngColorType.Grayscale, 8, 8)]          // 1 * 8
	[InlineData(PngColorType.Truecolor, 8, 24)]         // 3 * 8
	[InlineData(PngColorType.TruecolorWithAlpha, 8, 32)]// 4 * 8
	[InlineData(PngColorType.Truecolor, 16, 48)]        // 3 * 16
	public void GetColorDepth_ShouldReturnCorrectValue(PngColorType colorType, byte bitDepth, int expectedColorDepth)
	{
		// Arrange
		var png = new PngRaster { ColorType = colorType, BitDepth = bitDepth };

		// Act & Assert
		Assert.Equal(expectedColorDepth, png.GetColorDepth());
	}

	[Fact]
	public void GetEstimatedFileSize_WithValidData_ShouldReturnPositiveValue()
	{
		// Arrange
		var png = new PngRaster(100, 100)
		          {
			          ColorType        = PngColorType.Truecolor,
			          BitDepth         = 8,
			          CompressionLevel = 6
		          };

		// Act
		var fileSize = png.GetEstimatedFileSize();

		// Assert
		Assert.True(fileSize > 0);
	}

	[Fact]
	public void GetEstimatedFileSize_WithInvalidData_ShouldReturnZero()
	{
		// Arrange
		var png = new PngRaster { Width = 0, Height = 0 };

		// Act
		var fileSize = png.GetEstimatedFileSize();

		// Assert
		Assert.Equal(0, fileSize);
	}

	[Theory]
	[InlineData(0)]// No compression
	[InlineData(3)]// Low compression
	[InlineData(6)]// Medium compression
	[InlineData(9)]// High compression
	public void GetEstimatedFileSize_ShouldVaryWithCompressionLevel(int compressionLevel)
	{
		// Arrange
		var png1 = new PngRaster(100, 100) { CompressionLevel = compressionLevel };
		var png2 = new PngRaster(100, 100) { CompressionLevel = 0 };// No compression

		// Act
		var size1 = png1.GetEstimatedFileSize();
		var size2 = png2.GetEstimatedFileSize();

		// Assert
		if (compressionLevel == 0)
			Assert.True(size1 >= size2 * 0.9);// Account for overhead
		else
			Assert.True(size1 < size2);// Compressed should be smaller
	}

	[Fact]
	public void PaletteData_ShouldAcceptNullAndValidData()
	{
		// Arrange
		var png         = new PngRaster();
		var paletteData = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 };// RGB triplets

		// Act & Assert
		png.PaletteData = null;
		Assert.Null(png.PaletteData);

		png.PaletteData = paletteData;
		Assert.Equal(paletteData, png.PaletteData);
	}

	[Fact]
	public void TransparencyData_ShouldAcceptNullAndValidData()
	{
		// Arrange
		var png              = new PngRaster();
		var transparencyData = new byte[] { 255, 255 };// Grayscale transparency

		// Act & Assert
		png.TransparencyData = null;
		Assert.Null(png.TransparencyData);

		png.TransparencyData = transparencyData;
		Assert.Equal(transparencyData, png.TransparencyData);
	}

	[Fact]
	public void Dispose_ShouldClearResources()
	{
		// Arrange
		var png = new PngRaster();
		png.PaletteData                   = new byte[] { 1, 2, 3 };
		png.TransparencyData              = new byte[] { 4, 5 };
		png.Metadata.CustomChunks["test"] = new byte[] { 6, 7, 8 };

		// Act
		png.Dispose();

		// Assert
		Assert.Null(png.PaletteData);
		Assert.Null(png.TransparencyData);
		Assert.Empty(png.Metadata.CustomChunks);
	}

	[Theory]
	[InlineData(PngColorType.Grayscale, 1, true)]
	[InlineData(PngColorType.Grayscale, 2, true)]
	[InlineData(PngColorType.Grayscale, 4, true)]
	[InlineData(PngColorType.Grayscale, 8, true)]
	[InlineData(PngColorType.Grayscale, 16, true)]
	[InlineData(PngColorType.Grayscale, 3, false)]// Invalid
	[InlineData(PngColorType.Truecolor, 8, true)]
	[InlineData(PngColorType.Truecolor, 16, true)]
	[InlineData(PngColorType.Truecolor, 4, false)]// Invalid
	[InlineData(PngColorType.IndexedColor, 1, true)]
	[InlineData(PngColorType.IndexedColor, 2, true)]
	[InlineData(PngColorType.IndexedColor, 4, true)]
	[InlineData(PngColorType.IndexedColor, 8, true)]
	[InlineData(PngColorType.IndexedColor, 16, false)]// Invalid
	public void IsValid_ShouldValidateBitDepthForColorType(PngColorType colorType, byte bitDepth, bool expectedValid)
	{
		// Arrange
		var png = new PngRaster(100, 100)
		          {
			          ColorType = colorType,
			          BitDepth  = bitDepth
		          };

		// Act & Assert
		Assert.Equal(expectedValid, png.IsValid());
	}
}
