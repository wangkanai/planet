// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Jpegs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Jpegs;

public class JpegExamplesTests
{
	[Fact]
	public void CreateRgbJpeg_ShouldCreateValidRgbJpeg()
	{
		// Arrange
		const int width = 1920;
		const int height = 1080;
		const int quality = 85;

		// Act
		var jpeg = JpegExamples.CreateRgbJpeg(width, height, quality);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(quality, jpeg.Quality);
		Assert.Equal(JpegColorMode.Rgb, jpeg.ColorMode);
		Assert.Equal(3, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.Both, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);
		Assert.True(jpeg.IsOptimized);
		Assert.False(jpeg.IsProgressive);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(72.0, jpeg.Metadata.XResolution);
		Assert.Equal(72.0, jpeg.Metadata.YResolution);
		Assert.NotNull(jpeg.Metadata.DateTime);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateRgbJpeg_WithDefaultQuality_ShouldUse85Quality()
	{
		// Act
		var jpeg = JpegExamples.CreateRgbJpeg(800, 600);

		// Assert
		Assert.Equal(85, jpeg.Quality);
	}

	[Fact]
	public void CreateGrayscaleJpeg_ShouldCreateValidGrayscaleJpeg()
	{
		// Arrange
		const int width = 1024;
		const int height = 768;
		const int quality = 90;

		// Act
		var jpeg = JpegExamples.CreateGrayscaleJpeg(width, height, quality);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(quality, jpeg.Quality);
		Assert.Equal(JpegColorMode.Grayscale, jpeg.ColorMode);
		Assert.Equal(1, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.None, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);
		Assert.False(jpeg.IsOptimized);
		Assert.False(jpeg.IsProgressive);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(300.0, jpeg.Metadata.XResolution);
		Assert.Equal(300.0, jpeg.Metadata.YResolution);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateGrayscaleJpeg_WithDefaultQuality_ShouldUse90Quality()
	{
		// Act
		var jpeg = JpegExamples.CreateGrayscaleJpeg(800, 600);

		// Assert
		Assert.Equal(90, jpeg.Quality);
	}

	[Fact]
	public void CreateProgressiveJpeg_ShouldCreateValidProgressiveJpeg()
	{
		// Arrange
		const int width = 800;
		const int height = 600;
		const int quality = 75;

		// Act
		var jpeg = JpegExamples.CreateProgressiveJpeg(width, height, quality);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(quality, jpeg.Quality);
		Assert.Equal(JpegColorMode.Rgb, jpeg.ColorMode);
		Assert.Equal(3, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.Both, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Progressive, jpeg.Encoding);
		Assert.True(jpeg.IsProgressive);
		Assert.True(jpeg.IsOptimized);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(72.0, jpeg.Metadata.XResolution);
		Assert.Equal(72.0, jpeg.Metadata.YResolution);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateProgressiveJpeg_WithDefaultQuality_ShouldUse75Quality()
	{
		// Act
		var jpeg = JpegExamples.CreateProgressiveJpeg(800, 600);

		// Assert
		Assert.Equal(75, jpeg.Quality);
	}

	[Fact]
	public void CreatePhotographyJpeg_ShouldCreateHighQualityJpeg()
	{
		// Arrange
		const int width = 6000;
		const int height = 4000;

		// Act
		var jpeg = JpegExamples.CreatePhotographyJpeg(width, height);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(95, jpeg.Quality);
		Assert.Equal(JpegColorMode.Rgb, jpeg.ColorMode);
		Assert.Equal(3, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.None, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);
		Assert.True(jpeg.IsOptimized);
		Assert.False(jpeg.IsProgressive);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(300.0, jpeg.Metadata.XResolution);
		Assert.Equal(300.0, jpeg.Metadata.YResolution);
		Assert.Equal("Photographer", jpeg.Metadata.Artist);
		Assert.Equal(1, jpeg.Metadata.ColorSpace);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateCmykJpeg_ShouldCreateValidCmykJpeg()
	{
		// Arrange
		const int width = 2480;
		const int height = 3508; // A4 at 300 DPI

		// Act
		var jpeg = JpegExamples.CreateCmykJpeg(width, height);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(90, jpeg.Quality);
		Assert.Equal(JpegColorMode.Cmyk, jpeg.ColorMode);
		Assert.Equal(4, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.None, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);
		Assert.True(jpeg.IsOptimized);
		Assert.False(jpeg.IsProgressive);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(300.0, jpeg.Metadata.XResolution);
		Assert.Equal(300.0, jpeg.Metadata.YResolution);
		Assert.Equal(65535, jpeg.Metadata.ColorSpace);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateThumbnailJpeg_ShouldCreateSmallOptimizedJpeg()
	{
		// Act
		var jpeg = JpegExamples.CreateThumbnailJpeg();

		// Assert
		Assert.Equal(150, jpeg.Width);
		Assert.Equal(150, jpeg.Height);
		Assert.Equal(60, jpeg.Quality);
		Assert.Equal(JpegColorMode.Rgb, jpeg.ColorMode);
		Assert.Equal(3, jpeg.SamplesPerPixel);
		Assert.Equal(JpegChromaSubsampling.Both, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);
		Assert.True(jpeg.IsOptimized);
		Assert.False(jpeg.IsProgressive);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(72.0, jpeg.Metadata.XResolution);
		Assert.Equal(72.0, jpeg.Metadata.YResolution);
		Assert.Equal("Thumbnail", jpeg.Metadata.ImageDescription);
		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void CreateThumbnailJpeg_WithCustomDimensions_ShouldUseProvidedDimensions()
	{
		// Arrange
		const int width = 100;
		const int height = 100;

		// Act
		var jpeg = JpegExamples.CreateThumbnailJpeg(width, height);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
	}

	[Fact]
	public void CreateJpegWithExifData_ShouldCreateJpegWithComprehensiveMetadata()
	{
		// Arrange
		const int width = 4000;
		const int height = 3000;

		// Act
		var jpeg = JpegExamples.CreateJpegWithExifData(width, height);

		// Assert
		Assert.Equal(width, jpeg.Width);
		Assert.Equal(height, jpeg.Height);
		Assert.Equal(85, jpeg.Quality);
		Assert.Equal(JpegColorMode.Rgb, jpeg.ColorMode);
		Assert.Equal(JpegChromaSubsampling.Both, jpeg.ChromaSubsampling);
		Assert.Equal(JpegEncoding.Baseline, jpeg.Encoding);

		// Assert metadata
		Assert.Equal("Canon", jpeg.Metadata.Make);
		Assert.Equal("EOS R5", jpeg.Metadata.Model);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpeg.Metadata.Software);
		Assert.Equal("John Doe", jpeg.Metadata.Artist);
		Assert.Equal("Copyright 2025", jpeg.Metadata.Copyright);
		Assert.Equal("Sample image with EXIF data", jpeg.Metadata.ImageDescription);

		// Assert camera settings
		Assert.Equal(1.0 / 125.0, jpeg.Metadata.ExposureTime);
		Assert.Equal(5.6, jpeg.Metadata.FNumber);
		Assert.Equal(400, jpeg.Metadata.IsoSpeedRating);
		Assert.Equal(85.0, jpeg.Metadata.FocalLength);
		Assert.Equal(0, jpeg.Metadata.WhiteBalance);

		// Assert GPS data
		Assert.Equal(35.6762, jpeg.Metadata.GpsLatitude);
		Assert.Equal(139.6503, jpeg.Metadata.GpsLongitude);

		// Assert resolution and color space
		Assert.Equal(2, jpeg.Metadata.ResolutionUnit);
		Assert.Equal(300.0, jpeg.Metadata.XResolution);
		Assert.Equal(300.0, jpeg.Metadata.YResolution);
		Assert.Equal(1, jpeg.Metadata.ColorSpace);
		Assert.Equal(1, jpeg.Metadata.Orientation);

		Assert.True(jpeg.IsValid());
	}

	[Fact]
	public void GetQualityRecommendations_ShouldReturnValidRecommendations()
	{
		// Act
		var recommendations = JpegExamples.GetQualityRecommendations();

		// Assert
		Assert.Equal(7, recommendations.Count);
		Assert.Contains(100, recommendations.Keys);
		Assert.Contains(95, recommendations.Keys);
		Assert.Contains(85, recommendations.Keys);
		Assert.Contains(75, recommendations.Keys);
		Assert.Contains(60, recommendations.Keys);
		Assert.Contains(40, recommendations.Keys);
		Assert.Contains(20, recommendations.Keys);

		// Assert descriptions are meaningful
		Assert.Contains("Lossless", recommendations[100]);
		Assert.Contains("Excellent", recommendations[95]);
		Assert.Contains("High", recommendations[85]);
		Assert.Contains("Good", recommendations[75]);
		Assert.Contains("Medium", recommendations[60]);
		Assert.Contains("Low", recommendations[40]);
		Assert.Contains("Very low", recommendations[20]);
	}

	[Fact]
	public void GetChromaSubsamplingGuide_ShouldReturnValidGuide()
	{
		// Act
		var guide = JpegExamples.GetChromaSubsamplingGuide();

		// Assert
		Assert.Equal(3, guide.Count);
		Assert.Contains(JpegChromaSubsampling.None, guide.Keys);
		Assert.Contains(JpegChromaSubsampling.Horizontal, guide.Keys);
		Assert.Contains(JpegChromaSubsampling.Both, guide.Keys);

		// Assert descriptions contain expected information
		Assert.Contains("4:4:4", guide[JpegChromaSubsampling.None]);
		Assert.Contains("4:2:2", guide[JpegChromaSubsampling.Horizontal]);
		Assert.Contains("4:2:0", guide[JpegChromaSubsampling.Both]);
	}

	[Theory]
	[InlineData(800, 600, 85)]
	[InlineData(1920, 1080, 75)]
	[InlineData(3840, 2160, 90)]
	public void AllExampleMethods_ShouldCreateValidJpegs(int width, int height, int quality)
	{
		// Act
		var rgbJpeg = JpegExamples.CreateRgbJpeg(width, height, quality);
		var grayscaleJpeg = JpegExamples.CreateGrayscaleJpeg(width, height, quality);
		var progressiveJpeg = JpegExamples.CreateProgressiveJpeg(width, height, quality);
		var photographyJpeg = JpegExamples.CreatePhotographyJpeg(width, height);
		var cmykJpeg = JpegExamples.CreateCmykJpeg(width, height);
		var jpegWithExif = JpegExamples.CreateJpegWithExifData(width, height);

		// Assert
		Assert.True(rgbJpeg.IsValid());
		Assert.True(grayscaleJpeg.IsValid());
		Assert.True(progressiveJpeg.IsValid());
		Assert.True(photographyJpeg.IsValid());
		Assert.True(cmykJpeg.IsValid());
		Assert.True(jpegWithExif.IsValid());
	}

	[Fact]
	public void AllExampleMethods_ShouldHaveNonNullMetadata()
	{
		// Act
		var rgbJpeg = JpegExamples.CreateRgbJpeg(800, 600);
		var grayscaleJpeg = JpegExamples.CreateGrayscaleJpeg(800, 600);
		var progressiveJpeg = JpegExamples.CreateProgressiveJpeg(800, 600);
		var photographyJpeg = JpegExamples.CreatePhotographyJpeg(800, 600);
		var cmykJpeg = JpegExamples.CreateCmykJpeg(800, 600);
		var thumbnailJpeg = JpegExamples.CreateThumbnailJpeg();
		var jpegWithExif = JpegExamples.CreateJpegWithExifData(800, 600);

		// Assert
		Assert.NotNull(rgbJpeg.Metadata);
		Assert.NotNull(grayscaleJpeg.Metadata);
		Assert.NotNull(progressiveJpeg.Metadata);
		Assert.NotNull(photographyJpeg.Metadata);
		Assert.NotNull(cmykJpeg.Metadata);
		Assert.NotNull(thumbnailJpeg.Metadata);
		Assert.NotNull(jpegWithExif.Metadata);
	}

	[Fact]
	public void AllExampleMethods_ShouldHaveCorrectSoftwareMetadata()
	{
		// Act
		var rgbJpeg = JpegExamples.CreateRgbJpeg(800, 600);
		var grayscaleJpeg = JpegExamples.CreateGrayscaleJpeg(800, 600);
		var progressiveJpeg = JpegExamples.CreateProgressiveJpeg(800, 600);
		var photographyJpeg = JpegExamples.CreatePhotographyJpeg(800, 600);
		var cmykJpeg = JpegExamples.CreateCmykJpeg(800, 600);
		var thumbnailJpeg = JpegExamples.CreateThumbnailJpeg();
		var jpegWithExif = JpegExamples.CreateJpegWithExifData(800, 600);

		// Assert
		Assert.Equal("Wangkanai.Graphics.Rasters", rgbJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", grayscaleJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", progressiveJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", photographyJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", cmykJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", thumbnailJpeg.Metadata.Software);
		Assert.Equal("Wangkanai.Graphics.Rasters", jpegWithExif.Metadata.Software);
	}

	[Fact]
	public void ExampleJpegs_ShouldHaveReasonableFileSizes()
	{
		// Act
		var rgbJpeg = JpegExamples.CreateRgbJpeg(800, 600, 75);
		var grayscaleJpeg = JpegExamples.CreateGrayscaleJpeg(800, 600, 90);
		var progressiveJpeg = JpegExamples.CreateProgressiveJpeg(800, 600, 75);
		var photographyJpeg = JpegExamples.CreatePhotographyJpeg(800, 600);
		var cmykJpeg = JpegExamples.CreateCmykJpeg(800, 600);
		var thumbnailJpeg = JpegExamples.CreateThumbnailJpeg();

		// Assert
		Assert.True(rgbJpeg.GetEstimatedFileSize() > 0);
		Assert.True(grayscaleJpeg.GetEstimatedFileSize() > 0);
		Assert.True(progressiveJpeg.GetEstimatedFileSize() > 0);
		Assert.True(photographyJpeg.GetEstimatedFileSize() > 0);
		Assert.True(cmykJpeg.GetEstimatedFileSize() > 0);
		Assert.True(thumbnailJpeg.GetEstimatedFileSize() > 0);

		// Assert grayscale should be smaller than RGB
		Assert.True(grayscaleJpeg.GetEstimatedFileSize() < rgbJpeg.GetEstimatedFileSize());

		// Assert thumbnail should be smallest
		Assert.True(thumbnailJpeg.GetEstimatedFileSize() < rgbJpeg.GetEstimatedFileSize());
	}
}
