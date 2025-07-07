// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Bmps;

public class BmpConstantsTests
{
	[Fact]
	public void Signature_ShouldBeBM()
	{
		// Act
		var signature = BmpConstants.Signature;

		// Assert
		Assert.Equal([0x42, 0x4D], signature); // "BM"
	}

	[Fact]
	public void HeaderSizes_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(14, BmpConstants.FileHeaderSize);
		Assert.Equal(40, BmpConstants.BitmapInfoHeaderSize);
		Assert.Equal(108, BmpConstants.BitmapV4HeaderSize);
		Assert.Equal(124, BmpConstants.BitmapV5HeaderSize);
	}

	[Fact]
	public void DimensionLimits_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1, BmpConstants.MinWidth);
		Assert.Equal(int.MaxValue, BmpConstants.MaxWidth);
		Assert.Equal(1, BmpConstants.MinHeight);
		Assert.Equal(int.MaxValue, BmpConstants.MaxHeight);
	}

	[Fact]
	public void Planes_ShouldBeOne()
	{
		// Assert
		Assert.Equal(1, BmpConstants.Planes);
	}

	[Fact]
	public void RowAlignment_ShouldBeFourBytes()
	{
		// Assert
		Assert.Equal(4, BmpConstants.RowAlignment);
	}

	[Fact]
	public void DefaultResolution_ShouldBe96Dpi()
	{
		// Assert - 96 DPI = 3780 pixels per meter
		Assert.Equal(3780, BmpConstants.DefaultHorizontalResolution);
		Assert.Equal(3780, BmpConstants.DefaultVerticalResolution);
	}

	[Fact]
	public void PaletteConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(256u, BmpConstants.MaxPaletteColors);
		Assert.Equal(4, BmpConstants.PaletteEntrySize);
	}

	[Theory]
	[InlineData(BmpConstants.Compression.BI_RGB, 0u)]
	[InlineData(BmpConstants.Compression.BI_RLE8, 1u)]
	[InlineData(BmpConstants.Compression.BI_RLE4, 2u)]
	[InlineData(BmpConstants.Compression.BI_BITFIELDS, 3u)]
	[InlineData(BmpConstants.Compression.BI_JPEG, 4u)]
	[InlineData(BmpConstants.Compression.BI_PNG, 5u)]
	public void CompressionConstants_ShouldHaveCorrectValues(uint actual, uint expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(BmpConstants.BitDepth.Monochrome, 1)]
	[InlineData(BmpConstants.BitDepth.FourBit, 4)]
	[InlineData(BmpConstants.BitDepth.EightBit, 8)]
	[InlineData(BmpConstants.BitDepth.SixteenBit, 16)]
	[InlineData(BmpConstants.BitDepth.TwentyFourBit, 24)]
	[InlineData(BmpConstants.BitDepth.ThirtyTwoBit, 32)]
	public void BitDepthConstants_ShouldHaveCorrectValues(ushort actual, ushort expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void RGB555Masks_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(0x7C00u, BmpConstants.RGB555Masks.Red);
		Assert.Equal(0x03E0u, BmpConstants.RGB555Masks.Green);
		Assert.Equal(0x001Fu, BmpConstants.RGB555Masks.Blue);
		Assert.Equal(0x0000u, BmpConstants.RGB555Masks.Alpha);
	}

	[Fact]
	public void RGB565Masks_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(0xF800u, BmpConstants.RGB565Masks.Red);
		Assert.Equal(0x07E0u, BmpConstants.RGB565Masks.Green);
		Assert.Equal(0x001Fu, BmpConstants.RGB565Masks.Blue);
		Assert.Equal(0x0000u, BmpConstants.RGB565Masks.Alpha);
	}

	[Fact]
	public void ARGB8888Masks_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(0x00FF0000u, BmpConstants.ARGB8888Masks.Red);
		Assert.Equal(0x0000FF00u, BmpConstants.ARGB8888Masks.Green);
		Assert.Equal(0x000000FFu, BmpConstants.ARGB8888Masks.Blue);
		Assert.Equal(0xFF000000u, BmpConstants.ARGB8888Masks.Alpha);
	}

	[Fact]
	public void RLEConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(0x00, BmpConstants.RLE.EndOfLine);
		Assert.Equal(0x01, BmpConstants.RLE.EndOfBitmap);
		Assert.Equal(0x02, BmpConstants.RLE.Delta);
	}

	[Fact]
	public void ColorSpaceConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(0u, BmpConstants.ColorSpace.LCS_CALIBRATED_RGB);
		Assert.Equal(0x73524742u, BmpConstants.ColorSpace.LCS_sRGB); // 'sRGB'
		Assert.Equal(0x57696E20u, BmpConstants.ColorSpace.LCS_WINDOWS_COLOR_SPACE); // 'Win '
	}

	[Fact]
	public void IntentConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1u, BmpConstants.Intent.LCS_GM_BUSINESS);
		Assert.Equal(2u, BmpConstants.Intent.LCS_GM_GRAPHICS);
		Assert.Equal(4u, BmpConstants.Intent.LCS_GM_IMAGES);
		Assert.Equal(8u, BmpConstants.Intent.LCS_GM_ABS_COLORIMETRIC);
	}

	[Fact]
	public void Signature_ShouldBe2BytesLong()
	{
		// Assert
		Assert.Equal(2, BmpConstants.Signature.Length);
	}
}
