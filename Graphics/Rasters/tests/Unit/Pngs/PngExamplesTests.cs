// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Pngs;

public class PngExamplesTests
{
	[Theory]
	[InlineData(800, 600)]
	[InlineData(1920, 1080)]
	[InlineData(100, 100)]
	public void CreateBasicTruecolor_ShouldCreateValidPng(int width, int height)
	{
		// Act
		var png = PngExamples.CreateBasicTruecolor(width, height);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
		Assert.Equal(PngColorType.Truecolor, png.ColorType);
		Assert.Equal(8, png.BitDepth);
		Assert.Equal(6, png.CompressionLevel);
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1920, 1080)]
	[InlineData(100, 100)]
	public void CreateTruecolorWithAlpha_ShouldCreateValidPng(int width, int height)
	{
		// Act
		var png = PngExamples.CreateTruecolorWithAlpha(width, height);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
		Assert.Equal(PngColorType.TruecolorWithAlpha, png.ColorType);
		Assert.Equal(8, png.BitDepth);
		Assert.True(png.HasAlphaChannel);
		Assert.Equal(6, png.CompressionLevel);
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(800, 600, 1)]
	[InlineData(800, 600, 2)]
	[InlineData(800, 600, 4)]
	[InlineData(800, 600, 8)]
	[InlineData(800, 600, 16)]
	public void CreateGrayscale_ShouldCreateValidPng(int width, int height, byte bitDepth)
	{
		// Act
		var png = PngExamples.CreateGrayscale(width, height, bitDepth);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
		Assert.Equal(PngColorType.Grayscale, png.ColorType);
		Assert.Equal(bitDepth, png.BitDepth);
		Assert.Equal(6, png.CompressionLevel);
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.True(png.IsValid());
	}

	[Fact]
	public void CreateGrayscale_WithDefaultBitDepth_ShouldUse8Bit()
	{
		// Act
		var png = PngExamples.CreateGrayscale(100, 100);

		// Assert
		Assert.Equal(8, png.BitDepth);
		Assert.Equal(PngColorType.Grayscale, png.ColorType);
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(800, 600, 1)]
	[InlineData(800, 600, 2)]
	[InlineData(800, 600, 4)]
	[InlineData(800, 600, 8)]
	public void CreateIndexedColor_ShouldCreateValidPng(int width, int height, byte bitDepth)
	{
		// Act
		var png = PngExamples.CreateIndexedColor(width, height, bitDepth);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
		Assert.Equal(PngColorType.IndexedColor, png.ColorType);
		Assert.Equal(bitDepth, png.BitDepth);
		Assert.True(png.UsesPalette);
		Assert.False(png.PaletteData.IsEmpty);
		Assert.Equal(6, png.CompressionLevel);
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.True(png.IsValid());
	}

	[Fact]
	public void CreateIndexedColor_WithDefaultBitDepth_ShouldUse8Bit()
	{
		// Act
		var png = PngExamples.CreateIndexedColor(100, 100);

		// Assert
		Assert.Equal(8, png.BitDepth);
		Assert.Equal(PngColorType.IndexedColor, png.ColorType);
		Assert.True(png.IsValid());
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(4)]
	[InlineData(8)]
	public void CreateIndexedColor_ShouldCreateCorrectPaletteSize(byte bitDepth)
	{
		// Act
		var png = PngExamples.CreateIndexedColor(100, 100, bitDepth);

		// Assert
		var expectedPaletteEntries = Math.Min(256, 1 << bitDepth);
		var expectedPaletteSize    = expectedPaletteEntries * 3;// RGB triplets

		Assert.False(png.PaletteData.IsEmpty);
		Assert.Equal(expectedPaletteSize, png.PaletteData.Length);
	}

	[Fact]
	public void CreateIndexedColor_ShouldCreateGradientPalette()
	{
		// Act
		var png = PngExamples.CreateIndexedColor(100, 100, 8);

		// Assert
		Assert.False(png.PaletteData.IsEmpty);
		Assert.Equal(768, png.PaletteData.Length);// 256 * 3

		var palette = png.PaletteData.Span;
		// Check first entry (should be black: 0, 0, 0)
		Assert.Equal(0, palette[0]);
		Assert.Equal(0, palette[1]);
		Assert.Equal(0, palette[2]);

		// Check last entry (should be white: 255, 255, 255)
		Assert.Equal(255, palette[765]);
		Assert.Equal(255, palette[766]);
		Assert.Equal(255, palette[767]);
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1920, 1080)]
	public void CreateHighQuality_ShouldCreateValidPng(int width, int height)
	{
		// Act
		var png = PngExamples.CreateHighQuality(width, height);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);
		Assert.Equal(PngColorType.TruecolorWithAlpha, png.ColorType);
		Assert.Equal(16, png.BitDepth);// High quality
		Assert.True(png.HasAlphaChannel);
		Assert.Equal(9, png.CompressionLevel);// Maximum compression
		Assert.Equal(PngInterlaceMethod.None, png.InterlaceMethod);
		Assert.True(png.IsValid());

		// Check metadata
		Assert.Equal("Wangkanai Graphics Rasters", png.Metadata.Software);
		Assert.NotNull(png.Metadata.Created);
	}

	[Theory]
	[InlineData(800, 600, true)]
	[InlineData(800, 600, false)]
	[InlineData(1920, 1080, true)]
	public void CreateWebOptimized_ShouldCreateValidPng(int width, int height, bool useAlpha)
	{
		// Act
		var png = PngExamples.CreateWebOptimized(width, height, useAlpha);

		// Assert
		Assert.Equal(width, png.Width);
		Assert.Equal(height, png.Height);

		if (useAlpha)
		{
			Assert.Equal(PngColorType.TruecolorWithAlpha, png.ColorType);
			Assert.True(png.HasAlphaChannel);
		}
		else
		{
			Assert.Equal(PngColorType.Truecolor, png.ColorType);
			Assert.False(png.HasAlphaChannel);
		}

		Assert.Equal(8, png.BitDepth);                              // Standard web bit depth
		Assert.Equal(6, png.CompressionLevel);                      // Balanced compression
		Assert.Equal(PngInterlaceMethod.Adam7, png.InterlaceMethod);// Progressive
		Assert.True(png.IsValid());

		// Check web-appropriate metadata
		Assert.Equal("Wangkanai Graphics Rasters", png.Metadata.Software);
		Assert.Equal((byte)0, png.Metadata.SrgbRenderingIntent);// Perceptual rendering
	}

	[Fact]
	public void CreateWebOptimized_WithDefaultAlpha_ShouldIncludeAlpha()
	{
		// Act
		var png = PngExamples.CreateWebOptimized(100, 100);

		// Assert
		Assert.Equal(PngColorType.TruecolorWithAlpha, png.ColorType);
		Assert.True(png.HasAlphaChannel);
	}

	[Fact]
	public void AllExamples_ShouldPassValidation()
	{
		// Arrange & Act
		var examples = new List<PngRaster>
		               {
			               PngExamples.CreateBasicTruecolor(100, 100),
			               PngExamples.CreateTruecolorWithAlpha(100, 100),
			               PngExamples.CreateGrayscale(100, 100),
			               PngExamples.CreateGrayscale(100, 100, 16),
			               PngExamples.CreateIndexedColor(100, 100),
			               PngExamples.CreateIndexedColor(100, 100, 4),
			               PngExamples.CreateHighQuality(100, 100),
			               PngExamples.CreateWebOptimized(100, 100, true),
			               PngExamples.CreateWebOptimized(100, 100, false)
		               };

		// Assert
		foreach (var example in examples)
		{
			var validationResult = example.Validate();
			Assert.True(validationResult.IsValid, $"Example failed validation: {validationResult.GetSummary()}");
		}
	}
}
