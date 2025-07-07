// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;
using Wangkanai.Graphics.Rasters.Jpeg2000s;

namespace Wangkanai.Graphics.Rasters.UnitTests.Jpeg2000s;

public class Jpeg2000ValidatorTests
{
	[Fact]
	public void Validate_ValidJpeg2000_ShouldReturnNoErrors()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Theory]
	[InlineData(0, 600)]
	[InlineData(-1, 600)]
	[InlineData(800, 0)]
	[InlineData(800, -1)]
	public void Validate_InvalidDimensions_ShouldReturnErrors(int width, int height)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(100, 100, 3);
		// Manually set invalid dimensions to test validation
		typeof(Jpeg2000Raster).GetProperty("Width")!.SetValue(jpeg2000, width);
		typeof(Jpeg2000Raster).GetProperty("Height")!.SetValue(jpeg2000, height);

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.NotEmpty(result.Errors);
	}

	[Fact]
	public void Validate_DimensionsTooLarge_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(100000, 100000, 3); // Very large image

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very large image"));
	}

	[Fact]
	public void Validate_TooManyDecompositionLevels_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(100, 100, 3);
		jpeg2000.DecompositionLevels = 20; // Too many for small image

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Too many decomposition levels"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(20000)] // Exceeds MaxComponents
	public void Validate_InvalidComponents_ShouldReturnError(int components)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.Components = components;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("component"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(50)] // Exceeds MaxBitDepth
	public void Validate_InvalidBitDepth_ShouldReturnError(int bitDepth)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.BitDepth = bitDepth;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("bit depth"));
	}

	[Fact]
	public void Validate_HighBitDepthWithMultipleComponents_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 4);
		jpeg2000.Metadata.BitDepth = 24; // High bit depth with 4 components

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("High bit depth"));
	}

	[Fact]
	public void Validate_LosslessWithWrongWavelet_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.IsLossless = true;
		jpeg2000.Metadata.WaveletTransform = Jpeg2000Constants.WaveletTransforms.Irreversible97; // Wrong for lossless

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Lossless compression requires 5/3 reversible"));
	}

	[Theory]
	[InlineData(0.0f)]
	[InlineData(0.5f)]
	[InlineData(1.0f)]
	public void Validate_InvalidCompressionRatioForLossy_ShouldReturnError(float ratio)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.IsLossless = false;
		jpeg2000.Metadata.CompressionRatio = ratio;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid compression ratio"));
	}

	[Fact]
	public void Validate_VeryHighCompressionRatio_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.IsLossless = false;
		jpeg2000.CompressionRatio = 250.0f; // Very high

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very high compression ratio"));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(50)] // Exceeds MaxDecompositionLevels
	public void Validate_InvalidDecompositionLevels_ShouldReturnError(int levels)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.DecompositionLevels = levels;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("decomposition levels"));
	}

	[Fact]
	public void Validate_ZeroDecompositionLevels_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.DecompositionLevels = 0;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Zero decomposition levels"));
	}

	[Theory]
	[InlineData(0, 512)]
	[InlineData(-1, 512)]
	[InlineData(512, 0)]
	[InlineData(512, -1)]
	public void Validate_InvalidTileDimensions_ShouldReturnError(int tileWidth, int tileHeight)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		// Set invalid values directly in metadata to bypass property validation
		jpeg2000.Metadata.TileWidth = tileWidth;
		jpeg2000.Metadata.TileHeight = tileHeight;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid tile dimensions"));
	}

	[Fact]
	public void Validate_TileLargerThanImage_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(400, 300, 3);
		jpeg2000.TileWidth = 500; // Larger than image width

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Tile dimensions cannot exceed image dimensions"));
	}

	[Fact]
	public void Validate_SmallTileSize_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(2048, 1536, 3);
		jpeg2000.TileWidth = 100; // Small tile size
		jpeg2000.TileHeight = 100;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Small tile size"));
	}

	[Fact]
	public void Validate_LargeTileSize_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(8192, 6144, 3);
		jpeg2000.TileWidth = 5000; // Large tile size
		jpeg2000.TileHeight = 5000;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large tile size"));
	}

	[Fact]
	public void Validate_NonPowerOfTwoTileSize_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(2048, 1536, 3);
		jpeg2000.TileWidth = 300; // Not power of 2
		jpeg2000.TileHeight = 300;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Non-power-of-2 tile sizes"));
	}

	[Fact]
	public void Validate_ManyTiles_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(10000, 10000, 3);
		jpeg2000.TileWidth = 64; // Creates many tiles
		jpeg2000.TileHeight = 64;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large number of tiles"));
	}

	[Fact]
	public void Validate_LargeImageWithoutTiling_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(10000, 12000, 3); // 120 megapixels
		jpeg2000.TileWidth = jpeg2000.Width; // Single tile
		jpeg2000.TileHeight = jpeg2000.Height;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large image without tiling"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(70000)] // Exceeds MaxLayers
	public void Validate_InvalidQualityLayers_ShouldReturnError(int layers)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.QualityLayers = layers;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("quality layers"));
	}

	[Fact]
	public void Validate_LosslessWithMultipleLayers_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.IsLossless = true;
		jpeg2000.QualityLayers = 5;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Multiple quality layers have limited benefit for lossless"));
	}

	[Fact]
	public void Validate_ManyQualityLayers_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.QualityLayers = 25; // Many layers

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large number of quality layers"));
	}

	[Theory]
	[InlineData(0, 100)]
	[InlineData(-10, 100)]
	[InlineData(100, 0)]
	[InlineData(100, -10)]
	public void Validate_InvalidRoiDimensions_ShouldReturnError(int width, int height)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.RegionOfInterest = new Rectangle(100, 100, width, height);

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid ROI dimensions"));
	}

	[Fact]
	public void Validate_RoiOutsideBounds_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.RegionOfInterest = new Rectangle(700, 500, 200, 200); // Extends beyond bounds

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("ROI extends outside image bounds"));
	}

	[Theory]
	[InlineData(0.0f)]
	[InlineData(-1.0f)]
	public void Validate_InvalidRoiQualityFactor_ShouldReturnError(float qualityFactor)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.RegionOfInterest = new Rectangle(100, 100, 200, 200);
		jpeg2000.Metadata.RoiQualityFactor = qualityFactor;

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid ROI quality factor"));
	}

	[Fact]
	public void Validate_VeryHighRoiQualityFactor_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.RegionOfInterest = new Rectangle(100, 100, 200, 200);
		jpeg2000.RoiQualityFactor = 15.0f; // Very high

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very high ROI quality factor"));
	}

	[Fact]
	public void Validate_LargeRoi_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(1000, 1000, 3);
		jpeg2000.RegionOfInterest = new Rectangle(10, 10, 900, 900); // 81% of image

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large ROI"));
	}

	[Fact]
	public void Validate_SmallRoi_ShouldReturnWarning()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(1000, 1000, 3);
		jpeg2000.RegionOfInterest = new Rectangle(100, 100, 3, 3); // Very small ROI

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very small ROI"));
	}

	[Fact]
	public void Validate_DimensionMismatch_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.Width = 900; // Mismatch

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Width mismatch"));
	}

	[Fact]
	public void Validate_InvalidGeoTransform_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.GeoTransform = new double[] { 1.0, 2.0 }; // Wrong length

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid GeoTransform length"));
	}

	[Fact]
	public void Validate_ZeroPixelSizeInGeoTransform_ShouldReturnError()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.Metadata.GeoTransform = new double[] { 1.0, 0.0, 3.0, 4.0, 0.0, 0.0 }; // Both pixel sizes zero

		// Act
		var result = Jpeg2000Validator.Validate(jpeg2000);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Both X and Y pixel sizes are zero"));
	}

	[Fact]
	public void IsValidJp2Signature_WithValidSignature_ShouldReturnTrue()
	{
		// Arrange
		var data = new byte[12];
		// JPEG2000 uses big-endian byte order
		data[0] = 0x00; data[1] = 0x00; data[2] = 0x00; data[3] = 0x0C; // Box size (12)
		"jP  "u8.ToArray().CopyTo(data, 4); // Box type
		new byte[] { 0x0D, 0x0A, 0x87, 0x0A }.CopyTo(data, 8); // Signature data

		// Act
		var isValid = Jpeg2000Validator.IsValidJp2Signature(data);

		// Assert
		Assert.True(isValid);
	}

	[Fact]
	public void IsValidJp2Signature_WithInvalidSignature_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		// Act
		var isValid = Jpeg2000Validator.IsValidJp2Signature(data);

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void IsValidJp2Signature_WithTooShortData_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[5]; // Too short

		// Act
		var isValid = Jpeg2000Validator.IsValidJp2Signature(data);

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void DetectJpeg2000Variant_WithValidJP2_ShouldReturnJP2()
	{
		// Arrange
		var data = new byte[32];
		// JPEG2000 uses big-endian byte order
		data[0] = 0x00; data[1] = 0x00; data[2] = 0x00; data[3] = 0x0C; // Signature box size (12)
		"jP  "u8.ToArray().CopyTo(data, 4); // Signature box type
		new byte[] { 0x0D, 0x0A, 0x87, 0x0A }.CopyTo(data, 8); // Signature data
		data[12] = 0x00; data[13] = 0x00; data[14] = 0x00; data[15] = 0x14; // File type box size (20)
		"ftyp"u8.ToArray().CopyTo(data, 16); // File type box type
		"jp2 "u8.ToArray().CopyTo(data, 20); // JP2 brand

		// Act
		var variant = Jpeg2000Validator.DetectJpeg2000Variant(data);

		// Assert
		Assert.Equal("JP2", variant);
	}

	[Fact]
	public void DetectJpeg2000Variant_WithInvalidSignature_ShouldReturnEmpty()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		// Act
		var variant = Jpeg2000Validator.DetectJpeg2000Variant(data);

		// Assert
		Assert.Equal(string.Empty, variant);
	}

	[Fact]
	public void ValidationResult_GetSummary_ShouldReturnCorrectSummary()
	{
		// Arrange
		var result = new Jpeg2000ValidationResult();
		result.AddError("Test error");
		result.AddWarning("Test warning");

		// Act
		var summary = result.GetSummary();

		// Assert
		Assert.Contains("Invalid JPEG2000 configuration", summary);
		Assert.Contains("1 error(s)", summary);
		Assert.Contains("1 warning(s)", summary);
	}

	[Fact]
	public void ValidationResult_GetFormattedResults_ShouldIncludeAllIssues()
	{
		// Arrange
		var result = new Jpeg2000ValidationResult();
		result.AddError("Test error");
		result.AddWarning("Test warning");

		// Act
		var formatted = result.GetFormattedResults();

		// Assert
		Assert.Contains("Test error", formatted);
		Assert.Contains("Test warning", formatted);
		Assert.Contains("Errors:", formatted);
		Assert.Contains("Warnings:", formatted);
	}
}