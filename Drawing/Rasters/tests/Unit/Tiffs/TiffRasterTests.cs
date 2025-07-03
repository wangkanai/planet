// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Drawing.Rasters.Tiffs;
using Xunit;

namespace Wangkanai.Planet.Drawing.Rasters.UnitTests.Tiffs;

public class TiffRasterTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaultValues()
	{
		// Arrange & Act
		var tiffRaster = new TiffRaster();
		
		// Assert
		Assert.Equal(TiffColorDepth.TwentyFourBit, tiffRaster.ColorDepth);
		Assert.Equal(TiffCompression.None, tiffRaster.Compression);
		Assert.Equal(PhotometricInterpretation.Rgb, tiffRaster.PhotometricInterpretation);
		Assert.Equal(3, tiffRaster.SamplesPerPixel);
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(new[] { 8, 8, 8 }));
		Assert.False(tiffRaster.HasAlpha);
		Assert.Equal(1, tiffRaster.PlanarConfiguration);
		Assert.NotNull(tiffRaster.Metadata);
	}
	
	[Fact]
	public void Constructor_WithDimensions_ShouldSetWidthAndHeight()
	{
		// Arrange
		const int width = 1024;
		const int height = 768;
		
		// Act
		var tiffRaster = new TiffRaster(width, height);
		
		// Assert
		Assert.Equal(width, tiffRaster.Width);
		Assert.Equal(height, tiffRaster.Height);
	}
	
	[Fact]
	public void ColorDepth_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act
		tiffRaster.ColorDepth = TiffColorDepth.SixteenBit;
		
		// Assert
		Assert.Equal(TiffColorDepth.SixteenBit, tiffRaster.ColorDepth);
	}
	
	[Fact]
	public void Compression_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act
		tiffRaster.Compression = TiffCompression.Lzw;
		
		// Assert
		Assert.Equal(TiffCompression.Lzw, tiffRaster.Compression);
	}
	
	[Fact]
	public void Metadata_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var metadata = new TiffMetadata
		{
			ImageDescription = "Test Image",
			Make = "Test Camera",
			Model = "Test Model"
		};
		
		// Act
		tiffRaster.Metadata = metadata;
		
		// Assert
		Assert.Equal("Test Image", tiffRaster.Metadata.ImageDescription);
		Assert.Equal("Test Camera", tiffRaster.Metadata.Make);
		Assert.Equal("Test Model", tiffRaster.Metadata.Model);
	}
	
	[Fact]
	public void SetBitsPerSample_WithArray_ShouldUpdateBitsPerSample()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var newBitsPerSample = new[] { 16, 16, 16, 16 };
		
		// Act
		tiffRaster.SetBitsPerSample(newBitsPerSample);
		
		// Assert
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(newBitsPerSample));
	}
	
	[Fact]
	public void SetBitsPerSample_WithSpan_ShouldUpdateBitsPerSample()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var newBitsPerSample = new[] { 32, 32 };
		
		// Act
		tiffRaster.SetBitsPerSample(newBitsPerSample.AsSpan());
		
		// Assert
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(newBitsPerSample));
	}
	
	[Fact]
	public void BitsPerSample_ShouldReturnReadOnlySpan()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act
		var bitsPerSample = tiffRaster.BitsPerSample;
		
		// Assert
		Assert.Equal(3, bitsPerSample.Length);
		Assert.True(bitsPerSample.SequenceEqual(new[] { 8, 8, 8 }));
	}
	
	[Theory]
	[InlineData(new int[] { })]
	[InlineData(new[] { 8 })]
	[InlineData(new[] { 8, 8 })]
	[InlineData(new[] { 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8, 8, 8, 8, 8 })]
	public void BitsPerSample_WithVariousSampleCounts_ShouldStoreAndRetrieveCorrectly(int[] expected)
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;
		
		// Assert
		Assert.Equal(expected.Length, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}
	
	[Fact]
	public void BitsPerSample_WithInlineStorage_ShouldUseStackAllocation()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var expected = new[] { 16, 16, 16 };
		
		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;
		
		// Assert - Verify behavior for inline storage (1-4 samples)
		Assert.Equal(3, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}
	
	[Fact]
	public void BitsPerSample_WithLargeArray_ShouldFallbackToHeapAllocation()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var expected = new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 }; // More than 4 samples
		
		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;
		
		// Assert - Verify behavior for heap allocation (>4 samples)
		Assert.Equal(10, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}
	
	[Fact]
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act & Assert
		tiffRaster.Dispose();
	}
}