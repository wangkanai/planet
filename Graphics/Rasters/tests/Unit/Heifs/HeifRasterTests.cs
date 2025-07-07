// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Heifs;

public class HeifRasterTests
{
	[Fact]
	public void Constructor_WithValidDimensions_SetsProperties()
	{
		// Arrange & Act
		var heif = new HeifRaster(1920, 1080, true);

		// Assert
		Assert.Equal(1920, heif.Width);
		Assert.Equal(1080, heif.Height);
		Assert.True(heif.HasAlpha);
		Assert.Equal(HeifColorSpace.Srgb, heif.ColorSpace);
		Assert.Equal(HeifConstants.DefaultQuality, heif.Quality);
		Assert.Equal(HeifConstants.MinBitDepth, heif.BitDepth);
		Assert.Equal(HeifChromaSubsampling.Yuv420, heif.ChromaSubsampling);
		Assert.Equal(HeifConstants.DefaultSpeed, heif.Speed);
		Assert.Equal(HeifCompression.Hevc, heif.Compression);
		Assert.False(heif.IsLossless);
		Assert.Equal(HeifConstants.DefaultThreadCount, heif.ThreadCount);
		Assert.Equal(HeifProfile.Main, heif.Profile);
		Assert.False(heif.EnableProgressiveDecoding);
		Assert.True(heif.GenerateThumbnails);
		Assert.NotNull(heif.Metadata);
	}

	[Fact]
	public void Constructor_WithEncodedData_InitializesCorrectly()
	{
		// Arrange
		var data = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 };

		// Act
		var heif = new HeifRaster(data);

		// Assert
		Assert.Equal(HeifColorSpace.Srgb, heif.ColorSpace);
		Assert.Equal(HeifConstants.DefaultQuality, heif.Quality);
		Assert.NotNull(heif.Metadata);
	}

	[Fact]
	public void Constructor_WithNullEncodedData_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new HeifRaster(null!));
	}

	[Fact]
	public void HasHdrMetadata_WithoutHdrData_ReturnsFalse()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		Assert.False(heif.HasHdrMetadata);
	}

	[Fact]
	public void HasHdrMetadata_WithHdrData_ReturnsTrue()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var hdrMetadata = new HdrMetadata();

		// Act
		heif.SetHdrMetadata(hdrMetadata);

		// Assert
		Assert.True(heif.HasHdrMetadata);
	}

	[Fact]
	public void SetHdrMetadata_WithValidData_SetsMetadata()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var hdrMetadata = new HdrMetadata
		{
			MaxLuminance = 1000.0,
			MinLuminance = 0.005
		};

		// Act
		heif.SetHdrMetadata(hdrMetadata);

		// Assert
		Assert.Equal(hdrMetadata, heif.Metadata.HdrMetadata);
		Assert.True(heif.HasHdrMetadata);
	}

	[Fact]
	public void SetHdrMetadata_WithNullData_ThrowsArgumentNullException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => heif.SetHdrMetadata(null!));
	}

	[Fact]
	public async Task EncodeAsync_WithDefaultOptions_ReturnsValidData()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var result = await heif.EncodeAsync();

		// Assert
		Assert.NotNull(result);
		Assert.True(result.Length > 0);
		// Check for HEIF signature (ftyp box)
		Assert.Equal(0x66, result[4]); // 'f'
		Assert.Equal(0x74, result[5]); // 't'
		Assert.Equal(0x79, result[6]); // 'y'
		Assert.Equal(0x70, result[7]); // 'p'
		Assert.Equal(0x68, result[8]); // 'h'
		Assert.Equal(0x65, result[9]); // 'e'
		Assert.Equal(0x69, result[10]); // 'i'
		Assert.Equal(0x63, result[11]); // 'c'
	}

	[Fact]
	public async Task EncodeAsync_WithCustomOptions_ReturnsValidData()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var options = HeifEncodingOptions.CreateWebOptimized();

		// Act
		var result = await heif.EncodeAsync(options);

		// Assert
		Assert.NotNull(result);
		Assert.True(result.Length > 0);
	}

	[Fact]
	public async Task EncodeAsync_WithInvalidOptions_ThrowsInvalidOperationException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var options = new HeifEncodingOptions { Quality = -1 }; // Invalid quality

		// Act & Assert
		await Assert.ThrowsAsync<InvalidOperationException>(() => heif.EncodeAsync(options));
	}

	[Fact]
	public async Task DecodeAsync_WithValidData_SetsProperties()
	{
		// Arrange
		var heif = new HeifRaster(0, 0);
		var data = new byte[HeifConstants.BoxHeaderSize + 4];
		data[4] = 0x66; data[5] = 0x74; data[6] = 0x79; data[7] = 0x70; // ftyp

		// Act
		await heif.DecodeAsync(data);

		// Assert
		Assert.Equal(1920, heif.Width); // Default width set by decode
		Assert.Equal(1080, heif.Height); // Default height set by decode
	}

	[Fact]
	public async Task DecodeAsync_WithNullData_ThrowsArgumentNullException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => heif.DecodeAsync(null!));
	}

	[Fact]
	public async Task DecodeAsync_WithTooSmallData_ThrowsArgumentException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var data = new byte[4]; // Too small

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => heif.DecodeAsync(data));
	}

	[Fact]
	public void GetEstimatedFileSize_WithDefaultSettings_ReturnsReasonableSize()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var size = heif.GetEstimatedFileSize();

		// Assert
		Assert.True(size > 0);
		Assert.True(size < 50_000_000); // Less than 50MB for reasonable compression
	}

	[Fact]
	public void GetEstimatedFileSize_WithLosslessSettings_ReturnsLargerSize()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080)
		{
			IsLossless = true,
			Quality = HeifConstants.QualityPresets.Lossless
		};

		// Act
		var size = heif.GetEstimatedFileSize();

		// Assert
		Assert.True(size > 0);
	}

	[Fact]
	public void IsValid_WithValidConfiguration_ReturnsTrue()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		Assert.True(heif.IsValid());
	}

	[Theory]
	[InlineData(0, 1080)] // Invalid width
	[InlineData(1920, 0)] // Invalid height
	[InlineData(-1, 1080)] // Negative width
	[InlineData(1920, -1)] // Negative height
	[InlineData(100000, 1080)] // Width too large
	[InlineData(1920, 100000)] // Height too large
	public void IsValid_WithInvalidDimensions_ReturnsFalse(int width, int height)
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080) { Width = width, Height = height };

		// Act & Assert
		Assert.False(heif.IsValid());
	}

	[Theory]
	[InlineData(-1)] // Below minimum
	[InlineData(101)] // Above maximum
	public void IsValid_WithInvalidQuality_ReturnsFalse(int quality)
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080) { Quality = quality };

		// Act & Assert
		Assert.False(heif.IsValid());
	}

	[Theory]
	[InlineData(-1)] // Below minimum
	[InlineData(10)] // Above maximum
	public void IsValid_WithInvalidSpeed_ReturnsFalse(int speed)
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080) { Speed = speed };

		// Act & Assert
		Assert.False(heif.IsValid());
	}

	[Theory]
	[InlineData(7)] // Below minimum
	[InlineData(17)] // Above maximum
	public void IsValid_WithInvalidBitDepth_ReturnsFalse(int bitDepth)
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080) { BitDepth = bitDepth };

		// Act & Assert
		Assert.False(heif.IsValid());
	}

	[Fact]
	public async Task CreateThumbnailAsync_WithValidParameters_ReturnsThumbnailData()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var thumbnail = await heif.CreateThumbnailAsync(320, 240);

		// Assert
		Assert.NotNull(thumbnail);
		Assert.True(thumbnail.Length > 0);
	}

	[Theory]
	[InlineData(0, 240)] // Invalid width
	[InlineData(320, 0)] // Invalid height
	[InlineData(-1, 240)] // Negative width
	[InlineData(320, -1)] // Negative height
	public async Task CreateThumbnailAsync_WithInvalidParameters_ThrowsArgumentOutOfRangeException(int maxWidth, int maxHeight)
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => heif.CreateThumbnailAsync(maxWidth, maxHeight));
	}

	[Fact]
	public void ApplyColorProfile_WithValidProfile_SetsMetadata()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var iccProfile = new byte[1024];

		// Act
		heif.ApplyColorProfile(iccProfile);

		// Assert
		Assert.Equal(iccProfile, heif.Metadata.IccProfile);
	}

	[Fact]
	public void ApplyColorProfile_WithNullProfile_ThrowsArgumentNullException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => heif.ApplyColorProfile(null!));
	}

	[Fact]
	public void GetSupportedFeatures_ReturnsExpectedFeatures()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var features = heif.GetSupportedFeatures();

		// Assert
		Assert.True(features.HasFlag(HeifFeatures.BasicCodec));
		Assert.True(features.HasFlag(HeifFeatures.TenBitDepth));
		Assert.True(features.HasFlag(HeifFeatures.HdrMetadata));
		Assert.True(features.HasFlag(HeifFeatures.AlphaChannel));
		Assert.True(features.HasFlag(HeifFeatures.MultiThreading));
	}

	[Fact]
	public void SetCodecParameters_WithValidParameters_SetsMetadata()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		var parameters = new Dictionary<string, object>
		{
			["profile"] = "Main",
			["level"] = "4.0"
		};

		// Act
		heif.SetCodecParameters(parameters);

		// Assert
		Assert.Equal(parameters, heif.Metadata.CodecParameters);
	}

	[Fact]
	public void SetCodecParameters_WithNullParameters_ThrowsArgumentNullException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => heif.SetCodecParameters(null!));
	}

	[Fact]
	public void GetContainerInfo_ReturnsValidInfo()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var info = heif.GetContainerInfo();

		// Assert
		Assert.NotNull(info);
		Assert.Equal("heic", info.MajorBrand);
		Assert.Contains("mif1", info.CompatibleBrands);
		Assert.Contains("miaf", info.CompatibleBrands);
		Assert.True(info.HasThumbnails);
		Assert.Equal(1, info.ItemCount);
		Assert.True(info.BoxCount > 0);
	}

	[Fact]
	public void EstimatedMetadataSize_WithoutMetadata_ReturnsZero()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act
		var size = heif.Metadata.EstimatedMetadataSize;

		// Assert
		Assert.Equal(0, size);
	}

	[Fact]
	public void EstimatedMetadataSize_WithMetadata_ReturnsCorrectSize()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		heif.Metadata.ExifData = new byte[1024];
		heif.Metadata.XmpData = "<x:xmpmeta>test metadata</x:xmpmeta>";
		heif.SetHdrMetadata(new HdrMetadata());

		// Act
		var size = heif.Metadata.EstimatedMetadataSize;

		// Assert
		Assert.True(size >= 1024); // Just check it's reasonable
	}

	[Fact]
	public void Dispose_MultipleCalls_DoesNotThrow()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		heif.Dispose();
		heif.Dispose(); // Should not throw
	}

	[Fact]
	public async Task DisposeAsync_MultipleCalls_DoesNotThrow()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);

		// Act & Assert
		await heif.DisposeAsync();
		await heif.DisposeAsync(); // Should not throw
	}

	[Fact]
	public void DisposedRaster_ThrowsObjectDisposedException()
	{
		// Arrange
		var heif = new HeifRaster(1920, 1080);
		heif.Dispose();

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => heif.GetEstimatedFileSize());
		Assert.Throws<ObjectDisposedException>(() => heif.IsValid());
		Assert.Throws<ObjectDisposedException>(() => heif.SetHdrMetadata(new HdrMetadata()));
	}
}
