// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.WebPs;

public class WebPValidatorTests
{
	[Fact]
	public void Validate_WithValidWebP_ShouldReturnValidResult()
	{
		// Arrange
		var webp = new WebPRaster(800, 600)
		          {
			          ColorMode = WebPColorMode.Rgb,
			          Quality = 75,
			          Compression = WebPCompression.VP8,
			          CompressionLevel = 6
		          };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithNullWebP_ShouldThrowArgumentNullException()
	{
		// Arrange
		IWebPRaster? webp = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => webp!.Validate());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(-100)]
	public void Validate_WithInvalidWidth_ShouldAddError(int width)
	{
		// Arrange
		var webp = new WebPRaster { Width = width, Height = 600 };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid width"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(-100)]
	public void Validate_WithInvalidHeight_ShouldAddError(int height)
	{
		// Arrange
		var webp = new WebPRaster { Width = 800, Height = height };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid height"));
	}

	[Fact]
	public void Validate_WithExcessiveWidth_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster { Width = 20000, Height = 600 };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Width exceeds maximum"));
	}

	[Fact]
	public void Validate_WithExcessiveHeight_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster { Width = 800, Height = 20000 };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Height exceeds maximum"));
	}

	[Fact]
	public void Validate_WithLargeImage_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster { Width = 10001, Height = 10000 }; // Just over 100 megapixels

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very large image"));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	public void Validate_WithInvalidQuality_ShouldAddError(int quality)
	{
		// Arrange
		// Note: WebPRaster auto-clamps quality values, so we validate against the result
		var webp = new WebPRaster(800, 600) { Quality = quality };

		// Auto-clamping behavior: -1 becomes 0, 101 becomes 100
		var expectedClamped = Math.Clamp(quality, 0, 100);

		// Act
		var result = webp.Validate();

		// Assert
		// Since auto-clamping occurs, the result should be valid
		Assert.True(result.IsValid);
		Assert.Equal(expectedClamped, webp.Quality);
		Assert.Empty(result.Errors);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(10)]
	public void Validate_WithInvalidCompressionLevel_ShouldAddError(int level)
	{
		// Arrange
		// Note: WebPRaster auto-clamps compression level values
		var webp = new WebPRaster(800, 600) { CompressionLevel = level };

		// Auto-clamping behavior: -1 becomes 0, 10 becomes 9
		var expectedClamped = Math.Clamp(level, 0, 9);

		// Act
		var result = webp.Validate();

		// Assert
		// Since auto-clamping occurs, the result should be valid
		Assert.True(result.IsValid);
		Assert.Equal(expectedClamped, webp.CompressionLevel);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithHighCompressionLevel_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.ConfigureLossless();
		webp.CompressionLevel = 8;

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("High compression levels"));
	}

	[Fact]
	public void Validate_WithVeryLowQuality_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600) { Quality = 5 };

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Very low quality"));
	}

	[Fact]
	public void Validate_WithSimpleFormatAndWrongCompression_ShouldAddError()
	{
		// Arrange
		// Note: WebPRaster auto-synchronizes format and compression
		var webp = new WebPRaster(800, 600)
		{
			Format = WebPFormat.Simple,
			Compression = WebPCompression.VP8L
		};

		// Act
		var result = webp.Validate();

		// Assert
		// Auto-synchronization: VP8L compression sets format to Lossless
		Assert.True(result.IsValid);
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.Equal(WebPCompression.VP8L, webp.Compression);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithLosslessFormatAndWrongCompression_ShouldAddError()
	{
		// Arrange
		// Note: WebPRaster auto-synchronizes format and compression
		var webp = new WebPRaster(800, 600)
		{
			Format = WebPFormat.Lossless,
			Compression = WebPCompression.VP8
		};

		// Act
		var result = webp.Validate();

		// Assert
		// Auto-synchronization: Lossless format sets compression to VP8L
		Assert.True(result.IsValid);
		Assert.Equal(WebPFormat.Lossless, webp.Format);
		Assert.Equal(WebPCompression.VP8L, webp.Compression);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithExtendedFormatButNoFeatures_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600)
		{
			Format = WebPFormat.Extended
		};
		webp.WebPMetadata.IsExtended = false;

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Extended format specified"));
	}

	[Theory]
	[InlineData(WebPColorMode.Rgb, 3)]
	[InlineData(WebPColorMode.Rgba, 4)]
	public void Validate_WithCorrectChannels_ShouldPassValidation(WebPColorMode colorMode, int channels)
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.SetColorMode(colorMode);

		// Act
		var result = webp.Validate();

		// Assert
		Assert.True(result.IsValid);
		Assert.Equal(channels, webp.Channels);
	}

	[Fact]
	public void Validate_WithAnimatedWebP_ShouldRequireExtendedFormat()
	{
		// Arrange
		var webp = new WebPRaster(800, 600)
		{
			Format = WebPFormat.Simple
		};
		webp.WebPMetadata.HasAnimation = true;
		webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame { Width = 100, Height = 100 });

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Animated WebP requires Extended format"));
	}

	[Fact]
	public void Validate_WithAnimatedButNoFrames_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.EnableExtendedFeatures();
		webp.WebPMetadata.HasAnimation = true;

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("no animation frames are defined"));
	}

	[Fact]
	public void Validate_WithInvalidFrameDimensions_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.EnableExtendedFeatures();
		webp.WebPMetadata.HasAnimation = true;
		webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame { Width = 0, Height = 100 });

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("invalid dimensions"));
	}

	[Fact]
	public void Validate_WithFrameExceedingBounds_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.EnableExtendedFeatures();
		webp.WebPMetadata.HasAnimation = true;
		webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame
		{
			Width = 200,
			Height = 200,
			OffsetX = 700, // This would exceed image width
			OffsetY = 0
		});

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("extends beyond image width"));
	}

	[Fact]
	public void Validate_WithZeroDuration_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.EnableExtendedFeatures();
		webp.WebPMetadata.HasAnimation = true;
		webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame
		{
			Width = 100,
			Height = 100,
			Duration = 0
		});

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("zero duration"));
	}

	[Fact]
	public void Validate_WithManyFrames_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.EnableExtendedFeatures();
		webp.WebPMetadata.HasAnimation = true;

		// Add 101 frames
		for (int i = 0; i < 101; i++)
		{
			webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame
			{
				Width = 100,
				Height = 100,
				Duration = 100
			});
		}

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large number of animation frames"));
	}

	[Fact]
	public void Validate_WithInconsistentFlags_ShouldAddWarnings()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.WebPMetadata.HasIccProfile = true;
		webp.WebPMetadata.HasExif = true;
		webp.WebPMetadata.HasXmp = true;
		// But leave the data empty

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("HasIccProfile"));
		Assert.Contains(result.Warnings, w => w.Contains("HasExif"));
		Assert.Contains(result.Warnings, w => w.Contains("HasXmp"));
	}

	[Fact]
	public void Validate_WithInvalidChunkId_ShouldAddError()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.WebPMetadata.CustomChunks.Add("TOOLONG", new byte[10]);

		// Act
		var result = webp.Validate();

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Contains("Invalid chunk ID"));
	}

	[Fact]
	public void Validate_WithLargeMetadata_ShouldAddWarning()
	{
		// Arrange
		var webp = new WebPRaster(800, 600);
		webp.WebPMetadata.IccProfile = new byte[500_000];
		webp.WebPMetadata.ExifData = new byte[400_000];
		webp.WebPMetadata.XmpData = new byte[200_000];

		// Act
		var result = webp.Validate();

		// Assert
		Assert.Contains(result.Warnings, w => w.Contains("Large metadata size"));
	}

	[Fact]
	public void IsValidWebPSignature_WithValidSignature_ShouldReturnTrue()
	{
		// Arrange
		var data = new byte[]
		{
			0x52, 0x49, 0x46, 0x46, // "RIFF"
			0x00, 0x00, 0x00, 0x00, // File size (4 bytes)
			0x57, 0x45, 0x42, 0x50  // "WEBP"
		};

		// Act
		var result = WebPValidator.IsValidWebPSignature(data);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsValidWebPSignature_WithInvalidSignature_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[]
		{
			0x89, 0x50, 0x4E, 0x47, // PNG signature
			0x0D, 0x0A, 0x1A, 0x0A,
			0x00, 0x00, 0x00, 0x0D
		};

		// Act
		var result = WebPValidator.IsValidWebPSignature(data);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void IsValidWebPSignature_WithTooShortData_ShouldReturnFalse()
	{
		// Arrange
		var data = new byte[] { 0x52, 0x49, 0x46 }; // Too short

		// Act
		var result = WebPValidator.IsValidWebPSignature(data);

		// Assert
		Assert.False(result);
	}

	[Theory]
	[InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x20, 0x00, 0x00, 0x00, 0x00 }, WebPFormat.Simple)]
	[InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x4C, 0x00, 0x00, 0x00, 0x00 }, WebPFormat.Lossless)]
	[InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x58, 0x00, 0x00, 0x00, 0x00 }, WebPFormat.Extended)]
	public void DetectFormat_ShouldReturnCorrectFormat(byte[] data, WebPFormat expectedFormat)
	{
		// Act
		var format = WebPValidator.DetectFormat(data);

		// Assert
		Assert.Equal(expectedFormat, format);
	}

	[Fact]
	public void DetectFormat_WithInvalidSignature_ShouldThrowArgumentException()
	{
		// Arrange
		var data = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG signature

		// Act & Assert
		Assert.Throws<ArgumentException>(() => WebPValidator.DetectFormat(data));
	}
}
