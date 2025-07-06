// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Provides examples and common usage patterns for JPEG raster images.</summary>
public static class JpegExamples
{
	/// <summary>Creates a standard RGB JPEG image.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>A new JPEG raster configured for RGB color mode.</returns>
	public static JpegRaster CreateRgbJpeg(int width, int height, int quality = 85)
	{
		var jpeg = new JpegRaster(width, height, quality);
		jpeg.SetColorMode(JpegColorMode.Rgb);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.Both; // 4:2:0 for better compression
		jpeg.Encoding          = JpegEncoding.Baseline;
		jpeg.IsOptimized       = true;

		// Set common metadata
		jpeg.Metadata.Software        = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime = DateTime.Now;
		jpeg.Metadata.ResolutionUnit  = 2; // Inches
		jpeg.Metadata.XResolution     = 72.0;
		jpeg.Metadata.YResolution     = 72.0;

		return jpeg;
	}

	/// <summary>Creates a grayscale JPEG image.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>A new JPEG raster configured for grayscale.</returns>
	public static JpegRaster CreateGrayscaleJpeg(int width, int height, int quality = 90)
	{
		var jpeg = new JpegRaster(width, height, quality);
		jpeg.SetColorMode(JpegColorMode.Grayscale);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.None; // No chroma subsampling for grayscale
		jpeg.Encoding          = JpegEncoding.Baseline;

		// Set metadata
		jpeg.Metadata.Software        = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime = DateTime.Now;
		jpeg.Metadata.ResolutionUnit  = 2;     // Inches
		jpeg.Metadata.XResolution     = 300.0; // High resolution for grayscale
		jpeg.Metadata.YResolution     = 300.0;

		return jpeg;
	}

	/// <summary>Creates a progressive JPEG image for web use.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <param name="quality">The quality level (0-100).</param>
	/// <returns>A new progressive JPEG raster.</returns>
	public static JpegRaster CreateProgressiveJpeg(int width, int height, int quality = 75)
	{
		var jpeg = new JpegRaster(width, height, quality);
		jpeg.SetColorMode(JpegColorMode.Rgb);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.Both; // 4:2:0 for web optimization
		jpeg.Encoding          = JpegEncoding.Progressive;
		jpeg.IsProgressive     = true;
		jpeg.IsOptimized       = true;

		// Web-optimized metadata
		jpeg.Metadata.Software        = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime = DateTime.Now;
		jpeg.Metadata.ResolutionUnit  = 2;    // Inches
		jpeg.Metadata.XResolution     = 72.0; // Web standard
		jpeg.Metadata.YResolution     = 72.0;

		return jpeg;
	}

	/// <summary>Creates a high-quality JPEG for photography.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new high-quality JPEG raster.</returns>
	public static JpegRaster CreatePhotographyJpeg(int width, int height)
	{
		var jpeg = new JpegRaster(width, height, 95); // High quality
		jpeg.SetColorMode(JpegColorMode.Rgb);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.None; // 4:4:4 for maximum quality
		jpeg.Encoding          = JpegEncoding.Baseline;
		jpeg.IsOptimized       = true;

		// Photography metadata
		jpeg.Metadata.Software        = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime = DateTime.Now;
		jpeg.Metadata.ResolutionUnit  = 2;     // Inches
		jpeg.Metadata.XResolution     = 300.0; // Print quality
		jpeg.Metadata.YResolution     = 300.0;
		jpeg.Metadata.Artist          = "Photographer";
		jpeg.Metadata.ColorSpace      = 1; // sRGB

		return jpeg;
	}

	/// <summary>Creates a CMYK JPEG for print production.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new CMYK JPEG raster.</returns>
	public static JpegRaster CreateCmykJpeg(int width, int height)
	{
		var jpeg = new JpegRaster(width, height, 90); // High quality for print
		jpeg.SetColorMode(JpegColorMode.Cmyk);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.None; // No subsampling for CMYK
		jpeg.Encoding          = JpegEncoding.Baseline;
		jpeg.IsOptimized       = true;

		// Print metadata
		jpeg.Metadata.Software        = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime = DateTime.Now;
		jpeg.Metadata.ResolutionUnit  = 2;     // Inches
		jpeg.Metadata.XResolution     = 300.0; // Print resolution
		jpeg.Metadata.YResolution     = 300.0;
		jpeg.Metadata.ColorSpace      = 65535; // Uncalibrated/CMYK

		return jpeg;
	}

	/// <summary>Creates a thumbnail JPEG image.</summary>
	/// <param name="width">The width of the thumbnail.</param>
	/// <param name="height">The height of the thumbnail.</param>
	/// <returns>A new thumbnail JPEG raster.</returns>
	public static JpegRaster CreateThumbnailJpeg(int width = 150, int height = 150)
	{
		var jpeg = new JpegRaster(width, height, 60); // Lower quality for small size
		jpeg.SetColorMode(JpegColorMode.Rgb);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.Both; // Maximum compression
		jpeg.Encoding          = JpegEncoding.Baseline;
		jpeg.IsOptimized       = true;

		// Thumbnail metadata
		jpeg.Metadata.Software         = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime  = DateTime.Now;
		jpeg.Metadata.ResolutionUnit   = 2;    // Inches
		jpeg.Metadata.XResolution      = 72.0; // Screen resolution
		jpeg.Metadata.YResolution      = 72.0;
		jpeg.Metadata.ImageDescription = "Thumbnail";

		return jpeg;
	}

	/// <summary>Creates a JPEG with comprehensive EXIF metadata.</summary>
	/// <param name="width">The width of the image.</param>
	/// <param name="height">The height of the image.</param>
	/// <returns>A new JPEG raster with sample EXIF data.</returns>
	public static JpegRaster CreateJpegWithExifData(int width, int height)
	{
		var jpeg = new JpegRaster(width, height, 85);
		jpeg.SetColorMode(JpegColorMode.Rgb);
		jpeg.ChromaSubsampling = JpegChromaSubsampling.Both;
		jpeg.Encoding          = JpegEncoding.Baseline;

		// Comprehensive EXIF metadata
		jpeg.Metadata.Make             = "Canon";
		jpeg.Metadata.Model            = "EOS R5";
		jpeg.Metadata.Software         = "Wangkanai.Graphics.Rasters";
		jpeg.Metadata.CaptureDateTime  = DateTime.Now;
		jpeg.Metadata.Artist           = "John Doe";
		jpeg.Metadata.Copyright        = "Copyright 2025";
		jpeg.Metadata.ImageDescription = "Sample image with EXIF data";

		// Camera settings
		jpeg.Metadata.ExposureTime   = 1.0 / 125.0; // 1/125 second
		jpeg.Metadata.FNumber        = 5.6;
		jpeg.Metadata.IsoSpeedRating = 400;
		jpeg.Metadata.FocalLength    = 85.0; // 85mm
		jpeg.Metadata.WhiteBalance   = 0;    // Auto

		// GPS coordinates (example: Tokyo, Japan)
		jpeg.Metadata.GpsLatitude  = 35.6762;
		jpeg.Metadata.GpsLongitude = 139.6503;

		// Resolution
		jpeg.Metadata.ResolutionUnit = 2; // Inches
		jpeg.Metadata.XResolution    = 300.0;
		jpeg.Metadata.YResolution    = 300.0;
		jpeg.Metadata.ColorSpace     = 1; // sRGB
		jpeg.Metadata.Orientation    = 1; // Normal

		return jpeg;
	}

	/// <summary>Demonstrates common quality settings and their use cases.</summary>
	/// <returns>A dictionary of quality settings with descriptions.</returns>
	public static Dictionary<int, string> GetQualityRecommendations()
		=> new()
		   {
			   { 100, "Lossless quality - largest file size, perfect for archival" },
			   { 95, "Excellent quality - minimal compression, suitable for professional photography" },
			   { 85, "High quality - good balance of quality and file size, suitable for print" },
			   { 75, "Good quality - standard web quality, good for most web applications" },
			   { 60, "Medium quality - noticeable compression, suitable for thumbnails" },
			   { 40, "Low quality - high compression, suitable for previews or low-bandwidth" },
			   { 20, "Very low quality - maximum compression, suitable for placeholders" }
		   };

	/// <summary>Demonstrates chroma subsampling options and their effects.</summary>
	/// <returns>A dictionary of chroma subsampling modes with descriptions.</returns>
	public static Dictionary<JpegChromaSubsampling, string> GetChromaSubsamplingGuide()
		=> new()
		   {
			   { JpegChromaSubsampling.None, "4:4:4 - No subsampling, highest quality, larger file size" },
			   { JpegChromaSubsampling.Horizontal, "4:2:2 - Horizontal subsampling, good quality, moderate compression" },
			   { JpegChromaSubsampling.Both, "4:2:0 - Both horizontal and vertical subsampling, standard compression" }
		   };
}
