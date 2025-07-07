// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

public class TiffValidatorTests
{
	private static readonly int[] BitsPerSampleSingle = [8, 8, 8];

	[Theory]
	[InlineData(TiffColorDepth.Bilevel, true)]
	[InlineData(TiffColorDepth.EightBit, true)]
	[InlineData(TiffColorDepth.TwentyFourBit, true)]
	[InlineData(TiffColorDepth.SixtyFourBit, true)]
	public void IsValidColorDepth_ShouldReturnExpectedResult(TiffColorDepth colorDepth, bool expected)
	{
		// Act
		var result = TiffValidator.IsValidColorDepth(colorDepth);

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(TiffCompression.None, true)]
	[InlineData(TiffCompression.Lzw, true)]
	[InlineData(TiffCompression.Jpeg, true)]
	[InlineData(TiffCompression.PackBits, true)]
	public void IsValidCompression_ShouldReturnExpectedResult(TiffCompression compression, bool expected)
	{
		// Act
		var result = TiffValidator.IsValidCompression(compression);

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(PhotometricInterpretation.BlackIsZero, TiffColorDepth.Bilevel, true)]
	[InlineData(PhotometricInterpretation.Rgb, TiffColorDepth.TwentyFourBit, true)]
	[InlineData(PhotometricInterpretation.Palette, TiffColorDepth.EightBit, true)]
	[InlineData(PhotometricInterpretation.Cmyk, TiffColorDepth.ThirtyTwoBit, true)]
	[InlineData(PhotometricInterpretation.Rgb, TiffColorDepth.Bilevel, false)]
	public void IsValidPhotometricInterpretation_ShouldReturnExpectedResult(
		PhotometricInterpretation photometric,
		TiffColorDepth colorDepth,
		bool expected)
	{
		// Act
		var result = TiffValidator.IsValidPhotometricInterpretation(photometric, colorDepth);

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(3, PhotometricInterpretation.Rgb, false, true)]
	[InlineData(4, PhotometricInterpretation.Rgb, true, true)]
	[InlineData(1, PhotometricInterpretation.BlackIsZero, false, true)]
	[InlineData(4, PhotometricInterpretation.Cmyk, false, true)]
	[InlineData(5, PhotometricInterpretation.Cmyk, true, true)]
	[InlineData(2, PhotometricInterpretation.Rgb, false, false)]
	public void IsValidSamplesPerPixel_ShouldReturnExpectedResult(
		int samplesPerPixel,
		PhotometricInterpretation photometric,
		bool hasAlpha,
		bool expected)
	{
		// Act
		var result = TiffValidator.IsValidSamplesPerPixel(samplesPerPixel, photometric, hasAlpha);

		// Assert
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(new[] { 8, 8, 8 }, 3, true)]
	[InlineData(new[] { 16, 16, 16 }, 3, true)]
	[InlineData(new[] { 8, 8, 8, 8 }, 4, true)]
	[InlineData(new[] { 8, 8 }, 3, false)]
	[InlineData(new[] { 7, 8, 8 }, 3, false)]
	public void IsValidBitsPerSample_ShouldReturnExpectedResult(int[] bitsPerSample, int samplesPerPixel, bool expected)
	{
		// Act
		var result = TiffValidator.IsValidBitsPerSample(bitsPerSample.AsSpan(), samplesPerPixel);

		// Assert
		Assert.Equal(expected, result);
	}

	[Fact]
	public void IsValid_WithValidConfiguration_ShouldReturnTrue()
	{
		// Arrange
		var tiffRaster = new TiffRaster(1024, 768)
		{
			ColorDepth = TiffColorDepth.TwentyFourBit,
			Compression = TiffCompression.None,
			PhotometricInterpretation = PhotometricInterpretation.Rgb,
			SamplesPerPixel = 3,
			HasAlpha = false
		};
		tiffRaster.SetBitsPerSample(BitsPerSampleSingle);

		// Act
		var result = TiffValidator.IsValid(tiffRaster);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsValid_WithInvalidDimensions_ShouldReturnFalse()
	{
		// Arrange
		var tiffRaster = new TiffRaster(0, 768);

		// Act
		var result = TiffValidator.IsValid(tiffRaster);

		// Assert
		Assert.False(result);
	}
}
