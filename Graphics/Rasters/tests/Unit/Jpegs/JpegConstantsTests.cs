// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

public class JpegConstantsTests
{
	[Fact]
	public void JpegConstants_ShouldHaveCorrectValues()
	{
		// Assert - JPEG markers
		Assert.Equal(0xFFD8, JpegConstants.StartOfImage);
		Assert.Equal(0xFFD9, JpegConstants.EndOfImage);
		Assert.Equal(0xFFE0, JpegConstants.App0Marker);
		Assert.Equal(0xFFE1, JpegConstants.App1Marker);
		Assert.Equal(0xFFE2, JpegConstants.App2Marker);
		Assert.Equal(0xFFC0, JpegConstants.StartOfFrameBaseline);
		Assert.Equal(0xFFC2, JpegConstants.StartOfFrameProgressive);
		Assert.Equal(0xFFC4, JpegConstants.DefineHuffmanTable);
		Assert.Equal(0xFFDB, JpegConstants.DefineQuantizationTable);
		Assert.Equal(0xFFDA, JpegConstants.StartOfScan);
		Assert.Equal(0xFFD0, JpegConstants.RestartMarkerStart);
		Assert.Equal(0xFFD7, JpegConstants.RestartMarkerEnd);

		// Assert - Dimension limits
		Assert.Equal(65535, JpegConstants.MaxDimension);

		// Assert - Quality range
		Assert.Equal(0, JpegConstants.MinQuality);
		Assert.Equal(100, JpegConstants.MaxQuality);
		Assert.Equal(75, JpegConstants.DefaultQuality);

		// Assert - Bits per sample
		Assert.Equal(8, JpegConstants.BitsPerSample);

		// Assert - Identifiers
		Assert.Equal("JFIF", JpegConstants.JfifIdentifier);
		Assert.Equal("Exif", JpegConstants.ExifIdentifier);
	}

	[Fact]
	public void JpegFileExtensions_ShouldHaveCorrectValues()
	{
		// Assert - Individual extensions
		Assert.Equal(".jpg", JpegFileExtensions.Jpg);
		Assert.Equal(".jpeg", JpegFileExtensions.Jpeg);
		Assert.Equal(".jpe", JpegFileExtensions.Jpe);
		Assert.Equal(".jfif", JpegFileExtensions.Jfif);

		// Assert - All extensions array
		Assert.Equal(4, JpegFileExtensions.All.Length);
		Assert.Contains(".jpg", JpegFileExtensions.All);
		Assert.Contains(".jpeg", JpegFileExtensions.All);
		Assert.Contains(".jpe", JpegFileExtensions.All);
		Assert.Contains(".jfif", JpegFileExtensions.All);
	}

	[Fact]
	public void JpegMimeTypes_ShouldHaveCorrectValues()
	{
		// Assert - MIME types
		Assert.Equal("image/jpeg", JpegMimeTypes.ImageJpeg);
		Assert.Equal("image/jpg", JpegMimeTypes.ImageJpg);
		Assert.Equal("image/jfif", JpegMimeTypes.ImageJfif);
	}

	[Fact]
	public void QualityRange_ShouldBeValid()
	{
		// Assert - Quality range is valid
		Assert.True(JpegConstants.MinQuality >= 0);
		Assert.True(JpegConstants.MaxQuality <= 100);
		Assert.True(JpegConstants.MinQuality < JpegConstants.MaxQuality);
		Assert.True(JpegConstants.DefaultQuality >= JpegConstants.MinQuality);
		Assert.True(JpegConstants.DefaultQuality <= JpegConstants.MaxQuality);
	}

	[Fact]
	public void RestartMarkerRange_ShouldBeValid()
	{
		// Assert - Restart marker range is valid
		Assert.True(JpegConstants.RestartMarkerStart < JpegConstants.RestartMarkerEnd);
		Assert.Equal(8, JpegConstants.RestartMarkerEnd - JpegConstants.RestartMarkerStart + 1);
	}

	[Fact]
	public void MaxDimension_ShouldBe16BitMaxValue()
	{
		// Assert - Maximum dimension should be 16-bit max value
		Assert.Equal(ushort.MaxValue, JpegConstants.MaxDimension);
	}

	[Fact]
	public void BitsPerSample_ShouldBeStandardJpegValue()
	{
		// Assert - JPEG standard is 8 bits per sample
		Assert.Equal(8, JpegConstants.BitsPerSample);
	}
}

public class JpegColorModeTests
{
	[Fact]
	public void JpegColorMode_ShouldHaveCorrectValues()
	{
		// Assert - Color mode values match expected samples per pixel
		Assert.Equal(1, (int)JpegColorMode.Grayscale);
		Assert.Equal(3, (int)JpegColorMode.Rgb);
		Assert.Equal(4, (int)JpegColorMode.Cmyk);
		Assert.Equal(6, (int)JpegColorMode.YCbCr);
	}

	[Fact]
	public void JpegColorMode_AllValuesAreDefined()
	{
		// Arrange
		var colorModes = Enum.GetValues<JpegColorMode>();

		// Assert
		Assert.Equal(4, colorModes.Length);
		Assert.Contains(JpegColorMode.Grayscale, colorModes);
		Assert.Contains(JpegColorMode.Rgb, colorModes);
		Assert.Contains(JpegColorMode.Cmyk, colorModes);
		Assert.Contains(JpegColorMode.YCbCr, colorModes);
	}
}

public class JpegEncodingTests
{
	[Fact]
	public void JpegEncoding_ShouldHaveCorrectValues()
	{
		// Assert - Encoding values
		Assert.Equal(0, (int)JpegEncoding.Baseline);
		Assert.Equal(1, (int)JpegEncoding.Progressive);
		Assert.Equal(2, (int)JpegEncoding.Jpeg2000);
	}

	[Fact]
	public void JpegEncoding_AllValuesAreDefined()
	{
		// Arrange
		var encodings = Enum.GetValues<JpegEncoding>();

		// Assert
		Assert.Equal(3, encodings.Length);
		Assert.Contains(JpegEncoding.Baseline, encodings);
		Assert.Contains(JpegEncoding.Progressive, encodings);
		Assert.Contains(JpegEncoding.Jpeg2000, encodings);
	}
}

public class JpegChromaSubsamplingTests
{
	[Fact]
	public void JpegChromaSubsampling_ShouldHaveCorrectValues()
	{
		// Assert - Chroma subsampling values
		Assert.Equal(0, (int)JpegChromaSubsampling.None);
		Assert.Equal(1, (int)JpegChromaSubsampling.Horizontal);
		Assert.Equal(2, (int)JpegChromaSubsampling.Both);
	}

	[Fact]
	public void JpegChromaSubsampling_AllValuesAreDefined()
	{
		// Arrange
		var subsamplings = Enum.GetValues<JpegChromaSubsampling>();

		// Assert
		Assert.Equal(3, subsamplings.Length);
		Assert.Contains(JpegChromaSubsampling.None, subsamplings);
		Assert.Contains(JpegChromaSubsampling.Horizontal, subsamplings);
		Assert.Contains(JpegChromaSubsampling.Both, subsamplings);
	}
}

public class JpegSpecificationTests
{
	[Theory]
	[InlineData("test.jpg")]
	[InlineData("test.jpeg")]
	[InlineData("test.jpe")]
	[InlineData("test.jfif")]
	public void FileExtensions_ShouldBeValidJpegExtensions(string filename)
	{
		// Act
		var extension = Path.GetExtension(filename);

		// Assert
		Assert.Contains(extension, JpegFileExtensions.All);
	}

	[Theory]
	[InlineData("image/jpeg")]
	[InlineData("image/jpg")]
	[InlineData("image/jfif")]
	public void MimeTypes_ShouldBeValidJpegMimeTypes(string mimeType)
	{
		// Assert
		Assert.True(
			mimeType == JpegMimeTypes.ImageJpeg ||
			mimeType == JpegMimeTypes.ImageJpg ||
			mimeType == JpegMimeTypes.ImageJfif
		);
	}

	[Theory]
	[InlineData(1, 1)] // Minimum valid dimensions
	[InlineData(65535, 65535)] // Maximum valid dimensions
	[InlineData(1920, 1080)] // Common HD resolution
	[InlineData(3840, 2160)] // Common 4K resolution
	public void Dimensions_ShouldBeWithinJpegLimits(int width, int height)
	{
		// Assert
		Assert.True(width > 0 && width <= JpegConstants.MaxDimension);
		Assert.True(height > 0 && height <= JpegConstants.MaxDimension);
	}

	[Theory]
	[InlineData(0)] // Minimum quality
	[InlineData(50)] // Medium quality
	[InlineData(75)] // Default quality
	[InlineData(100)] // Maximum quality
	public void Quality_ShouldBeWithinJpegRange(int quality)
	{
		// Assert
		Assert.True(quality >= JpegConstants.MinQuality);
		Assert.True(quality <= JpegConstants.MaxQuality);
	}

	[Fact]
	public void BitsPerSample_ShouldBe8ForAllColorModes()
	{
		// Assert - JPEG standard is 8 bits per sample for all color modes
		Assert.Equal(8, JpegConstants.BitsPerSample);
	}

	[Fact]
	public void JpegSignature_ShouldMatchStandard()
	{
		// Assert - JPEG files should start with SOI marker
		Assert.Equal(0xFF, (JpegConstants.StartOfImage >> 8) & 0xFF);
		Assert.Equal(0xD8, JpegConstants.StartOfImage & 0xFF);
	}
}
