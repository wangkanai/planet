// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Pngs;

/// <summary>Provides examples and usage patterns for PNG raster images.</summary>
public static class PngExamples
{
	/// <summary>Creates a basic truecolor PNG raster.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured PNG raster instance.</returns>
	public static PngRaster CreateBasicTruecolor(int width, int height)
	{
		return new PngRaster(width, height)
		       {
			       ColorType        = PngColorType.Truecolor,
			       BitDepth         = 8,
			       CompressionLevel = 6,
			       InterlaceMethod  = PngInterlaceMethod.None
		       };
	}

	/// <summary>Creates a truecolor PNG with alpha channel.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured PNG raster instance with alpha support.</returns>
	public static PngRaster CreateTruecolorWithAlpha(int width, int height)
	{
		return new PngRaster(width, height)
		       {
			       ColorType        = PngColorType.TruecolorWithAlpha,
			       BitDepth         = 8,
			       HasAlphaChannel  = true,
			       CompressionLevel = 6,
			       InterlaceMethod  = PngInterlaceMethod.None
		       };
	}

	/// <summary>Creates a grayscale PNG raster.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="bitDepth">The bit depth (1, 2, 4, 8, or 16).</param>
	/// <returns>A configured grayscale PNG raster instance.</returns>
	public static PngRaster CreateGrayscale(int width, int height, byte bitDepth = 8)
	{
		return new PngRaster(width, height)
		       {
			       ColorType        = PngColorType.Grayscale,
			       BitDepth         = bitDepth,
			       CompressionLevel = 6,
			       InterlaceMethod  = PngInterlaceMethod.None
		       };
	}

	/// <summary>Creates an indexed-color PNG raster.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="bitDepth">The bit depth (1, 2, 4, or 8).</param>
	/// <returns>A configured indexed-color PNG raster instance.</returns>
	public static PngRaster CreateIndexedColor(int width, int height, byte bitDepth = 8)
	{
		var png = new PngRaster(width, height)
		          {
			          ColorType        = PngColorType.IndexedColor,
			          BitDepth         = bitDepth,
			          UsesPalette      = true,
			          CompressionLevel = 6,
			          InterlaceMethod  = PngInterlaceMethod.None
		          };

		// Create a basic palette (RGB triplets)
		var paletteSize  = Math.Min(256, 1 << bitDepth);
		var paletteBytes = new byte[paletteSize * 3];

		// Fill with a simple gradient palette
		for (var i = 0; i < paletteSize; i++)
		{
			var intensity = (byte)(i * 255 / (paletteSize - 1));
			paletteBytes[i * 3]     = intensity; // Red
			paletteBytes[i * 3 + 1] = intensity; // Green
			paletteBytes[i * 3 + 2] = intensity; // Blue
		}

		png.PaletteData = new ReadOnlyMemory<byte>(paletteBytes);

		return png;
	}

	/// <summary>Creates a high-quality PNG with maximum compression.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A configured PNG raster instance optimized for file size.</returns>
	public static PngRaster CreateHighQuality(int width, int height)
	{
		var png = new PngRaster(width, height)
		          {
			          ColorType        = PngColorType.TruecolorWithAlpha,
			          BitDepth         = 16, // High bit depth for quality
			          HasAlphaChannel  = true,
			          CompressionLevel = 9, // Maximum compression
			          InterlaceMethod  = PngInterlaceMethod.None
		          };

		// Set metadata for high-quality images
		png.PngMetadata.Software = "Wangkanai Graphics Rasters";
		png.PngMetadata.Created  = DateTime.UtcNow;

		return png;
	}

	/// <summary>Creates a PNG optimized for web usage.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="useAlpha">Whether to include alpha channel for transparency.</param>
	/// <returns>A configured PNG raster instance optimized for web.</returns>
	public static PngRaster CreateWebOptimized(int width, int height, bool useAlpha = true)
	{
		var png = new PngRaster(width, height)
		          {
			          ColorType        = useAlpha ? PngColorType.TruecolorWithAlpha : PngColorType.Truecolor,
			          BitDepth         = 8, // Standard web bit depth
			          HasAlphaChannel  = useAlpha,
			          CompressionLevel = 6,                       // Balanced compression for web
			          InterlaceMethod  = PngInterlaceMethod.Adam7 // Progressive loading
		          };

		// Set web-appropriate metadata
		png.PngMetadata.Software            = "Wangkanai Graphics Rasters";
		png.PngMetadata.SrgbRenderingIntent = 0; // Perceptual rendering

		return png;
	}
}
