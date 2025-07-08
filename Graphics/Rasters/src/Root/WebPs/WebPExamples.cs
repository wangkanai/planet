// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

/// <summary>Provides examples and common usage patterns for WebP raster images.</summary>
public static class WebPExamples
{
	/// <summary>Creates a standard lossy WebP image optimized for web use.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>A new WebP raster configured for web optimization.</returns>
	public static WebPRaster CreateWebOptimized(int width, int height, int quality = 75)
	{
		var webp = new WebPRaster(width, height, quality);
		webp.ConfigureLossy(quality);
		webp.SetColorMode(WebPColorMode.Rgb);
		webp.Preset = WebPPreset.Default;

		// Set web-optimized metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;

		return webp;
	}

	/// <summary>Creates a lossless WebP image for high-quality preservation.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="compressionLevel">The compression level (0-9).</param>
	/// <returns>A new lossless WebP raster.</returns>
	public static WebPRaster CreateLossless(int width, int height, int compressionLevel = 6)
	{
		var webp = new WebPRaster(width, height);
		webp.ConfigureLossless();
		webp.CompressionLevel = compressionLevel;
		webp.SetColorMode(WebPColorMode.Rgb);
		webp.Preset = WebPPreset.Default;

		// Set metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;

		return webp;
	}

	/// <summary>Creates a WebP image with alpha channel support.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>A new WebP raster with alpha support.</returns>
	public static WebPRaster CreateWithAlpha(int width, int height, int quality = 85)
	{
		var webp = new WebPRaster(width, height, quality);
		webp.SetColorMode(WebPColorMode.Rgba);
		webp.EnableExtendedFeatures();
		webp.Preset = WebPPreset.Picture;

		// Set metadata for alpha channel
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.HasAlpha         = true;

		return webp;
	}

	/// <summary>Creates a WebP image optimized for photographic content.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new photo-optimized WebP raster.</returns>
	public static WebPRaster CreateForPhotography(int width, int height)
	{
		var webp = new WebPRaster(width, height, 90);
		webp.Preset = WebPPreset.Photo;
		webp.SetColorMode(WebPColorMode.Rgb);

		// Photography-specific metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Description      = "High-quality photographic image";

		return webp;
	}

	/// <summary>Creates a WebP image optimized for drawings and graphics.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new drawing-optimized WebP raster.</returns>
	public static WebPRaster CreateForDrawing(int width, int height)
	{
		var webp = new WebPRaster(width, height);
		webp.Preset = WebPPreset.Drawing;
		webp.ConfigureLossless();              // Better for graphics
		webp.SetColorMode(WebPColorMode.Rgba); // Support transparency
		webp.EnableExtendedFeatures();

		// Drawing-specific metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Description      = "Vector graphics or drawing";

		return webp;
	}

	/// <summary>Creates a WebP image optimized for icon usage.</summary>
	/// <param name="size">The size of the square icon.</param>
	/// <returns>A new icon-optimized WebP raster.</returns>
	public static WebPRaster CreateIcon(int size = 256)
	{
		var webp = new WebPRaster(size, size);
		webp.Preset = WebPPreset.Icon;
		webp.SetColorMode(WebPColorMode.Rgba);
		webp.EnableExtendedFeatures();

		// Icon-specific metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Description      = $"Icon {size}x{size}";
		webp.WebPMetadata.Title            = "Application Icon";

		return webp;
	}

	/// <summary>Creates a WebP image optimized for text content.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new text-optimized WebP raster.</returns>
	public static WebPRaster CreateForText(int width, int height)
	{
		var webp = new WebPRaster(width, height);
		webp.Preset = WebPPreset.Text;
		webp.SetColorMode(WebPColorMode.Rgb);

		// Text-specific metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Description      = "Text-based image content";

		return webp;
	}

	/// <summary>Creates an animated WebP image with basic settings.</summary>
	/// <param name="width">The width of the animation canvas.</param>
	/// <param name="height">The height of the animation canvas.</param>
	/// <param name="loops">The number of animation loops (0 = infinite).</param>
	/// <returns>A new animated WebP raster.</returns>
	public static WebPRaster CreateAnimated(int width, int height, ushort loops = 0)
	{
		var webp = new WebPRaster(width, height, 75);
		webp.EnableExtendedFeatures();
		webp.SetColorMode(WebPColorMode.Rgba);

		// Configure animation
		webp.WebPMetadata.HasAnimation    = true;
		webp.WebPMetadata.AnimationLoops  = loops;
		webp.WebPMetadata.BackgroundColor = 0x00000000; // Transparent

		// Animation metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Description      = "Animated WebP image";

		return webp;
	}

	/// <summary>Creates a WebP image with comprehensive metadata.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new WebP raster with sample metadata.</returns>
	public static WebPRaster CreateWithMetadata(int width, int height)
	{
		var webp = new WebPRaster(width, height, 85);
		webp.EnableExtendedFeatures();
		webp.SetColorMode(WebPColorMode.Rgb);
		webp.Preset = WebPPreset.Photo;

		// Comprehensive metadata
		webp.WebPMetadata.Software         = "Wangkanai.Graphics.Rasters";
		webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;
		webp.WebPMetadata.Artist           = "Sample Artist";
		webp.WebPMetadata.Copyright        = "Copyright 2025";
		webp.WebPMetadata.Title            = "Sample WebP Image";
		webp.WebPMetadata.Description      = "Sample image with comprehensive metadata";

		// Add flags for features
		webp.WebPMetadata.HasExif       = true;
		webp.WebPMetadata.HasXmp        = true;
		webp.WebPMetadata.HasIccProfile = true;

		return webp;
	}

	/// <summary>Demonstrates quality settings and their use cases.</summary>
	/// <returns>A dictionary of quality settings with descriptions.</returns>
	public static Dictionary<int, string> GetQualityRecommendations()
	{
		return new Dictionary<int, string>
		       {
			       { 100, "Near-lossless quality - excellent for archival, larger file size" },
			       { 90, "Excellent quality - suitable for professional photography" },
			       { 85, "High quality - good for detailed images and print" },
			       { 75, "Good quality - default web quality, balanced size/quality" },
			       { 60, "Medium quality - suitable for thumbnails and previews" },
			       { 40, "Low quality - high compression, noticeable artifacts" },
			       { 20, "Very low quality - maximum compression, significant artifacts" },
			       { 0, "Minimum quality - smallest size, poor visual quality" }
		       };
	}

	/// <summary>Demonstrates preset options and their effects.</summary>
	/// <returns>A dictionary of presets with descriptions.</returns>
	public static Dictionary<WebPPreset, string> GetPresetGuide()
	{
		return new Dictionary<WebPPreset, string>
		       {
			       { WebPPreset.Default, "Balanced settings for general use" },
			       { WebPPreset.Picture, "Optimized for natural images with good detail preservation" },
			       { WebPPreset.Photo, "Best for photographic content with fine details" },
			       { WebPPreset.Drawing, "Ideal for line art, drawings, and graphics" },
			       { WebPPreset.Icon, "Perfect for icons and simple graphics with transparency" },
			       { WebPPreset.Text, "Optimized for text content and screenshots" }
		       };
	}

	/// <summary>Demonstrates compression type benefits.</summary>
	/// <returns>A dictionary of compression types with descriptions.</returns>
	public static Dictionary<WebPCompression, string> GetCompressionGuide()
	{
		return new Dictionary<WebPCompression, string>
		       {
			       { WebPCompression.VP8, "Lossy compression - smaller files, some quality loss" },
			       { WebPCompression.VP8L, "Lossless compression - perfect quality, larger files" }
		       };
	}

	/// <summary>Creates a performance-optimized WebP for large images.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A performance-optimized WebP raster.</returns>
	public static WebPRaster CreatePerformanceOptimized(int width, int height)
	{
		var webp = new WebPRaster(width, height);

		// Choose a format based on image size for optimal performance
		if (width * height > 4_000_000) // 4 megapixels
		{
			webp.ConfigureLossy(70);   // Lower quality for very large images
			webp.CompressionLevel = 3; // Faster encoding
		}
		else
		{
			webp.ConfigureLossy(80);
			webp.CompressionLevel = 6;
		}

		webp.Preset = WebPPreset.Default;
		webp.SetColorMode(WebPColorMode.Rgb);

		// Minimal metadata for performance
		webp.WebPMetadata.Software = "Wangkanai.Graphics.Rasters";

		return webp;
	}
}
