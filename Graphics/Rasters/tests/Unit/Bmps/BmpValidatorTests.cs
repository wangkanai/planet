// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Bmps;

namespace Wangkanai.Graphics.Rasters.UnitTests.Bmps;

public class BmpValidatorTests
{
	[Fact]
	public void Validate_ValidBmp_ShouldReturnNoErrors()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Theory]
	[InlineData(0, 100)]
	[InlineData(-1, 100)]
	[InlineData(100, 0)]
	public void Validate_InvalidDimensions_ShouldReturnErrors(int width, int height)
	{
		// Arrange
		var bmp = new BmpRaster(width, height);

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
	}

	[Fact]
	public void Validate_DimensionsTooLarge_ShouldReturnErrors()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);
		bmp.Width = int.MaxValue;
		bmp.Height = int.MaxValue;
		bmp.Metadata.Width = int.MaxValue;         // Update metadata to match
		bmp.Metadata.Height = int.MaxValue;        // Update metadata to match
		bmp.Metadata.BitsPerPixel = (ushort)BmpColorDepth.TwentyFourBit; // Ensure consistency

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("exceeds maximum"));
	}

	[Fact]
	public void Validate_VeryLargeImage_ShouldReturnWarning()
	{
		// Arrange
		var bmp = new BmpRaster(100000, 100000); // Very large image

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very large image"));
	}

	[Fact]
	public void Validate_UnsupportedColorDepth_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.BitsPerPixel = 12; // Unsupported

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Color depth mismatch"));
	}

	[Fact]
	public void Validate_ColorDepthMismatch_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);
		bmp.Metadata.BitsPerPixel = 16; // Mismatch

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Color depth mismatch"));
	}

	[Fact]
	public void Validate_Rle4WithWrongColorDepth_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		bmp.Compression = BmpCompression.Rle4;

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("RLE4 compression is only valid for 4-bit"));
	}

	[Fact]
	public void Validate_Rle8WithWrongColorDepth_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.FourBit);
		bmp.Compression = BmpCompression.Rle8;

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("RLE8 compression is only valid for 8-bit"));
	}

	[Fact]
	public void Validate_BitFieldsWithWrongColorDepth_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		bmp.Compression = BmpCompression.BitFields;

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("BI_BITFIELDS compression is only valid for 16-bit and 32-bit"));
	}

	[Theory]
	[InlineData(BmpCompression.Jpeg)]
	[InlineData(BmpCompression.Png)]
	public void Validate_RareCompressionTypes_ShouldReturnWarning(BmpCompression compression)
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Compression = compression;

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("rarely supported"));
	}

	[Fact]
	public void Validate_MissingPaletteForIndexedImage_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		bmp.Metadata.ColorsUsed = 256; // Indicates palette should be present

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Palette required"));
	}

	[Fact]
	public void Validate_PaletteForNonIndexedImage_ShouldReturnWarning()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.TwentyFourBit);
		bmp.ColorPalette = new byte[256 * 4];

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Palette provided") && w.Contains("not required"));
	}

	[Fact]
	public void Validate_InvalidPaletteSize_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.EightBit);
		bmp.ColorPalette = new byte[5]; // Not multiple of 4

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid palette size"));
	}

	[Fact]
	public void Validate_TooManyPaletteColors_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.FourBit);
		bmp.ColorPalette = new byte[256 * 4]; // Too many for 4-bit

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Too many palette colors"));
	}

	[Fact]
	public void Validate_BitFieldsWithZeroMasks_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.SixteenBit);
		bmp.Compression = BmpCompression.BitFields;
		bmp.Metadata.Compression = BmpCompression.BitFields; // Update metadata to match
		// Masks remain zero (default)

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("requires non-zero color masks"));
	}

	[Fact]
	public void Validate_OverlappingBitMasks_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100, BmpColorDepth.SixteenBit);
		bmp.SetBitMasks(0xF000, 0xF000, 0x000F); // Red and Green overlap

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Color bit masks cannot overlap"));
	}

	[Fact]
	public void Validate_InvalidFileSignature_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.FileSignature = "XX";

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid BMP signature"));
	}

	[Fact]
	public void Validate_UnsupportedHeaderSize_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.HeaderSize = 100; // Unsupported

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Unsupported header size"));
	}

	[Fact]
	public void Validate_DimensionMismatch_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.Width = 200; // Mismatch

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Width mismatch"));
	}

	[Fact]
	public void Validate_InvalidPlanes_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.Planes = 2; // Should be 1

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid planes value"));
	}

	[Fact]
	public void Validate_NegativeResolution_ShouldReturnError()
	{
		// Arrange
		var bmp = new BmpRaster(100, 100);
		bmp.Metadata.XPixelsPerMeter = -100;

		// Act
		var result = BmpValidator.Validate(bmp);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Resolution values cannot be negative"));
	}

	[Fact]
	public void IsValidBmpSignature_ValidSignature_ShouldReturnTrue()
	{
		// Arrange
		var data = new byte[] { 0x42, 0x4D, 0x00, 0x00 }; // "BM" + padding

		// Act
		var isValid = BmpValidator.IsValidBmpSignature(data);

		// Assert
		Assert.True(isValid);
	}

	[Fact]
	public void IsValidBmpSignature_InvalidSignature_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		// Act
		var isValid = BmpValidator.IsValidBmpSignature(data);

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void IsValidBmpSignature_TooShort_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[] { 0x42 }; // Only one byte

		// Act
		var isValid = BmpValidator.IsValidBmpSignature(data);

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(BmpConstants.BitmapInfoHeaderSize)]
	[InlineData(BmpConstants.BitmapV4HeaderSize)]
	[InlineData(BmpConstants.BitmapV5HeaderSize)]
	public void DetectHeaderType_ValidHeader_ShouldReturnCorrectSize(uint headerSize)
	{
		// Arrange
		var data = new byte[20];
		data[0] = 0x42; // 'B'
		data[1] = 0x4D; // 'M'
		// Skip file header (14 bytes)
		var headerSizeBytes = BitConverter.GetBytes(headerSize);
		Array.Copy(headerSizeBytes, 0, data, 14, 4);

		// Act
		var detectedSize = BmpValidator.DetectHeaderType(data);

		// Assert
		Assert.Equal(headerSize, detectedSize);
	}

	[Fact]
	public void DetectHeaderType_InvalidSignature_ShouldReturnZero()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		// Act
		var detectedSize = BmpValidator.DetectHeaderType(data);

		// Assert
		Assert.Equal(0u, detectedSize);
	}

	[Fact]
	public void DetectHeaderType_TooShort_ShouldReturnZero()
	{
		// Arrange
		var data = new byte[10]; // Too short

		// Act
		var detectedSize = BmpValidator.DetectHeaderType(data);

		// Assert
		Assert.Equal(0u, detectedSize);
	}
}