// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Bmps;

namespace Wangkanai.Graphics.Rasters.UnitTests.Bmps;

public class BmpMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaults()
	{
		// Act
		var metadata = new BmpMetadata();

		// Assert
		Assert.Equal("BM", metadata.FileSignature);
		Assert.Equal((uint)BmpConstants.BitmapInfoHeaderSize, metadata.HeaderSize);
		Assert.Equal(BmpConstants.Planes, metadata.Planes);
		Assert.Equal(BmpCompression.Rgb, metadata.Compression);
		Assert.Equal(BmpConstants.DefaultHorizontalResolution, metadata.XPixelsPerMeter);
		Assert.Equal(BmpConstants.DefaultVerticalResolution, metadata.YPixelsPerMeter);
		Assert.NotNull(metadata.CustomFields);
		Assert.Empty(metadata.CustomFields);
	}

	[Fact]
	public void HasPalette_ShouldReturnTrueFor8BitAndBelow()
	{
		// Arrange
		var metadata = new BmpMetadata();

		// Act & Assert
		metadata.BitsPerPixel = 1;
		Assert.True(metadata.HasPalette);

		metadata.BitsPerPixel = 4;
		Assert.True(metadata.HasPalette);

		metadata.BitsPerPixel = 8;
		Assert.True(metadata.HasPalette);

		metadata.BitsPerPixel = 16;
		Assert.False(metadata.HasPalette);

		metadata.BitsPerPixel = 24;
		Assert.False(metadata.HasPalette);

		metadata.BitsPerPixel = 32;
		Assert.False(metadata.HasPalette);
	}

	[Fact]
	public void HasAlpha_ShouldReturnTrueFor32BitOrBitFields()
	{
		// Arrange
		var metadata = new BmpMetadata();

		// Act & Assert
		metadata.BitsPerPixel = 32;
		Assert.True(metadata.HasAlpha);

		metadata.BitsPerPixel = 24;
		Assert.False(metadata.HasAlpha);

		metadata.BitsPerPixel = 16;
		metadata.Compression = BmpCompression.BitFields;
		metadata.AlphaMask = 0xFF000000;
		Assert.True(metadata.HasAlpha);

		metadata.AlphaMask = 0;
		Assert.False(metadata.HasAlpha);
	}

	[Fact]
	public void IsTopDown_ShouldReturnTrueForNegativeHeight()
	{
		// Arrange
		var metadata = new BmpMetadata();

		// Act & Assert
		metadata.Height = 600;
		Assert.False(metadata.IsTopDown);

		metadata.Height = -600;
		Assert.True(metadata.IsTopDown);
	}

	[Fact]
	public void AbsoluteHeight_ShouldReturnPositiveValue()
	{
		// Arrange
		var metadata = new BmpMetadata();

		// Act & Assert
		metadata.Height = 600;
		Assert.Equal(600, metadata.AbsoluteHeight);

		metadata.Height = -600;
		Assert.Equal(600, metadata.AbsoluteHeight);
	}

	[Theory]
	[InlineData(1, 2u)]
	[InlineData(4, 16u)]
	[InlineData(8, 256u)]
	[InlineData(16, 0u)]
	[InlineData(24, 0u)]
	[InlineData(32, 0u)]
	public void PaletteColors_ShouldReturnCorrectCount(ushort bitsPerPixel, uint expectedColors)
	{
		// Arrange
		var metadata = new BmpMetadata { BitsPerPixel = bitsPerPixel };

		// Act
		var colors = metadata.PaletteColors;

		// Assert
		Assert.Equal(expectedColors, colors);
	}

	[Theory]
	[InlineData(1, 8u)]
	[InlineData(4, 64u)]
	[InlineData(8, 1024u)]
	[InlineData(16, 0u)]
	public void PaletteSizeInBytes_ShouldReturnCorrectSize(ushort bitsPerPixel, uint expectedSize)
	{
		// Arrange
		var metadata = new BmpMetadata { BitsPerPixel = bitsPerPixel };

		// Act
		var size = metadata.PaletteSizeInBytes;

		// Assert
		Assert.Equal(expectedSize, size);
	}

	[Theory]
	[InlineData(8, 1)]
	[InlineData(16, 2)]
	[InlineData(24, 3)]
	[InlineData(32, 4)]
	public void BytesPerPixel_ShouldReturnCorrectValue(ushort bitsPerPixel, int expectedBytes)
	{
		// Arrange
		var metadata = new BmpMetadata { BitsPerPixel = bitsPerPixel };

		// Act
		var bytes = metadata.BytesPerPixel;

		// Assert
		Assert.Equal(expectedBytes, bytes);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(4)]
	public void BytesPerPixel_ShouldReturnZeroForPackedFormats(ushort bitsPerPixel)
	{
		// Arrange
		var metadata = new BmpMetadata { BitsPerPixel = bitsPerPixel };

		// Act
		var bytes = metadata.BytesPerPixel;

		// Assert
		Assert.Equal(0, bytes);
	}

	[Fact]
	public void BytesPerPixel_ShouldThrowForUnsupportedBitDepth()
	{
		// Arrange
		var metadata = new BmpMetadata { BitsPerPixel = 12 };

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => metadata.BytesPerPixel);
	}

	[Theory]
	[InlineData(100, 24, 300)] // (100 * 24 + 7) / 8 = 300, already aligned
	[InlineData(101, 24, 304)] // (101 * 24 + 7) / 8 = 303, aligned to 304
	[InlineData(100, 8, 100)]  // (100 * 8 + 7) / 8 = 100, already aligned
	[InlineData(101, 8, 104)]  // (101 * 8 + 7) / 8 = 101, aligned to 104
	public void RowStride_ShouldAlignTo4ByteBoundary(int width, ushort bitsPerPixel, int expectedStride)
	{
		// Arrange
		var metadata = new BmpMetadata
		{
			Width = width,
			BitsPerPixel = bitsPerPixel
		};

		// Act
		var stride = metadata.RowStride;

		// Assert
		Assert.Equal(expectedStride, stride);
	}

	[Fact]
	public void PixelDataSize_ShouldCalculateCorrectly()
	{
		// Arrange
		var metadata = new BmpMetadata
		{
			Width = 100,
			Height = 50,
			BitsPerPixel = 24
		};

		// Act
		var size = metadata.PixelDataSize;

		// Assert - 100 pixels * 24 bits = 300 bytes, already aligned, * 50 rows = 15000
		Assert.Equal(15000u, size);
	}

	[Fact]
	public void PixelDataSize_ShouldHandleNegativeHeight()
	{
		// Arrange
		var metadata = new BmpMetadata
		{
			Width = 100,
			Height = -50, // Top-down
			BitsPerPixel = 24
		};

		// Act
		var size = metadata.PixelDataSize;

		// Assert - Should use absolute height
		Assert.Equal(15000u, size);
	}

	[Theory]
	[InlineData(BmpConstants.BitmapInfoHeaderSize, "BITMAPINFOHEADER")]
	[InlineData(BmpConstants.BitmapV4HeaderSize, "BITMAPV4HEADER")]
	[InlineData(BmpConstants.BitmapV5HeaderSize, "BITMAPV5HEADER")]
	[InlineData(100u, "Unknown (100 bytes)")]
	public void HeaderType_ShouldReturnCorrectType(uint headerSize, string expectedType)
	{
		// Arrange
		var metadata = new BmpMetadata { HeaderSize = headerSize };

		// Act
		var headerType = metadata.HeaderType;

		// Assert
		Assert.Equal(expectedType, headerType);
	}

	[Fact]
	public void CustomFields_ShouldAllowAddingCustomData()
	{
		// Arrange
		var metadata = new BmpMetadata();

		// Act
		metadata.CustomFields["CustomString"] = "Test Value";
		metadata.CustomFields["CustomNumber"] = 42;
		metadata.CustomFields["CustomBytes"] = new byte[] { 1, 2, 3 };

		// Assert
		Assert.Equal(3, metadata.CustomFields.Count);
		Assert.Equal("Test Value", metadata.CustomFields["CustomString"]);
		Assert.Equal(42, metadata.CustomFields["CustomNumber"]);
		Assert.Equal(new byte[] { 1, 2, 3 }, metadata.CustomFields["CustomBytes"]);
	}
}