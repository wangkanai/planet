// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Heifs;

public class HeifEncodingOptionsTests
{
	[Fact]
	public void DefaultConstructor_SetsCorrectDefaults()
	{
		// Act
		var options = new HeifEncodingOptions();

		// Assert
		Assert.Equal(HeifConstants.DefaultQuality, options.Quality);
		Assert.Equal(HeifConstants.DefaultSpeed, options.Speed);
		Assert.False(options.IsLossless);
		Assert.Equal(HeifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.Null(options.ThreadCount);
		Assert.Null(options.Compression);
		Assert.Null(options.Profile);
		Assert.False(options.EnableProgressiveDecoding);
		Assert.True(options.GenerateThumbnails);
		Assert.Equal(HeifConstants.Memory.DefaultPixelBufferSizeMB, options.MaxPixelBufferSizeMB);
		Assert.Equal(HeifConstants.Memory.DefaultMetadataBufferSizeMB, options.MaxMetadataBufferSizeMB);
		Assert.Equal(HeifConstants.Memory.DefaultTileSize, options.TileSize);
		Assert.True(options.PreserveMetadata);
		Assert.True(options.PreserveColorProfile);
		Assert.NotNull(options.CodecParameters);
		Assert.NotNull(options.CustomParameters);
	}

	[Fact]
	public void CreateDefault_ReturnsValidOptions()
	{
		// Act
		var options = HeifEncodingOptions.CreateDefault();

		// Assert
		Assert.NotNull(options);
		Assert.Equal(HeifConstants.DefaultQuality, options.Quality);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateLossless_ReturnsLosslessConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateLossless();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Lossless, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slow, options.Speed);
		Assert.True(options.IsLossless);
		Assert.Equal(HeifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateWebOptimized_ReturnsWebConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateWebOptimized();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Web, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fast, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.True(options.GenerateThumbnails);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateHighQuality_ReturnsHighQualityConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateHighQuality();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slow, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateFast_ReturnsFastConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateFast();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Standard, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fastest, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateHdr_ReturnsHdrConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateHdr();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.High, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Medium, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.Equal(HeifCompression.Hevc, options.Compression);
		Assert.Equal(HeifProfile.Main10, options.Profile);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateThumbnail_ReturnsThumbnailConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateThumbnail();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Thumbnail, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fast, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.False(options.GenerateThumbnails);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateMobile_ReturnsMobileConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateMobile();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.Mobile, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Fast, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.Equal(256, options.MaxPixelBufferSizeMB);
		Assert.Equal(512, options.TileSize);
		Assert.True(options.Validate(out _));
	}

	[Fact]
	public void CreateArchival_ReturnsArchivalConfiguration()
	{
		// Act
		var options = HeifEncodingOptions.CreateArchival();

		// Assert
		Assert.Equal(HeifConstants.QualityPresets.NearLossless, options.Quality);
		Assert.Equal(HeifConstants.SpeedPresets.Slowest, options.Speed);
		Assert.Equal(HeifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.True(options.PreserveMetadata);
		Assert.True(options.PreserveColorProfile);
		Assert.True(options.Validate(out _));
	}

	[Theory]
	[InlineData(-1)] // Below minimum
	[InlineData(101)] // Above maximum
	public void Validate_WithInvalidQuality_ReturnsFalse(int quality)
	{
		// Arrange
		var options = new HeifEncodingOptions { Quality = quality };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Quality", error);
	}

	[Theory]
	[InlineData(-1)] // Below minimum
	[InlineData(10)] // Above maximum
	public void Validate_WithInvalidSpeed_ReturnsFalse(int speed)
	{
		// Arrange
		var options = new HeifEncodingOptions { Speed = speed };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Speed", error);
	}

	[Fact]
	public void Validate_WithLosslessAndWrongQuality_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions
		{
			IsLossless = true,
			Quality = 90 // Should be 100 for lossless
		};

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Lossless mode requires quality to be 100", error);
	}

	[Fact]
	public void Validate_WithNegativeThreadCount_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions { ThreadCount = -1 };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Thread count cannot be negative", error);
	}

	[Fact]
	public void Validate_WithExcessiveThreadCount_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions { ThreadCount = HeifConstants.Memory.MaxThreads + 1 };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains($"Thread count cannot exceed {HeifConstants.Memory.MaxThreads}", error);
	}

	[Theory]
	[InlineData(0)] // Zero
	[InlineData(-1)] // Negative
	public void Validate_WithInvalidPixelBufferSize_ReturnsFalse(int bufferSize)
	{
		// Arrange
		var options = new HeifEncodingOptions { MaxPixelBufferSizeMB = bufferSize };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Pixel buffer size", error);
	}

	[Fact]
	public void Validate_WithExcessivePixelBufferSize_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions { MaxPixelBufferSizeMB = HeifConstants.Memory.MaxPixelBufferSizeMB + 1 };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Pixel buffer size", error);
	}

	[Theory]
	[InlineData(0)] // Zero
	[InlineData(-1)] // Negative
	public void Validate_WithInvalidMetadataBufferSize_ReturnsFalse(int bufferSize)
	{
		// Arrange
		var options = new HeifEncodingOptions { MaxMetadataBufferSizeMB = bufferSize };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Metadata buffer size", error);
	}

	[Fact]
	public void Validate_WithExcessiveMetadataBufferSize_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions { MaxMetadataBufferSizeMB = HeifConstants.Memory.MaxMetadataBufferSizeMB + 1 };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Metadata buffer size", error);
	}

	[Theory]
	[InlineData(0)] // Zero
	[InlineData(-1)] // Negative
	[InlineData(8193)] // Too large
	public void Validate_WithInvalidTileSize_ReturnsFalse(int tileSize)
	{
		// Arrange
		var options = new HeifEncodingOptions { TileSize = tileSize };

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Tile size", error);
	}

	[Fact]
	public void Validate_WithJpegAndLossless_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions
		{
			Compression = HeifCompression.Jpeg,
			IsLossless = true,
			Quality = HeifConstants.QualityPresets.Lossless
		};

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("JPEG compression cannot be used with lossless mode", error);
	}

	[Fact]
	public void Validate_WithMain10AndMonochrome_ReturnsFalse()
	{
		// Arrange
		var options = new HeifEncodingOptions
		{
			Profile = HeifProfile.Main10,
			ChromaSubsampling = HeifChromaSubsampling.Yuv400
		};

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.False(isValid);
		Assert.NotNull(error);
		Assert.Contains("Main 10 profile cannot be used with monochrome", error);
	}

	[Fact]
	public void Validate_WithValidConfiguration_ReturnsTrue()
	{
		// Arrange
		var options = new HeifEncodingOptions
		{
			Quality = 85,
			Speed = 5,
			ThreadCount = 4,
			MaxPixelBufferSizeMB = 256,
			MaxMetadataBufferSizeMB = 32,
			TileSize = 1024
		};

		// Act
		var isValid = options.Validate(out var error);

		// Assert
		Assert.True(isValid);
		Assert.Null(error);
	}

	[Fact]
	public void Clone_CreatesIndependentCopy()
	{
		// Arrange
		var original = new HeifEncodingOptions
		{
			Quality = 90,
			Speed = 3,
			IsLossless = true,
			ThreadCount = 8,
			Compression = HeifCompression.Av1,
			Profile = HeifProfile.Main10
		};
		original.CodecParameters["test"] = "value";
		original.CustomParameters["custom"] = "data";

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.Quality, clone.Quality);
		Assert.Equal(original.Speed, clone.Speed);
		Assert.Equal(original.IsLossless, clone.IsLossless);
		Assert.Equal(original.ThreadCount, clone.ThreadCount);
		Assert.Equal(original.Compression, clone.Compression);
		Assert.Equal(original.Profile, clone.Profile);

		// Verify collections are independent
		Assert.NotSame(original.CodecParameters, clone.CodecParameters);
		Assert.NotSame(original.CustomParameters, clone.CustomParameters);
		Assert.Equal(original.CodecParameters["test"], clone.CodecParameters["test"]);
		Assert.Equal(original.CustomParameters["custom"], clone.CustomParameters["custom"]);

		// Modify original to ensure independence
		original.Quality = 50;
		original.CodecParameters["test"] = "modified";
		Assert.NotEqual(original.Quality, clone.Quality);
		Assert.NotEqual(original.CodecParameters["test"], clone.CodecParameters["test"]);
	}
}
