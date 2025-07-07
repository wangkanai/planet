// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

public class WebPExamplesTests
{
	[Theory]
	[InlineData(800, 600, 75)]
	[InlineData(1920, 1080, 85)]
	[InlineData(400, 300, 60)]
	public void CreateWebOptimized_ShouldCreateValidWebP(int width, int height, int quality)
	{
		// Act
		var webp = WebPExamples.CreateWebOptimized(width, height, quality);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(quality, webp.Quality);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.Equal(WebPPreset.Default, webp.Preset);
		Assert.False(webp.IsLossless);
		Assert.Equal("Wangkanai.Graphics.Rasters", webp.Metadata.Software);
		Assert.NotNull(webp.Metadata.CreationDateTime);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(800, 600, 6)]
	[InlineData(1024, 768, 9)]
	[InlineData(512, 512, 3)]
	public void CreateLossless_ShouldCreateLosslessWebP(int width, int height, int compressionLevel)
	{
		// Act
		var webp = WebPExamples.CreateLossless(width, height, compressionLevel);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(compressionLevel, webp.CompressionLevel);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.True(webp.IsLossless);
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.Equal(WebPCompression.VP8L, webp.Compression);
		Assert.Equal("Wangkanai.Graphics.Rasters", webp.Metadata.Software);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(800, 600, 85)]
	[InlineData(1200, 800, 90)]
	public void CreateWithAlpha_ShouldCreateWebPWithAlpha(int width, int height, int quality)
	{
		// Act
		var webp = WebPExamples.CreateWithAlpha(width, height, quality);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(quality, webp.Quality);
		Assert.Equal(WebPColorMode.Rgba, webp.ColorMode);
		Assert.True(webp.HasAlpha);
		Assert.Equal(4, webp.Channels);
		Assert.Equal(WebPFormat.Extended, webp.Format);
		Assert.Equal(WebPPreset.Picture, webp.Preset);
		Assert.True(webp.Metadata.HasAlpha);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(1920, 1080)]
	[InlineData(3840, 2160)]
	public void CreateForPhotography_ShouldCreatePhotoOptimizedWebP(int width, int height)
	{
		// Act
		var webp = WebPExamples.CreateForPhotography(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(90, webp.Quality);
		Assert.Equal(WebPPreset.Photo, webp.Preset);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.Equal("High-quality photographic image", webp.Metadata.Description);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1024, 768)]
	public void CreateForDrawing_ShouldCreateDrawingOptimizedWebP(int width, int height)
	{
		// Act
		var webp = WebPExamples.CreateForDrawing(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(WebPPreset.Drawing, webp.Preset);
		Assert.True(webp.IsLossless);
		Assert.Equal(WebPColorMode.Rgba, webp.ColorMode);
		// Note: ConfigureLossless() in the Drawing preset overrides EnableExtendedFeatures()
		// Auto-synchronization sets format to Lossless when VP8L compression is used
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.Equal("Vector graphics or drawing", webp.Metadata.Description);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(16)]
	[InlineData(32)]
	[InlineData(64)]
	[InlineData(128)]
	[InlineData(256)]
	[InlineData(512)]
	public void CreateIcon_ShouldCreateIconOptimizedWebP(int size)
	{
		// Act
		var webp = WebPExamples.CreateIcon(size);

		// Assert
		Assert.Equal(size, webp.Width);
		Assert.Equal(size, webp.Height);
		Assert.Equal(WebPPreset.Icon, webp.Preset);
		Assert.True(webp.IsLossless);
		Assert.Equal(WebPColorMode.Rgba, webp.ColorMode);
		// Note: ConfigureLossless() in the Icon preset overrides EnableExtendedFeatures()
		// Auto-synchronization sets format to Lossless when VP8L compression is used
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.Equal($"Icon {size}x{size}", webp.Metadata.Description);
		Assert.Equal("Application Icon", webp.Metadata.Title);
		Assert.True(webp.IsValid());
	}

	[Fact]
	public void CreateIcon_WithoutSize_ShouldUseDefault256()
	{
		// Act
		var webp = WebPExamples.CreateIcon();

		// Assert
		Assert.Equal(256, webp.Width);
		Assert.Equal(256, webp.Height);
	}

	[Theory]
	[InlineData(800, 600)]
	[InlineData(1200, 400)]
	public void CreateForText_ShouldCreateTextOptimizedWebP(int width, int height)
	{
		// Act
		var webp = WebPExamples.CreateForText(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(WebPPreset.Text, webp.Preset);
		Assert.True(webp.IsLossless);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.Equal("Text-based image content", webp.Metadata.Description);
		Assert.True(webp.IsValid());
	}

	[Theory]
	[InlineData(800, 600, 0)]   // Infinite loops
	[InlineData(1024, 768, 5)]  // 5 loops
	[InlineData(640, 480, 1)]   // Single loop
	public void CreateAnimated_ShouldCreateAnimatedWebP(int width, int height, ushort loops)
	{
		// Act
		var webp = WebPExamples.CreateAnimated(width, height, loops);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(WebPFormat.Extended, webp.Format);
		Assert.Equal(WebPColorMode.Rgba, webp.ColorMode);
		Assert.True(webp.Metadata.HasAnimation);
		Assert.Equal(loops, webp.Metadata.AnimationLoops);
		Assert.Equal(0x00000000u, webp.Metadata.BackgroundColor);
		Assert.Equal("Animated WebP image", webp.Metadata.Description);
		Assert.True(webp.IsValid());
	}

	[Fact]
	public void CreateAnimated_WithoutLoops_ShouldUseInfinite()
	{
		// Act
		var webp = WebPExamples.CreateAnimated(800, 600);

		// Assert
		Assert.Equal(0, webp.Metadata.AnimationLoops);
	}

	[Theory]
	[InlineData(1920, 1080)]
	[InlineData(800, 600)]
	public void CreateWithMetadata_ShouldCreateWebPWithComprehensiveMetadata(int width, int height)
	{
		// Act
		var webp = WebPExamples.CreateWithMetadata(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(WebPFormat.Extended, webp.Format);
		Assert.Equal(85, webp.Quality);
		Assert.Equal(WebPPreset.Photo, webp.Preset);

		// Check metadata
		Assert.Equal("Wangkanai.Graphics.Rasters", webp.Metadata.Software);
		Assert.Equal("Sample Artist", webp.Metadata.Artist);
		Assert.Equal("Copyright 2025", webp.Metadata.Copyright);
		Assert.Equal("Sample WebP Image", webp.Metadata.Title);
		Assert.Equal("Sample image with comprehensive metadata", webp.Metadata.Description);
		Assert.True(webp.Metadata.HasExif);
		Assert.True(webp.Metadata.HasXmp);
		Assert.True(webp.Metadata.HasIccProfile);
		Assert.NotNull(webp.Metadata.CreationDateTime);
		Assert.True(webp.IsValid());
	}

	[Fact]
	public void GetQualityRecommendations_ShouldReturnValidDictionary()
	{
		// Act
		var recommendations = WebPExamples.GetQualityRecommendations();

		// Assert
		Assert.NotEmpty(recommendations);
		Assert.True(recommendations.ContainsKey(0));
		Assert.True(recommendations.ContainsKey(75));
		Assert.True(recommendations.ContainsKey(100));
		Assert.Contains("web quality", recommendations[75]);
		Assert.Contains("archival", recommendations[100]);
		Assert.Contains("smallest size", recommendations[0]);
	}

	[Fact]
	public void GetPresetGuide_ShouldReturnValidDictionary()
	{
		// Act
		var guide = WebPExamples.GetPresetGuide();

		// Assert
		Assert.NotEmpty(guide);
		Assert.True(guide.ContainsKey(WebPPreset.Default));
		Assert.True(guide.ContainsKey(WebPPreset.Photo));
		Assert.True(guide.ContainsKey(WebPPreset.Drawing));
		Assert.True(guide.ContainsKey(WebPPreset.Icon));
		Assert.True(guide.ContainsKey(WebPPreset.Text));
		Assert.Contains("general use", guide[WebPPreset.Default]);
		Assert.Contains("photographic", guide[WebPPreset.Photo]);
	}

	[Fact]
	public void GetCompressionGuide_ShouldReturnValidDictionary()
	{
		// Act
		var guide = WebPExamples.GetCompressionGuide();

		// Assert
		Assert.NotEmpty(guide);
		Assert.True(guide.ContainsKey(WebPCompression.VP8));
		Assert.True(guide.ContainsKey(WebPCompression.VP8L));
		Assert.Contains("Lossy", guide[WebPCompression.VP8]);
		Assert.Contains("Lossless", guide[WebPCompression.VP8L]);
	}

	[Theory]
	[InlineData(1920, 1080)] // Small image
	[InlineData(4000, 3000)] // Medium image
	[InlineData(8000, 6000)] // Large image
	public void CreatePerformanceOptimized_ShouldOptimizeBasedOnSize(int width, int height)
	{
		// Act
		var webp = WebPExamples.CreatePerformanceOptimized(width, height);

		// Assert
		Assert.Equal(width, webp.Width);
		Assert.Equal(height, webp.Height);
		Assert.Equal(WebPColorMode.Rgb, webp.ColorMode);
		Assert.Equal(WebPPreset.Default, webp.Preset);
		Assert.False(webp.IsLossless);
		Assert.Equal("Wangkanai.Graphics.Rasters", webp.Metadata.Software);

		var pixelCount = width * height;
		if (pixelCount > 4_000_000)
		{
			// Large images should use lower quality and faster compression
			Assert.Equal(70, webp.Quality);
			Assert.Equal(3, webp.CompressionLevel);
		}
		else
		{
			// Smaller images can use higher quality
			Assert.Equal(80, webp.Quality);
			Assert.Equal(6, webp.CompressionLevel);
		}

		Assert.True(webp.IsValid());
	}

	[Fact]
	public void AllExampleMethods_ShouldCreateValidWebP()
	{
		// Arrange & Act
		var examples = new[]
		{
			WebPExamples.CreateWebOptimized(800, 600),
			WebPExamples.CreateLossless(800, 600),
			WebPExamples.CreateWithAlpha(800, 600),
			WebPExamples.CreateForPhotography(800, 600),
			WebPExamples.CreateForDrawing(800, 600),
			WebPExamples.CreateIcon(),
			WebPExamples.CreateForText(800, 600),
			WebPExamples.CreateAnimated(800, 600),
			WebPExamples.CreateWithMetadata(800, 600),
			WebPExamples.CreatePerformanceOptimized(800, 600)
		};

		// Assert
		foreach (var webp in examples)
		{
			Assert.True(webp.IsValid(), $"WebP example is not valid: {webp.GetType().Name}");
			Assert.True(webp.Width > 0);
			Assert.True(webp.Height > 0);
			Assert.NotNull(webp.Metadata);
			Assert.NotNull(webp.Metadata.Software);
		}
	}
}
