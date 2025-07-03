// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Planet.Drawing.Rasters.Tiff;
using Xunit;

namespace Wangkanai.Planet.Drawing.Rasters.UnitTests.Tiff;

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
		Assert.Equal(new[] { 8, 8, 8 }, tiffRaster.BitsPerSample);
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
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		
		// Act & Assert
		tiffRaster.Dispose();
	}
}