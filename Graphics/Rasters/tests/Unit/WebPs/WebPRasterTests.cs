// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.WebPs;

namespace Wangkanai.Graphics.Rasters.WebPs;

public class WebPRasterTests
{
	[Fact]
	public void Constructor_Default_ShouldSetDefaultValues()
	{
		// Act
		var webp = new WebPRaster();

		// Assert
		Assert.Equal(1, webp.Width);
		Assert.Equal(1, webp.Height);
		Assert.Equal(WebPFormat.Simple, webp.Format);
		Assert.Equal(WebPCompression.VP8, webp.Compression);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.Equal(WebPConstants.DefaultQuality, webp.Quality);
		Assert.Equal(WebPConstants.DefaultCompressionLevel, webp.CompressionLevel);
		Assert.Equal(WebPPreset.Default, webp.Preset);
		Assert.Equal(WebPConstants.RgbChannels, webp.Channels);
		Assert.False(webp.HasAlpha);
		Assert.False(webp.IsLossless);
		Assert.False(webp.IsAnimated);
		Assert.NotNull(webp.Metadata);
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1920, 1080)]
	[InlineData(100, 100)]
	public void Constructor_WithDimensions_ShouldSetDimensions(int width, int height)
	{
		// Act
		var webp = new WebPRaster(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
	}

	[Theory]
	[InlineData(0, 1)]
	[InlineData(-10, 1)]
	[InlineData(20000, 16383)]
	public void Constructor_WithInvalidDimensions_ShouldClampToValidRange(int input, int expected)
	{
		// Act
		var webp = new WebPRaster(input, 100);

		// Assert
		Assert.Equal(expected, webp.Width);
	}

	[Theory]
	[InlineData(800, 600, 85)]
	[InlineData(1024, 768, 90)]
	public void Constructor_WithQuality_ShouldSetQuality(int width, int height, int quality)
	{
		// Act
		var webp = new WebPRaster(width, height, quality);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(quality, webp.Quality);
	}

	[Theory]
	[InlineData(-10, 0)]
	[InlineData(0, 0)]
	[InlineData(50, 50)]
	[InlineData(100, 100)]
	[InlineData(150, 100)]
	public void Quality_ShouldClampToValidRange(int input, int expected)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.Quality = input;

		// Assert
		Assert.Equal(expected, webp.Quality);
	}

	[Theory]
	[InlineData(-5, 0)]
	[InlineData(0, 0)]
	[InlineData(6, 6)]
	[InlineData(9, 9)]
	[InlineData(15, 9)]
	public void CompressionLevel_ShouldClampToValidRange(int input, int expected)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.CompressionLevel = input;

		// Assert
		Assert.Equal(expected, webp.CompressionLevel);
	}

	[Theory]
	[InlineData(WebPColorMode.Rgb, 3, false)]
	[InlineData(WebPColorMode.Rgba, 4, true)]
	public void SetColorMode_ShouldUpdateRelatedProperties(WebPColorMode colorMode, int expectedChannels, bool expectedAlpha)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.SetColorMode(colorMode);

		// Assert
		Assert.Equal(colorMode, webp.ColorMode);
		Assert.Equal(expectedChannels, webp.Channels);
		Assert.Equal(expectedAlpha, webp.HasAlpha);
		Assert.Equal(expectedAlpha, webp.Metadata.HasAlpha);
	}

	[Fact]
	public void ConfigureLossless_ShouldSetLosslessProperties()
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.ConfigureLossless();

		// Assert
		Assert.Equal(WebPCompression.VP8L, webp.Compression);
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.True(webp.IsLossless);
		Assert.Equal(WebPConstants.DefaultCompressionLevel, webp.CompressionLevel);
	}

	[Theory]
	[InlineData(50)]
	[InlineData(75)]
	[InlineData(90)]
	public void ConfigureLossy_ShouldSetLossyProperties(int quality)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.ConfigureLossy(quality);

		// Assert
		Assert.Equal(WebPCompression.VP8, webp.Compression);
		Assert.Equal(WebPFormat.Simple, webp.Format);
		Assert.False(webp.IsLossless);
		Assert.Equal(quality, webp.Quality);
	}

	[Fact]
	public void EnableExtendedFeatures_ShouldSetExtendedFormat()
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.EnableExtendedFeatures();

		// Assert
		Assert.Equal(WebPFormat.Extended, webp.Format);
		Assert.True(webp.Metadata.IsExtended);
	}

	[Theory]
	[InlineData(WebPPreset.Picture)]
	[InlineData(WebPPreset.Photo)]
	[InlineData(WebPPreset.Drawing)]
	[InlineData(WebPPreset.Icon)]
	[InlineData(WebPPreset.Text)]
	public void Preset_ShouldApplyOptimizations(WebPPreset preset)
	{
		// Arrange
		var webp = new WebPRaster(800, 600, 50);
		var originalQuality = webp.Quality;

		// Act
		webp.Preset = preset;

		// Assert
		Assert.Equal(preset, webp.Preset);

		// Verify preset-specific optimizations
		switch (preset)
		{
			case WebPPreset.Picture:
				Assert.True(webp.Quality >= 80 || webp.Quality >= originalQuality);
				break;
			case WebPPreset.Photo:
				Assert.True(webp.Quality >= 85 || webp.Quality >= originalQuality);
				break;
			case WebPPreset.Icon:
			case WebPPreset.Text:
				Assert.True(webp.IsLossless);
				break;
		}
	}

	[Fact]
	public void IsValid_WithValidData_ShouldReturnTrue()
	{
		// Arrange
		var webp = new WebPRaster(800, 600, 75);

		// Act & Assert
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(0, 600)]
	[InlineData(800, 0)]
	[InlineData(-1, 600)]
	[InlineData(800, -1)]
	[InlineData(20000, 600)]
	[InlineData(800, 20000)]
	public void IsValid_WithInvalidDimensions_ShouldReturnFalse(int width, int height)
	{
		// Arrange
		var webp = new WebPRaster { Width = width, Height = height };

		// Act & Assert
		Assert.False(webp.IsValid());
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(101, 100)]
	public void Quality_WithInvalidValue_ShouldClampToValidRange(int invalidQuality, int expectedQuality)
	{
		// Arrange
		var webp = new WebPRaster(800, 600) { Quality = invalidQuality };

		// Act & Assert
		Assert.Equal(expectedQuality, webp.Quality);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(10, 9)]
	public void CompressionLevel_WithInvalidValue_ShouldClampToValidRange(int invalidLevel, int expectedLevel)
	{
		// Arrange
		var webp = new WebPRaster(800, 600) { CompressionLevel = invalidLevel };

		// Act & Assert
		Assert.Equal(expectedLevel, webp.CompressionLevel);
		Assert.True(webp.IsValid());
	}

	[Fact]
	public void GetEstimatedFileSize_WithValidData_ShouldReturnPositiveValue()
	{
		// Arrange
		var webp = new WebPRaster(800, 600, 75);

		// Act
		var fileSize = webp.GetEstimatedFileSize();

		// Assert
		Assert.True(fileSize > 0);
		Assert.True(fileSize > WebPConstants.ContainerOverhead);
	}

	[Fact]
	public void GetEstimatedFileSize_WithInvalidData_ShouldReturnZero()
	{
		// Arrange
		var webp = new WebPRaster { Width = 0, Height = 0 };

		// Act
		var fileSize = webp.GetEstimatedFileSize();

		// Assert
		Assert.Equal(0, fileSize);
	}

	[Theory]
	[InlineData(WebPColorMode.Rgb, WebPCompression.VP8, 75)]
	[InlineData(WebPColorMode.Rgba, WebPCompression.VP8, 85)]
	[InlineData(WebPColorMode.Rgb, WebPCompression.VP8L, 6)]
	public void GetEstimatedFileSize_ShouldVaryBySettings(WebPColorMode colorMode, WebPCompression compression, int setting)
	{
		// Arrange
		var webp1 = new WebPRaster(800, 600);
		var webp2 = new WebPRaster(800, 600);

		webp1.SetColorMode(colorMode);
		webp1.Compression = compression;
		if (compression == WebPCompression.VP8)
			webp1.Quality = setting;
		else
			webp1.CompressionLevel = setting;

		// Act
		var size1 = webp1.GetEstimatedFileSize();
		var size2 = webp2.GetEstimatedFileSize();

		// Assert
		Assert.True(size1 > 0);
		Assert.True(size2 > 0);
		// Sizes should be different based on settings
		if (colorMode == WebPColorMode.Rgba)
			Assert.True(size1 > size2); // RGBA should be larger than RGB
	}

	[Fact]
	public void GetEstimatedFileSize_WithMetadata_ShouldIncludeOverhead()
	{
		// Arrange
		var webp1 = new WebPRaster(800, 600);
		var webp2 = new WebPRaster(800, 600);

		// Add metadata to webp2
		webp2.Metadata.IccProfile = new byte[1000];
		webp2.Metadata.ExifData = new byte[500];
		webp2.Metadata.XmpData = new byte[300];

		// Act
		var size1 = webp1.GetEstimatedFileSize();
		var size2 = webp2.GetEstimatedFileSize();

		// Assert
		Assert.True(size2 > size1);
		Assert.True(size2 - size1 >= 1800); // At least the metadata size
	}

	[Fact]
	public void CompressionRatio_ShouldNotAllowNegativeValues()
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.CompressionRatio = -5.0;

		// Assert
		Assert.True(webp.CompressionRatio >= 1.0);
	}

	[Theory]
	[InlineData(WebPFormat.Simple, WebPCompression.VP8)]
	[InlineData(WebPFormat.Lossless, WebPCompression.VP8L)]
	[InlineData(WebPFormat.Extended, WebPCompression.VP8)]
	public void Format_ShouldUpdateCompression(WebPFormat format, WebPCompression expectedCompression)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.Format = format;

		// Assert
		if (format != WebPFormat.Extended)
			Assert.Equal(expectedCompression, webp.Compression);
	}

	[Theory]
	[InlineData(WebPCompression.VP8, WebPFormat.Simple)]
	[InlineData(WebPCompression.VP8L, WebPFormat.Lossless)]
	public void Compression_ShouldUpdateFormat(WebPCompression compression, WebPFormat expectedFormat)
	{
		// Arrange
		var webp = new WebPRaster();

		// Act
		webp.Compression = compression;

		// Assert
		Assert.Equal(expectedFormat, webp.Format);
	}

	[Fact]
	public void IsAnimated_ShouldReturnTrueWithAnimationFrames()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.HasAnimation = true;
		webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame { Width = 100, Height = 100 });

		// Act & Assert
		Assert.True(webp.IsAnimated);
	}

	[Fact]
	public void IsAnimated_ShouldReturnFalseWithoutFrames()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.HasAnimation = true;

		// Act & Assert
		Assert.False(webp.IsAnimated);
	}

	[Fact]
	public void Dispose_ShouldClearMetadata()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[100];
		webp.Metadata.ExifData = new byte[100];
		webp.Metadata.XmpData = new byte[100];
		webp.Metadata.CustomChunks.Add("TEST", new byte[50]);
		webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame());

		// Act
		webp.Dispose();

		// Assert
		Assert.True(webp.Metadata.IccProfile.IsEmpty);
		Assert.True(webp.Metadata.ExifData.IsEmpty);
		Assert.True(webp.Metadata.XmpData.IsEmpty);
		Assert.Empty(webp.Metadata.CustomChunks);
		Assert.Empty(webp.Metadata.AnimationFrames);
	}

	[Fact]
	public async Task DisposeAsync_ShouldClearMetadata()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[100];
		webp.Metadata.ExifData = new byte[100];
		webp.Metadata.XmpData = new byte[100];
		webp.Metadata.CustomChunks.Add("TEST", new byte[50]);
		webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame());

		// Act
		await webp.DisposeAsync();

		// Assert
		Assert.True(webp.Metadata.IccProfile.IsEmpty);
		Assert.True(webp.Metadata.ExifData.IsEmpty);
		Assert.True(webp.Metadata.XmpData.IsEmpty);
		Assert.Empty(webp.Metadata.CustomChunks);
		Assert.Empty(webp.Metadata.AnimationFrames);
	}

	[Fact]
	public void HasLargeMetadata_WithSmallMetadata_ShouldReturnFalse()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[1000]; // 1KB
		webp.Metadata.ExifData = new byte[500];    // 500 bytes

		// Act & Assert
		Assert.False(webp.HasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeMetadata_ShouldReturnTrue()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB

		// Act & Assert
		Assert.True(webp.HasLargeMetadata);
	}

	[Fact]
	public void EstimatedMetadataSize_ShouldCalculateCorrectly()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[1000];
		webp.Metadata.ExifData = new byte[500];
		webp.Metadata.XmpData = new byte[300];
		webp.Metadata.CustomChunks.Add("TEST1", new byte[200]);
		webp.Metadata.CustomChunks.Add("TEST2", new byte[100]);

		// Act
		var size = webp.EstimatedMetadataSize;

		// Assert
		Assert.Equal(2100, size); // 1000 + 500 + 300 + 200 + 100
	}

	[Fact]
	public void EstimatedMetadataSize_WithAnimationFrames_ShouldIncludeFrameData()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.HasAnimation = true;
		webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame { Data = new byte[500] });
		webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame { Data = new byte[300] });

		// Act
		var size = webp.EstimatedMetadataSize;

		// Assert
		Assert.Equal(800, size); // 500 + 300
	}

	[Fact]
	public async Task DisposeAsync_WithLargeMetadata_ShouldUseBatchedCleanup()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB to trigger large metadata path

		// Add many animation frames to test batched cleanup
		for (int i = 0; i < 150; i++)
		{
			webp.Metadata.AnimationFrames.Add(new WebPAnimationFrame { Data = new byte[1000] });
		}
		webp.Metadata.HasAnimation = true;

		// Verify we have large metadata
		Assert.True(webp.HasLargeMetadata);
		Assert.True(webp.EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold);

		// Act
		await webp.DisposeAsync();

		// Assert
		Assert.True(webp.Metadata.IccProfile.IsEmpty);
		Assert.Empty(webp.Metadata.AnimationFrames);
	}

	[Fact]
	public async Task DisposeAsync_WithSmallMetadata_ShouldUseRegularCleanup()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[1000]; // Small metadata
		webp.Metadata.ExifData = new byte[500];

		// Verify we have small metadata
		Assert.False(webp.HasLargeMetadata);

		// Act
		await webp.DisposeAsync();

		// Assert
		Assert.True(webp.Metadata.IccProfile.IsEmpty);
		Assert.True(webp.Metadata.ExifData.IsEmpty);
	}

	[Fact]
	public void EstimatedMetadataSize_WithEmptyMetadata_ShouldReturnZero()
	{
		// Arrange
		var webp = new WebPRaster();

		// Act & Assert
		Assert.Equal(0, webp.EstimatedMetadataSize);
		Assert.False(webp.HasLargeMetadata);
	}

	[Fact]
	public async Task DisposeAsync_CalledMultipleTimes_ShouldNotThrow()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[100];

		// Act & Assert - Should not throw
		await webp.DisposeAsync();
		await webp.DisposeAsync(); // Second call should be safe
	}

	[Fact]
	public void Dispose_CalledMultipleTimes_ShouldNotThrow()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[100];

		// Act & Assert - Should not throw
		webp.Dispose();
		webp.Dispose(); // Second call should be safe
	}
}
