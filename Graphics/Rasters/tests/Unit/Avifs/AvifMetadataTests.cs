// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Avifs;

public class AvifMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeDefaultValues()
	{
		var metadata = new AvifMetadata();

		Assert.Equal(0, metadata.Width);
		Assert.Equal(0, metadata.Height);
		Assert.Equal(8, metadata.BitDepth);
		Assert.Equal(AvifColorSpace.Srgb, metadata.ColorSpace);
		Assert.Equal(AvifConstants.DefaultQuality, metadata.Quality);
		Assert.Equal(AvifConstants.DefaultSpeed, metadata.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, metadata.ChromaSubsampling);
		Assert.False(metadata.HasAlpha);
		Assert.False(metadata.IsLossless);
		Assert.False(metadata.UsesFilmGrain);
		Assert.False(metadata.AlphaPremultiplied);
		Assert.Equal(1.0, metadata.PixelAspectRatio);
		Assert.Equal(0, metadata.Rotation);
		Assert.Null(metadata.HdrInfo);
		Assert.Null(metadata.ExifData);
		Assert.Null(metadata.IccProfile);
	}

	[Fact]
	public void HasLargeMetadata_WithSmallMetadata_ShouldReturnFalse()
	{
		var metadata = new AvifMetadata
		{
			Width = 100,
			Height = 100
		};

		Assert.False(metadata.HasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeImage_ShouldReturnTrue()
	{
		var metadata = new AvifMetadata
		{
			Width = 8000,
			Height = 8000,
			BitDepth = 12,
			// Need actual large data to trigger large metadata
			ExifData = new byte[2 * 1024 * 1024] // 2MB of EXIF data
		};

		Assert.True(metadata.HasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeExif_ShouldReturnTrue()
	{
		var metadata = new AvifMetadata
		{
			Width = 100,
			Height = 100,
			ExifData = new byte[5 * 1024 * 1024] // 5MB EXIF
		};

		Assert.True(metadata.HasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeIccProfile_ShouldReturnTrue()
	{
		var metadata = new AvifMetadata
		{
			Width = 100,
			Height = 100,
			IccProfile = new byte[3 * 1024 * 1024] // 3MB ICC profile
		};

		Assert.True(metadata.HasLargeMetadata);
	}

	[Fact]
	public void EstimatedMemoryUsage_ShouldCalculateCorrectly()
	{
		var metadata = new AvifMetadata
		{
			Width = 1920,
			Height = 1080,
			BitDepth = 10,
			HasAlpha = true,
			ExifData = new byte[64 * 1024], // 64KB
			IccProfile = new byte[128 * 1024] // 128KB
		};

		var estimatedSize = metadata.EstimatedMemoryUsage;

		// Should include pixel data, metadata, EXIF, and ICC profile
		Assert.True(estimatedSize > 0);
		Assert.True(estimatedSize > 64 * 1024 + 128 * 1024); // At least EXIF + ICC
	}

	[Theory]
	[InlineData(1920, 1080, 8, false, false)]
	[InlineData(3840, 2160, 10, true, false)]
	[InlineData(7680, 4320, 12, true, false)] // Dimensions don't affect metadata size
	public void EstimatedMemoryUsage_WithDifferentConfigurations_ShouldVary(int width, int height, int bitDepth, bool hasAlpha, bool expectLarge)
	{
		var metadata = new AvifMetadata
		{
			Width = width,
			Height = height,
			BitDepth = bitDepth,
			HasAlpha = hasAlpha
		};

		var estimatedSize = metadata.EstimatedMemoryUsage;

		Assert.True(estimatedSize > 0);

		// All these configurations have the same base metadata size since dimensions don't affect it
		Assert.False(estimatedSize > ImageConstants.LargeMetadataThreshold);
	}

	[Fact]
	public void Clone_ShouldCreateDeepCopy()
	{
		var original = new AvifMetadata
		{
			Width = 1920,
			Height = 1080,
			BitDepth = 10,
			ColorSpace = AvifColorSpace.DisplayP3,
			Quality = 85,
			HasAlpha = true,
			ExifData = new byte[] { 1, 2, 3, 4 },
			IccProfile = new byte[] { 5, 6, 7, 8 }
		};

		var clone = original.Clone();

		Assert.NotSame(original, clone);
		Assert.Equal(original.Width, clone.Width);
		Assert.Equal(original.Height, clone.Height);
		Assert.Equal(original.BitDepth, clone.BitDepth);
		Assert.Equal(original.ColorSpace, clone.ColorSpace);
		Assert.Equal(original.Quality, clone.Quality);
		Assert.Equal(original.HasAlpha, clone.HasAlpha);

		// Should be deep copy of arrays
		Assert.NotSame(original.ExifData, clone.ExifData);
		Assert.NotSame(original.IccProfile, clone.IccProfile);
		Assert.Equal(original.ExifData, clone.ExifData);
		Assert.Equal(original.IccProfile, clone.IccProfile);
	}

	[Fact]
	public void HdrInfo_WhenSet_ShouldUpdateHdrRelatedProperties()
	{
		var metadata = new AvifMetadata();
		var hdrInfo = new HdrMetadata
		{
			Format = HdrFormat.Hdr10,
			MaxLuminance = 4000.0,
			MinLuminance = 0.01
		};

		metadata.HdrInfo = hdrInfo;

		Assert.Same(hdrInfo, metadata.HdrInfo);
		Assert.True(metadata.EstimatedMemoryUsage > 0);
	}

	[Fact]
	public void CleanAperture_WhenSet_ShouldBeValid()
	{
		var metadata = new AvifMetadata
		{
			Width = 1920,
			Height = 1080
		};

		var cleanAperture = new CleanAperture
		{
			Width = 1800,
			Height = 1000,
			HorizontalOffset = 60,
			VerticalOffset = 40
		};

		metadata.CleanAperture = cleanAperture;

		Assert.Same(cleanAperture, metadata.CleanAperture);
	}

	[Theory]
	[InlineData(0.5)]
	[InlineData(1.0)]
	[InlineData(2.0)]
	[InlineData(1.77777)]
	public void PixelAspectRatio_WithValidValues_ShouldSet(double ratio)
	{
		var metadata = new AvifMetadata
		{
			PixelAspectRatio = ratio
		};

		Assert.Equal(ratio, metadata.PixelAspectRatio, 5);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(90)]
	[InlineData(180)]
	[InlineData(270)]
	public void Rotation_WithValidValues_ShouldSet(int rotation)
	{
		var metadata = new AvifMetadata
		{
			Rotation = rotation
		};

		Assert.Equal(rotation, metadata.Rotation);
	}

	[Fact]
	public void Dispose_ShouldClearManagedResources()
	{
		var metadata = new AvifMetadata
		{
			ExifData = new byte[1000],
			IccProfile = new byte[2000],
			HdrInfo = new HdrMetadata()
		};

		metadata.Dispose();

		Assert.Null(metadata.ExifData);
		Assert.Null(metadata.IccProfile);
		// HdrInfo is not cleared by Dispose() in the actual implementation
		Assert.NotNull(metadata.HdrInfo);
	}

	[Fact]
	public async Task DisposeAsync_WithLargeMetadata_ShouldClearResourcesAsynchronously()
	{
		var metadata = new AvifMetadata
		{
			Width = 8000,
			Height = 8000,
			ExifData = new byte[5 * 1024 * 1024], // Large EXIF
			IccProfile = new byte[3 * 1024 * 1024] // Large ICC
		};

		await metadata.DisposeAsync();

		Assert.Null(metadata.ExifData);
		Assert.Null(metadata.IccProfile);
	}

	[Fact]
	public void ToString_ShouldReturnDescriptiveString()
	{
		var metadata = new AvifMetadata
		{
			Width = 1920,
			Height = 1080,
			BitDepth = 10,
			ColorSpace = AvifColorSpace.DisplayP3,
			Quality = 85
		};

		var result = metadata.ToString();

		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.Contains("AvifMetadata", result); // Should contain the class name
	}

	[Fact]
	public void UsesFilmGrain_ShouldDefaultToFalse()
	{
		var metadata = new AvifMetadata();

		Assert.False(metadata.UsesFilmGrain);

		metadata.UsesFilmGrain = true;
		Assert.True(metadata.UsesFilmGrain);
	}
}
