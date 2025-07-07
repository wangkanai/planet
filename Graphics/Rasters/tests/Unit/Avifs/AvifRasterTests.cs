// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Avifs;

public class AvifRasterTests
{
	[Fact]
	public void Constructor_WithoutParameters_ShouldInitializeDefaults()
	{
		using var avif = new AvifRaster();

		Assert.Equal(0, avif.Width);
		Assert.Equal(0, avif.Height);
		Assert.Equal(8, avif.BitDepth);
		Assert.Equal(AvifColorSpace.Srgb, avif.ColorSpace);
		Assert.False(avif.HasAlpha);
		Assert.False(avif.IsLossless);
		Assert.NotNull(avif.Metadata);
	}

	[Theory]
	[InlineData(1920, 1080, false)]
	[InlineData(3840, 2160, true)]
	[InlineData(100, 100, false)]
	public void Constructor_WithValidDimensions_ShouldSetCorrectly(int width, int height, bool hasAlpha)
	{
		using var avif = new AvifRaster(width, height, hasAlpha);

		Assert.Equal(width, avif.Width);
		Assert.Equal(height, avif.Height);
		Assert.Equal(hasAlpha, avif.HasAlpha);
		Assert.Equal(width, avif.Metadata.Width);
		Assert.Equal(height, avif.Metadata.Height);
		Assert.Equal(hasAlpha, avif.Metadata.HasAlpha);
	}

	[Theory]
	[InlineData(0, 100)]
	[InlineData(100, 0)]
	[InlineData(-1, 100)]
	[InlineData(100, -1)]
	[InlineData(AvifConstants.MaxDimension + 1, 100)]
	[InlineData(100, AvifConstants.MaxDimension + 1)]
	public void Constructor_WithInvalidDimensions_ShouldThrowArgumentException(int width, int height)
	{
		Assert.Throws<ArgumentException>(() => new AvifRaster(width, height));
	}

	[Theory]
	[InlineData(8)]
	[InlineData(10)]
	[InlineData(12)]
	public void BitDepth_WithValidValues_ShouldSet(int bitDepth)
	{
		using var avif = new AvifRaster(100, 100);
		
		avif.BitDepth = bitDepth;
		
		Assert.Equal(bitDepth, avif.BitDepth);
		Assert.Equal(bitDepth, avif.Metadata.BitDepth);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(9)]
	[InlineData(16)]
	[InlineData(24)]
	public void BitDepth_WithInvalidValues_ShouldThrowArgumentException(int bitDepth)
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Throws<ArgumentException>(() => avif.BitDepth = bitDepth);
	}

	[Theory]
	[InlineData(AvifConstants.MinQuality)]
	[InlineData(50)]
	[InlineData(AvifConstants.MaxQuality)]
	public void Quality_WithValidValues_ShouldSet(int quality)
	{
		using var avif = new AvifRaster(100, 100);
		
		avif.Quality = quality;
		
		Assert.Equal(quality, avif.Quality);
		Assert.Equal(quality, avif.Metadata.Quality);
	}

	[Theory]
	[InlineData(AvifConstants.MinQuality - 1)]
	[InlineData(AvifConstants.MaxQuality + 1)]
	[InlineData(-10)]
	[InlineData(150)]
	public void Quality_WithInvalidValues_ShouldThrowArgumentException(int quality)
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Throws<ArgumentException>(() => avif.Quality = quality);
	}

	[Theory]
	[InlineData(AvifConstants.MinSpeed)]
	[InlineData(5)]
	[InlineData(AvifConstants.MaxSpeed)]
	public void Speed_WithValidValues_ShouldSet(int speed)
	{
		using var avif = new AvifRaster(100, 100);
		
		avif.Speed = speed;
		
		Assert.Equal(speed, avif.Speed);
		Assert.Equal(speed, avif.Metadata.Speed);
	}

	[Theory]
	[InlineData(AvifConstants.MinSpeed - 1)]
	[InlineData(AvifConstants.MaxSpeed + 1)]
	[InlineData(-1)]
	[InlineData(20)]
	public void Speed_WithInvalidValues_ShouldThrowArgumentException(int speed)
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Throws<ArgumentException>(() => avif.Speed = speed);
	}

	[Fact]
	public void IsLossless_WhenSetToTrue_ShouldUpdateQualityAndChromaSubsampling()
	{
		using var avif = new AvifRaster(100, 100)
		{
			Quality = 50,
			ChromaSubsampling = AvifChromaSubsampling.Yuv420
		};

		avif.IsLossless = true;

		Assert.True(avif.IsLossless);
		Assert.Equal(AvifConstants.QualityPresets.Lossless, avif.Quality);
		Assert.Equal(AvifChromaSubsampling.Yuv444, avif.ChromaSubsampling);
	}

	[Fact]
	public void HasAlpha_ShouldSyncWithMetadata()
	{
		using var avif = new AvifRaster(100, 100);

		avif.HasAlpha = true;

		Assert.True(avif.HasAlpha);
		Assert.True(avif.Metadata.HasAlpha);
	}

	[Theory]
	[InlineData(AvifColorSpace.Srgb)]
	[InlineData(AvifColorSpace.DisplayP3)]
	[InlineData(AvifColorSpace.Bt2020Ncl)]
	[InlineData(AvifColorSpace.Bt2100Pq)]
	[InlineData(AvifColorSpace.Bt2100Hlg)]
	public void ColorSpace_WithValidValues_ShouldSet(AvifColorSpace colorSpace)
	{
		using var avif = new AvifRaster(100, 100);
		
		avif.ColorSpace = colorSpace;
		
		Assert.Equal(colorSpace, avif.ColorSpace);
		Assert.Equal(colorSpace, avif.Metadata.ColorSpace);
	}

	[Theory]
	[InlineData(AvifChromaSubsampling.Yuv444)]
	[InlineData(AvifChromaSubsampling.Yuv422)]
	[InlineData(AvifChromaSubsampling.Yuv420)]
	[InlineData(AvifChromaSubsampling.Yuv400)]
	public void ChromaSubsampling_WithValidValues_ShouldSet(AvifChromaSubsampling subsampling)
	{
		using var avif = new AvifRaster(100, 100);
		
		avif.ChromaSubsampling = subsampling;
		
		Assert.Equal(subsampling, avif.ChromaSubsampling);
		Assert.Equal(subsampling, avif.Metadata.ChromaSubsampling);
	}

	[Fact]
	public void EnableFilmGrain_ShouldSyncWithMetadata()
	{
		using var avif = new AvifRaster(100, 100);

		avif.EnableFilmGrain = true;

		Assert.True(avif.EnableFilmGrain);
		Assert.True(avif.Metadata.UsesFilmGrain);
	}

	[Fact]
	public void SetHdrMetadata_WithValidMetadata_ShouldUpdateColorSpaceAndBitDepth()
	{
		using var avif = new AvifRaster(100, 100);
		var hdrMetadata = new HdrMetadata
		{
			Format = HdrFormat.Hdr10,
			MaxLuminance = 4000.0,
			MinLuminance = 0.01
		};

		avif.SetHdrMetadata(hdrMetadata);

		Assert.True(avif.HasHdrMetadata);
		Assert.Same(hdrMetadata, avif.Metadata.HdrInfo);
		Assert.Equal(AvifColorSpace.Bt2100Pq, avif.ColorSpace);
		Assert.Equal(10, avif.BitDepth);
	}

	[Fact]
	public void SetHdrMetadata_WithHlgFormat_ShouldSetCorrectColorSpace()
	{
		using var avif = new AvifRaster(100, 100);
		var hdrMetadata = new HdrMetadata
		{
			Format = HdrFormat.Hlg,
			MaxLuminance = 1000.0,
			MinLuminance = 0.005
		};

		avif.SetHdrMetadata(hdrMetadata);

		Assert.Equal(AvifColorSpace.Bt2100Hlg, avif.ColorSpace);
		Assert.Equal(10, avif.BitDepth);
	}

	[Fact]
	public void SetHdrMetadata_WithNullMetadata_ShouldThrowArgumentNullException()
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Throws<ArgumentNullException>(() => avif.SetHdrMetadata(null!));
	}

	[Fact]
	public void GetEstimatedFileSize_ShouldReturnReasonableSize()
	{
		using var avif = new AvifRaster(1920, 1080);
		
		var estimatedSize = avif.GetEstimatedFileSize();
		
		Assert.True(estimatedSize > 0);
		Assert.True(estimatedSize < (long)1920 * 1080 * 3); // Should be compressed
	}

	[Fact]
	public void GetEstimatedFileSize_WithLossless_ShouldReturnLargerSize()
	{
		using var lossyAvif = new AvifRaster(1920, 1080) { IsLossless = false, Quality = 50 };
		using var losslessAvif = new AvifRaster(1920, 1080) { IsLossless = true };
		
		var lossySize = lossyAvif.GetEstimatedFileSize();
		var losslessSize = losslessAvif.GetEstimatedFileSize();
		
		Assert.True(losslessSize > lossySize);
	}

	[Fact]
	public void GetEstimatedFileSize_WithHigherBitDepth_ShouldReturnLargerSize()
	{
		using var avif8bit = new AvifRaster(1920, 1080) { BitDepth = 8 };
		using var avif10bit = new AvifRaster(1920, 1080) { BitDepth = 10 };
		
		var size8bit = avif8bit.GetEstimatedFileSize();
		var size10bit = avif10bit.GetEstimatedFileSize();
		
		Assert.True(size10bit > size8bit);
	}

	[Fact]
	public void GetEstimatedFileSize_WithAlpha_ShouldReturnLargerSize()
	{
		using var avifWithoutAlpha = new AvifRaster(1920, 1080, false);
		using var avifWithAlpha = new AvifRaster(1920, 1080, true);
		
		var sizeWithoutAlpha = avifWithoutAlpha.GetEstimatedFileSize();
		var sizeWithAlpha = avifWithAlpha.GetEstimatedFileSize();
		
		Assert.True(sizeWithAlpha > sizeWithoutAlpha);
	}

	[Fact]
	public void IsValid_WithValidConfiguration_ShouldReturnTrue()
	{
		using var avif = new AvifRaster(1920, 1080)
		{
			Quality = 85,
			BitDepth = 8,
			ColorSpace = AvifColorSpace.Srgb
		};
		
		Assert.True(avif.IsValid());
	}

	[Fact]
	public async Task EncodeAsync_WithValidConfiguration_ShouldReturnData()
	{
		using var avif = new AvifRaster(100, 100);
		
		var encodedData = await avif.EncodeAsync();
		
		Assert.NotNull(encodedData);
		Assert.True(encodedData.Length > 0);
	}

	[Fact]
	public async Task EncodeAsync_WithCustomOptions_ShouldUseOptions()
	{
		using var avif = new AvifRaster(100, 100);
		var options = new AvifEncodingOptions
		{
			Quality = 100, // Lossless mode requires quality to be 100
			Speed = AvifConstants.SpeedPresets.Slow,
			IsLossless = true
		};
		
		var encodedData = await avif.EncodeAsync(options);
		
		Assert.NotNull(encodedData);
		Assert.True(encodedData.Length > 0);
		Assert.Equal(100, avif.Quality); // Should be 100 for lossless
		Assert.True(avif.IsLossless);
	}

	[Fact]
	public async Task DecodeAsync_WithValidData_ShouldUpdateDimensions()
	{
		using var sourceAvif = new AvifRaster(200, 150);
		var encodedData = await sourceAvif.EncodeAsync();
		
		using var targetAvif = new AvifRaster();
		await targetAvif.DecodeAsync(encodedData);
		
		Assert.True(targetAvif.Width > 0);
		Assert.True(targetAvif.Height > 0);
	}

	[Theory]
	[InlineData(null)]
	[InlineData(new byte[0])]
	public async Task DecodeAsync_WithInvalidData_ShouldThrowArgumentException(byte[]? data)
	{
		using var avif = new AvifRaster();
		
		await Assert.ThrowsAsync<ArgumentException>(() => avif.DecodeAsync(data!));
	}

	[Fact]
	public async Task CreateThumbnailAsync_ShouldReturnValidData()
	{
		using var avif = new AvifRaster(1920, 1080);
		
		var thumbnailData = await avif.CreateThumbnailAsync(200, 200);
		
		Assert.NotNull(thumbnailData);
		Assert.True(thumbnailData.Length > 0);
	}

	[Theory]
	[InlineData(0, 100)]
	[InlineData(100, 0)]
	[InlineData(-1, 100)]
	[InlineData(100, -1)]
	public async Task CreateThumbnailAsync_WithInvalidDimensions_ShouldThrowArgumentException(int maxWidth, int maxHeight)
	{
		using var avif = new AvifRaster(1920, 1080);
		
		await Assert.ThrowsAsync<ArgumentException>(() => avif.CreateThumbnailAsync(maxWidth, maxHeight));
	}

	[Fact]
	public void ApplyColorProfile_WithValidProfile_ShouldSetIccProfile()
	{
		using var avif = new AvifRaster(100, 100);
		var iccProfile = new byte[] { 1, 2, 3, 4, 5 };
		
		avif.ApplyColorProfile(iccProfile);
		
		Assert.Same(iccProfile, avif.Metadata.IccProfile);
	}

	[Theory]
	[InlineData(null)]
	[InlineData(new byte[0])]
	public void ApplyColorProfile_WithInvalidProfile_ShouldThrowArgumentException(byte[]? profile)
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Throws<ArgumentException>(() => avif.ApplyColorProfile(profile!));
	}

	[Fact]
	public void GetSupportedFeatures_ShouldReturnValidFeatures()
	{
		using var avif = new AvifRaster(100, 100);
		
		var features = avif.GetSupportedFeatures();
		
		Assert.True(features.HasFlag(AvifFeatures.BasicCodec));
		Assert.True(features.HasFlag(AvifFeatures.TenBitDepth));
		Assert.True(features.HasFlag(AvifFeatures.AlphaChannel));
	}

	[Fact]
	public void HasLargeMetadata_WithLargeImage_ShouldReturnTrue()
	{
		using var avif = new AvifRaster(8000, 8000) { BitDepth = 12 };
		// Need actual large metadata content to trigger large metadata threshold
		avif.Metadata.ExifData = new byte[2 * 1024 * 1024]; // 2MB of EXIF data
		
		Assert.True(avif.HasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithSmallImage_ShouldReturnFalse()
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.False(avif.HasLargeMetadata);
	}

	[Fact]
	public void EstimatedMetadataSize_ShouldIncludeMetadataSize()
	{
		using var avif = new AvifRaster(1920, 1080);
		
		var estimatedSize = avif.EstimatedMetadataSize;
		
		Assert.True(estimatedSize > 0);
		Assert.True(estimatedSize >= avif.Metadata.EstimatedMemoryUsage);
	}

	[Fact]
	public void ThreadCount_ShouldDefaultToReasonableValue()
	{
		using var avif = new AvifRaster(100, 100);
		
		Assert.Equal(0, avif.ThreadCount); // Default is 0 (auto-detect)
	}

	[Fact]
	public void Dispose_ShouldNotThrowWhenCalledMultipleTimes()
	{
		var avif = new AvifRaster(100, 100);
		
		avif.Dispose();
		avif.Dispose(); // Should not throw
	}

	[Fact]
	public async Task DisposeAsync_ShouldNotThrowWhenCalledMultipleTimes()
	{
		var avif = new AvifRaster(100, 100);
		
		await avif.DisposeAsync();
		await avif.DisposeAsync(); // Should not throw
	}

	[Fact]
	public void AccessProperties_AfterDispose_ShouldThrowObjectDisposedException()
	{
		var avif = new AvifRaster(100, 100);
		avif.Dispose();
		
		// Implementation does throw ObjectDisposedException on property access
		Assert.Throws<ObjectDisposedException>(() => avif.Width);
		Assert.Throws<ObjectDisposedException>(() => avif.Height);
		Assert.Throws<ObjectDisposedException>(() => avif.Quality);
		Assert.Throws<ObjectDisposedException>(() => avif.BitDepth);
	}

	[Fact]
	public void SetProperties_AfterDispose_ShouldThrowObjectDisposedException()
	{
		var avif = new AvifRaster(100, 100);
		avif.Dispose();
		
		Assert.Throws<ObjectDisposedException>(() => avif.Quality = 50);
		Assert.Throws<ObjectDisposedException>(() => avif.BitDepth = 10);
		Assert.Throws<ObjectDisposedException>(() => avif.HasAlpha = true);
	}
}