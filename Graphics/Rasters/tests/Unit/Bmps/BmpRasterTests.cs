// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

public class BmpRasterTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaultValues()
	{
		// Act
		var bmp = new BmpRaster();

		// Assert
		Assert.Equal(BmpColorDepth.TwentyFourBit, bmp.ColorDepth);
		Assert.Equal(BmpCompression.Rgb, bmp.Compression);
		Assert.Equal(BmpConstants.DefaultHorizontalResolution, bmp.HorizontalResolution);
		Assert.Equal(BmpConstants.DefaultVerticalResolution, bmp.VerticalResolution);
		Assert.NotNull(bmp.Metadata);
		Assert.Null(bmp.ColorPalette);
	}

	[Fact]
	public void Constructor_WithDimensions_ShouldSetWidthAndHeight()
	{
		// Arrange
		const int width = 800;
		const int height = 600;

		// Act
		var bmp = new BmpRaster(width, height);

		// Assert
		Assert.Equal(width, bmp.Width);
		Assert.Equal(height, bmp.Height);
		Assert.Equal(width, bmp.Metadata.Width);
		Assert.Equal(height, bmp.Metadata.Height);
	}

	[Fact]
	public void Constructor_WithColorDepth_ShouldSetColorDepthAndMetadata()
	{
		// Arrange
		const int width = 800;
		const int height = 600;
		const BmpColorDepth colorDepth = BmpColorDepth.EightBit;

		// Act
		var bmp = new BmpRaster(width, height, colorDepth);

		// Assert
		Assert.Equal(colorDepth, bmp.ColorDepth);
		Assert.Equal((ushort)colorDepth, bmp.Metadata.BitsPerPixel);
	}

	[Theory]
	[InlineData(BmpColorDepth.Monochrome, true)]
	[InlineData(BmpColorDepth.FourBit, true)]
	[InlineData(BmpColorDepth.EightBit, true)]
	[InlineData(BmpColorDepth.SixteenBit, false)]
	[InlineData(BmpColorDepth.TwentyFourBit, false)]
	[InlineData(BmpColorDepth.ThirtyTwoBit, false)]
	public void HasPalette_ShouldReturnCorrectValue(BmpColorDepth colorDepth, bool expectedHasPalette)
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, colorDepth);

		// Act
		var hasPalette = bmp.HasPalette;

		// Assert
		Assert.Equal(expectedHasPalette, hasPalette);
	}

	[Theory]
	[InlineData(BmpColorDepth.ThirtyTwoBit, true)]
	[InlineData(BmpColorDepth.TwentyFourBit, false)]
	[InlineData(BmpColorDepth.SixteenBit, false)]
	public void HasTransparency_ShouldReturnCorrectValue(BmpColorDepth colorDepth, bool expectedHasTransparency)
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, colorDepth);

		// Act
		var hasTransparency = bmp.HasTransparency;

		// Assert
		Assert.Equal(expectedHasTransparency, hasTransparency);
	}

	[Fact]
	public void HasTransparency_ShouldReturnTrueForBitFieldsWithAlphaMask()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.SixteenBit);
		bmp.SetBitMasks(0x7C00, 0x03E0, 0x001F, 0x8000); // With alpha mask

		// Act
		var hasTransparency = bmp.HasTransparency;

		// Assert
		Assert.True(hasTransparency);
	}

	[Fact]
	public void IsTopDown_ShouldReturnTrueForNegativeHeight()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.Height = -100;

		// Act
		var isTopDown = bmp.IsTopDown;

		// Assert
		Assert.True(isTopDown);
	}

	[Theory]
	[InlineData(BmpColorDepth.EightBit, 1)]
	[InlineData(BmpColorDepth.SixteenBit, 2)]
	[InlineData(BmpColorDepth.TwentyFourBit, 3)]
	[InlineData(BmpColorDepth.ThirtyTwoBit, 4)]
	public void BytesPerPixel_ShouldReturnCorrectValue(BmpColorDepth colorDepth, int expectedBytes)
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, colorDepth);

		// Act
		var bytesPerPixel = bmp.BytesPerPixel;

		// Assert
		Assert.Equal(expectedBytes, bytesPerPixel);
	}

	[Theory]
	[InlineData(BmpColorDepth.Monochrome, 0)]
	[InlineData(BmpColorDepth.FourBit, 0)]
	public void BytesPerPixel_ShouldReturnZeroForPackedFormats(BmpColorDepth colorDepth, int expectedBytes)
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, colorDepth);

		// Act
		var bytesPerPixel = bmp.BytesPerPixel;

		// Assert
		Assert.Equal(expectedBytes, bytesPerPixel);
	}

	[Fact]
	public void RowStride_ShouldCalculateCorrectly()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);

		// Act
		var rowStride = bmp.RowStride;

		// Assert - 100 * 24 bits = 300 bytes, already aligned
		Assert.Equal(300, rowStride);
	}

	[Fact]
	public void PixelDataSize_ShouldCalculateCorrectly()
	{
		// Arrange
		var bmp = new BmpRaster(100, 50, BmpColorDepth.TwentyFourBit);

		// Act
		var pixelDataSize = bmp.PixelDataSize;

		// Assert - Row stride 300 * 50 rows = 15000
		Assert.Equal(15000u, pixelDataSize);
	}

	[Fact]
	public void GetBitMasks_ShouldReturnCorrectDefaultsFor16Bit()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.SixteenBit);

		// Act
		var (red, green, blue, alpha) = bmp.GetBitMasks();

		// Assert - Default RGB555 masks
		Assert.Equal(BmpConstants.RGB555Masks.Red, red);
		Assert.Equal(BmpConstants.RGB555Masks.Green, green);
		Assert.Equal(BmpConstants.RGB555Masks.Blue, blue);
		Assert.Equal(BmpConstants.RGB555Masks.Alpha, alpha);
	}

	[Fact]
	public void GetBitMasks_ShouldReturnCorrectDefaultsFor32Bit()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.ThirtyTwoBit);

		// Act
		var (red, green, blue, alpha) = bmp.GetBitMasks();

		// Assert - Default ARGB8888 masks
		Assert.Equal(BmpConstants.ARGB8888Masks.Red, red);
		Assert.Equal(BmpConstants.ARGB8888Masks.Green, green);
		Assert.Equal(BmpConstants.ARGB8888Masks.Blue, blue);
		Assert.Equal(BmpConstants.ARGB8888Masks.Alpha, alpha);
	}

	[Fact]
	public void SetBitMasks_ShouldSetCustomMasks()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.SixteenBit);
		const uint redMask = 0xF800;
		const uint greenMask = 0x07E0;
		const uint blueMask = 0x001F;

		// Act
		bmp.SetBitMasks(redMask, greenMask, blueMask);

		// Assert
		Assert.Equal(BmpCompression.BitFields, bmp.Compression);
		var (red, green, blue, alpha) = bmp.GetBitMasks();
		Assert.Equal(redMask, red);
		Assert.Equal(greenMask, green);
		Assert.Equal(blueMask, blue);
		Assert.Equal(0u, alpha);
	}

	[Fact]
	public void SetBitMasks_ShouldThrowForUnsupportedColorDepth()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => bmp.SetBitMasks(0xFF0000, 0x00FF00, 0x0000FF));
	}

	[Fact]
	public void ConvertToRgb_ShouldConvertToRgb24()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.ThirtyTwoBit);
		bmp.SetBitMasks(0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);

		// Act
		bmp.ConvertToRgb();

		// Assert
		Assert.Equal(BmpColorDepth.TwentyFourBit, bmp.ColorDepth);
		Assert.Equal(BmpCompression.Rgb, bmp.Compression);
		Assert.Equal(0u, bmp.Metadata.RedMask);
		Assert.Equal(0u, bmp.Metadata.GreenMask);
		Assert.Equal(0u, bmp.Metadata.BlueMask);
		Assert.Equal(0u, bmp.Metadata.AlphaMask);
	}

	[Fact]
	public void ApplyPalette_ShouldSetPalette()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		var palette = new byte[256 * 4]; // 256 colors * 4 bytes each

		// Act
		bmp.ApplyPalette(palette);

		// Assert
		Assert.NotNull(bmp.ColorPalette);
		Assert.Equal(palette.Length, bmp.ColorPalette.Length);
		Assert.Equal(256u, bmp.Metadata.ColorsUsed);
	}

	[Fact]
	public void ApplyPalette_ShouldThrowForNonPaletteImage()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);
		var palette = new byte[4];

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => bmp.ApplyPalette(palette));
	}

	[Fact]
	public void ApplyPalette_ShouldThrowForInvalidPaletteSize()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		var palette = new byte[5]; // Not multiple of 4

		// Act & Assert
		Assert.Throws<ArgumentException>(() => bmp.ApplyPalette(palette));
	}

	[Fact]
	public void IsValid_ShouldReturnTrueForValidImage()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);

		// Act
		var isValid = bmp.IsValid();

		// Assert
		Assert.True(isValid);
	}

	[Theory]
	[InlineData(0, 100)]
	[InlineData(-1, 100)]
	[InlineData(100, 0)]
	public void IsValid_ShouldReturnFalseForInvalidDimensions(int width, int height)
	{
		// Arrange
		var bmp = new BmpRaster(width, height);

		// Act
		var isValid = bmp.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void GetEstimatedFileSize_ShouldCalculateCorrectly()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);

		// Act
		var fileSize = bmp.GetEstimatedFileSize();

		// Assert
		var expectedSize = BmpConstants.FileHeaderSize + // 14
		                   BmpConstants.BitmapInfoHeaderSize + // 40
		                   bmp.PixelDataSize; // 30400
		Assert.Equal(expectedSize, fileSize);
	}

	[Fact]
	public void GetEstimatedFileSize_ShouldIncludePaletteSize()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		var palette = new byte[256 * 4];
		bmp.ApplyPalette(palette);

		// Act
		var fileSize = bmp.GetEstimatedFileSize();

		// Assert
		var expectedSize = BmpConstants.FileHeaderSize + // 14
		                   BmpConstants.BitmapInfoHeaderSize + // 40
		                   1024 + // Palette size
		                   bmp.PixelDataSize; // 10400
		Assert.Equal(expectedSize, fileSize);
	}

	[Fact]
	public void HasLargeMetadata_ShouldReturnFalseForSmallImage()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);

		// Act
		var hasLargeMetadata = bmp.Metadata.HasLargeMetadata;

		// Assert
		Assert.False(hasLargeMetadata);
	}

	[Fact]
	public void EstimatedMetadataSize_ShouldIncludeAllComponents()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		var palette = new byte[256 * 4];
		bmp.ApplyPalette(palette);
		bmp.Metadata.CustomFields["test"] = "test value";

		// Act
		var metadataSize = bmp.Metadata.EstimatedMetadataSize;

		// Assert
		Assert.True(metadataSize > 0);
		// Should include file header + DIB header + palette + custom field
		var expectedMinimum = BmpConstants.FileHeaderSize + BmpConstants.BitmapInfoHeaderSize + 1024;
		Assert.True(metadataSize >= expectedMinimum);
	}

	[Fact]
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);

		// Act & Assert
		var exception = Record.Exception(() => bmp.Dispose());
		Assert.Null(exception);
	}

	[Fact]
	public async Task DisposeAsync_ShouldNotThrow()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);

		// Act & Assert
		var exception = await Record.ExceptionAsync(async () => await bmp.DisposeAsync());
		Assert.Null(exception);
	}
}
