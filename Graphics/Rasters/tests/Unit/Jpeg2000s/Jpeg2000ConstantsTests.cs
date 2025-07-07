// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

public class Jpeg2000ConstantsTests
{
	[Fact]
	public void SignatureBoxType_ShouldHaveCorrectValue()
	{
		// Arrange & Act
		var signatureBoxType = Jpeg2000Constants.SignatureBoxType;

		// Assert
		Assert.Equal(4, signatureBoxType.Length);
		Assert.Equal("jP  "u8.ToArray(), signatureBoxType.ToArray());
	}

	[Fact]
	public void SignatureData_ShouldHaveCorrectValue()
	{
		// Arrange & Act
		var signatureData = Jpeg2000Constants.SignatureData;

		// Assert
		Assert.Equal(4, signatureData.Length);
		Assert.Equal(new byte[] { 0x0D, 0x0A, 0x87, 0x0A }, signatureData.ToArray());
	}

	[Fact]
	public void FileTypeBoxType_ShouldHaveCorrectValue()
	{
		// Arrange & Act
		var fileTypeBoxType = Jpeg2000Constants.FileTypeBoxType;

		// Assert
		Assert.Equal(4, fileTypeBoxType.Length);
		Assert.Equal("ftyp"u8.ToArray(), fileTypeBoxType.ToArray());
	}

	[Fact]
	public void Jp2Brand_ShouldHaveCorrectValue()
	{
		// Arrange & Act
		var jp2Brand = Jpeg2000Constants.Jp2Brand;

		// Assert
		Assert.Equal(4, jp2Brand.Length);
		Assert.Equal("jp2 "u8.ToArray(), jp2Brand.ToArray());
	}

	[Fact]
	public void BoxHeaderLengths_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(8, Jpeg2000Constants.BoxHeaderLength);
		Assert.Equal(16, Jpeg2000Constants.ExtendedBoxHeaderLength);
		Assert.Equal(4, Jpeg2000Constants.BoxSizeLength);
		Assert.Equal(4, Jpeg2000Constants.BoxTypeLength);
		Assert.Equal(8, Jpeg2000Constants.ExtendedBoxSizeLength);
	}

	[Fact]
	public void ImageHeaderDataLength_ShouldHaveCorrectValue()
	{
		// Assert
		Assert.Equal(14, Jpeg2000Constants.ImageHeaderDataLength);
	}

	[Fact]
	public void DimensionLimits_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1, Jpeg2000Constants.MinWidth);
		Assert.Equal(1, Jpeg2000Constants.MinHeight);
		Assert.Equal(int.MaxValue, Jpeg2000Constants.MaxWidth);
		Assert.Equal(int.MaxValue, Jpeg2000Constants.MaxHeight);
	}

	[Fact]
	public void ComponentLimits_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(16384, Jpeg2000Constants.MaxComponents);
		Assert.Equal(38, Jpeg2000Constants.MaxBitDepth);
	}

	[Fact]
	public void DefaultValues_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(20.0f, Jpeg2000Constants.DefaultCompressionRatio);
		Assert.Equal(5, Jpeg2000Constants.DefaultDecompositionLevels);
		Assert.Equal(32, Jpeg2000Constants.MaxDecompositionLevels);
		Assert.Equal(1024, Jpeg2000Constants.DefaultTileSize);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.Markers.StartOfCodestream, 0xFF4F)]
	[InlineData(Jpeg2000Constants.Markers.EndOfCodestream, 0xFFD9)]
	[InlineData(Jpeg2000Constants.Markers.CodingStyleDefault, 0xFF52)]
	[InlineData(Jpeg2000Constants.Markers.QuantizationDefault, 0xFF5C)]
	public void Markers_ShouldHaveCorrectValues(ushort actual, ushort expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.ColorSpaces.Grayscale, 17)]
	[InlineData(Jpeg2000Constants.ColorSpaces.sRGB, 16)]
	[InlineData(Jpeg2000Constants.ColorSpaces.YCC, 18)]
	[InlineData(Jpeg2000Constants.ColorSpaces.RestrictedICC, 2)]
	[InlineData(Jpeg2000Constants.ColorSpaces.AnyICC, 3)]
	[InlineData(Jpeg2000Constants.ColorSpaces.VendorColor, 4)]
	public void ColorSpaces_ShouldHaveCorrectValues(ushort actual, ushort expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.ProgressionOrders.LRCP, 0)]
	[InlineData(Jpeg2000Constants.ProgressionOrders.RLCP, 1)]
	[InlineData(Jpeg2000Constants.ProgressionOrders.RPCL, 2)]
	[InlineData(Jpeg2000Constants.ProgressionOrders.PCRL, 3)]
	[InlineData(Jpeg2000Constants.ProgressionOrders.CPRL, 4)]
	public void ProgressionOrders_ShouldHaveCorrectValues(byte actual, byte expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.WaveletTransforms.Irreversible97, 0)]
	[InlineData(Jpeg2000Constants.WaveletTransforms.Reversible53, 1)]
	public void WaveletTransforms_ShouldHaveCorrectValues(byte actual, byte expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.CompressionModes.Lossless, 0)]
	[InlineData(Jpeg2000Constants.CompressionModes.Lossy, 1)]
	public void CompressionModes_ShouldHaveCorrectValues(byte actual, byte expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.ChannelTypes.Color, 0)]
	[InlineData(Jpeg2000Constants.ChannelTypes.Opacity, 1)]
	[InlineData(Jpeg2000Constants.ChannelTypes.PremultipliedOpacity, 2)]
	[InlineData(Jpeg2000Constants.ChannelTypes.Unspecified, 65535)]
	public void ChannelTypes_ShouldHaveCorrectValues(ushort actual, ushort expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void GeoJp2_GeoTiffUuid_ShouldHaveCorrectValue()
	{
		// Arrange
		var expectedUuid = new byte[]
		{
			0xB1, 0x4B, 0xF8, 0xBD, 0x08, 0x3D, 0x4B, 0x43,
			0xA5, 0xAE, 0x8C, 0xD7, 0xD5, 0xA6, 0xCE, 0x03
		};

		// Act
		var actualUuid = Jpeg2000Constants.GeoJp2.GeoTiffUuid;

		// Assert
		Assert.Equal(16, actualUuid.Length);
		Assert.Equal(expectedUuid, actualUuid.ToArray());
	}

	[Fact]
	public void GeoJp2_GmlUuid_ShouldHaveCorrectValue()
	{
		// Arrange
		var expectedUuid = new byte[]
		{
			0x96, 0xA9, 0xF1, 0xF1, 0xDC, 0x98, 0x40, 0x2D,
			0xA7, 0xAE, 0xD6, 0x8E, 0x34, 0x45, 0x18, 0x09
		};

		// Act
		var actualUuid = Jpeg2000Constants.GeoJp2.GmlUuid;

		// Assert
		Assert.Equal(16, actualUuid.Length);
		Assert.Equal(expectedUuid, actualUuid.ToArray());
	}

	[Fact]
	public void QualityLayers_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1, Jpeg2000Constants.QualityLayers.MinLayers);
		Assert.Equal(65535, Jpeg2000Constants.QualityLayers.MaxLayers);
		Assert.Equal(5, Jpeg2000Constants.QualityLayers.DefaultLayers);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.ResolutionLevels.FullResolution, 0)]
	[InlineData(Jpeg2000Constants.ResolutionLevels.HalfResolution, 1)]
	[InlineData(Jpeg2000Constants.ResolutionLevels.QuarterResolution, 2)]
	[InlineData(Jpeg2000Constants.ResolutionLevels.EighthResolution, 3)]
	public void ResolutionLevels_ShouldHaveCorrectValues(int actual, int expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Jpeg2000Constants.ErrorResilience.None, 0x00)]
	[InlineData(Jpeg2000Constants.ErrorResilience.SegmentationMarkers, 0x01)]
	[InlineData(Jpeg2000Constants.ErrorResilience.RestartMarkers, 0x02)]
	[InlineData(Jpeg2000Constants.ErrorResilience.TerminateOnEachPass, 0x04)]
	[InlineData(Jpeg2000Constants.ErrorResilience.VerticallyStreaming, 0x08)]
	[InlineData(Jpeg2000Constants.ErrorResilience.PredictableTermination, 0x10)]
	[InlineData(Jpeg2000Constants.ErrorResilience.ErrorResilienceTier1, 0x20)]
	public void ErrorResilience_ShouldHaveCorrectValues(byte actual, byte expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Memory_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(256, Jpeg2000Constants.Memory.DefaultTileCacheSizeMB);
		Assert.Equal(64, Jpeg2000Constants.Memory.DefaultDecodedTileCacheCapacity);
		Assert.Equal(256, Jpeg2000Constants.Memory.MinEfficientTileSize);
		Assert.Equal(4096, Jpeg2000Constants.Memory.MaxEfficientTileSize);
	}

	[Fact]
	public void AllBoxTypes_ShouldHaveFourByteLength()
	{
		// Arrange
		var boxTypes = new[]
		{
			Jpeg2000Constants.SignatureBoxType,
			Jpeg2000Constants.FileTypeBoxType,
			Jpeg2000Constants.HeaderBoxType,
			Jpeg2000Constants.ImageHeaderBoxType,
			Jpeg2000Constants.ColorSpecBoxType,
			Jpeg2000Constants.PaletteBoxType,
			Jpeg2000Constants.ComponentMappingBoxType,
			Jpeg2000Constants.ChannelDefinitionBoxType,
			Jpeg2000Constants.ResolutionBoxType,
			Jpeg2000Constants.CaptureResolutionBoxType,
			Jpeg2000Constants.DisplayResolutionBoxType,
			Jpeg2000Constants.CodestreamBoxType,
			Jpeg2000Constants.UuidBoxType,
			Jpeg2000Constants.XmlBoxType,
			Jpeg2000Constants.IntellectualPropertyBoxType
		};

		// Act & Assert
		foreach (var boxType in boxTypes)
		{
			Assert.Equal(4, boxType.Length);
		}
	}

	[Fact]
	public void ConstantsAreImmutable_ShouldNotAllowModification()
	{
		// Arrange
		var originalSignature = Jpeg2000Constants.SignatureBoxType;

		// Assert - Verify the original array remains unchanged
		Assert.True(originalSignature.IsDefaultOrEmpty == false);
		Assert.NotEmpty(originalSignature);
		Assert.Equal("jP  "u8.ToArray(), originalSignature.ToArray());
	}
}
