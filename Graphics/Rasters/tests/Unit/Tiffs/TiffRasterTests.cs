// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

public class TiffRasterTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaultValues()
	{
		// Arrange & Act
		var tiffRaster = new TiffRaster();

		// Assert
		Assert.Equal(TiffColorDepth.TwentyFourBit, tiffRaster.ColorDepth);
		Assert.Equal(TiffCompression.None, tiffRaster.Compression);
		Assert.Equal(PhotometricInterpretation.Rgb, tiffRaster.PhotometricInterpretation);
		Assert.Equal(3, tiffRaster.SamplesPerPixel);
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(new[] { 8, 8, 8 }));
		Assert.False(tiffRaster.HasAlpha);
		Assert.Equal(1, tiffRaster.PlanarConfiguration);
		Assert.NotNull(tiffRaster.Metadata);
	}

	[Fact]
	public void Constructor_WithDimensions_ShouldSetWidthAndHeight()
	{
		// Arrange
		const int width = 1024;
		const int height = 768;

		// Act
		var tiffRaster = new TiffRaster(width, height);

		// Assert
		Assert.Equal(width, tiffRaster.Width);
		Assert.Equal(height, tiffRaster.Height);
	}

	[Fact]
	public void ColorDepth_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act
		tiffRaster.ColorDepth = TiffColorDepth.SixteenBit;

		// Assert
		Assert.Equal(TiffColorDepth.SixteenBit, tiffRaster.ColorDepth);
	}

	[Fact]
	public void Compression_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act
		tiffRaster.Compression = TiffCompression.Lzw;

		// Assert
		Assert.Equal(TiffCompression.Lzw, tiffRaster.Compression);
	}

	[Fact]
	public void Metadata_CanBeModified()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var metadata = new TiffMetadata
		{
			ImageDescription = "Test Image",
			Make = "Test Camera",
			Model = "Test Model"
		};

		// Act
		tiffRaster.Metadata = metadata;

		// Assert
		Assert.Equal("Test Image", tiffRaster.Metadata.ImageDescription);
		Assert.Equal("Test Camera", tiffRaster.Metadata.Make);
		Assert.Equal("Test Model", tiffRaster.Metadata.Model);
	}

	[Fact]
	public void SetBitsPerSample_WithArray_ShouldUpdateBitsPerSample()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var newBitsPerSample = new[] { 16, 16, 16, 16 };

		// Act
		tiffRaster.SetBitsPerSample(newBitsPerSample);

		// Assert
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(newBitsPerSample));
	}

	[Fact]
	public void SetBitsPerSample_WithSpan_ShouldUpdateBitsPerSample()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var newBitsPerSample = new[] { 32, 32 };

		// Act
		tiffRaster.SetBitsPerSample(newBitsPerSample.AsSpan());

		// Assert
		Assert.True(tiffRaster.BitsPerSample.SequenceEqual(newBitsPerSample));
	}

	[Fact]
	public void BitsPerSample_ShouldReturnReadOnlySpan()
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act
		var bitsPerSample = tiffRaster.BitsPerSample;

		// Assert
		Assert.Equal(3, bitsPerSample.Length);
		Assert.True(bitsPerSample.SequenceEqual(new[] { 8, 8, 8 }));
	}

	[Theory]
	[InlineData(new int[] { })]
	[InlineData(new[] { 8 })]
	[InlineData(new[] { 8, 8 })]
	[InlineData(new[] { 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8, 8 })]
	[InlineData(new[] { 8, 8, 8, 8, 8, 8, 8, 8 })]
	public void BitsPerSample_WithVariousSampleCounts_ShouldStoreAndRetrieveCorrectly(int[] expected)
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;

		// Assert
		Assert.Equal(expected.Length, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}

	[Fact]
	public void BitsPerSample_WithInlineStorage_ShouldUseStackAllocation()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var expected = new[] { 16, 16, 16 };

		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;

		// Assert - Verify behavior for inline storage (1-4 samples)
		Assert.Equal(3, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}

	[Fact]
	public void BitsPerSample_WithLargeArray_ShouldFallbackToHeapAllocation()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var expected = new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 }; // More than 4 samples

		// Act
		tiffRaster.SetBitsPerSample(expected);
		var actual = tiffRaster.BitsPerSample;

		// Assert - Verify behavior for heap allocation (>4 samples)
		Assert.Equal(10, actual.Length);
		Assert.True(actual.SequenceEqual(expected));
	}

	[Fact]
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act & Assert
		var exception = Record.Exception(() => tiffRaster.Dispose());
		Assert.Null(exception);
	}

	[Fact]
	public void EstimatedMetadataSize_WithBasicMetadata_ShouldCalculateCorrectly()
	{
		// Arrange
		var tiffRaster = new TiffRaster();

		// Act
		var initialSize = tiffRaster.EstimatedMetadataSize;

		// Assert - Should include basic TIFF directory overhead
		Assert.True(initialSize > 0);
		Assert.True(initialSize < ImageConstants.LargeMetadataThreshold); // Basic metadata should be small
	}

	[Fact]
	public void EstimatedMetadataSize_WithStringMetadata_ShouldIncludeStringSize()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var testDescription = "Test image description with substantial text content";

		// Act
		var initialSize = tiffRaster.EstimatedMetadataSize;
		tiffRaster.Metadata.ImageDescription = testDescription;
		var sizeWithDescription = tiffRaster.EstimatedMetadataSize;

		// Assert
		var expectedIncrease = System.Text.Encoding.UTF8.GetByteCount(testDescription);
		Assert.Equal(expectedIncrease, sizeWithDescription - initialSize);
	}

	[Fact]
	public void EstimatedMetadataSize_WithStripData_ShouldIncludeArraySizes()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var stripOffsets = new int[100];
		var stripByteCounts = new int[100];

		// Act
		var initialSize = tiffRaster.EstimatedMetadataSize;
		tiffRaster.Metadata.StripOffsets = stripOffsets;
		tiffRaster.Metadata.StripByteCounts = stripByteCounts;
		var sizeWithStrips = tiffRaster.EstimatedMetadataSize;

		// Assert
		var expectedIncrease = (stripOffsets.Length + stripByteCounts.Length) * sizeof(int);
		Assert.Equal(expectedIncrease, sizeWithStrips - initialSize);
	}

	[Fact]
	public void EstimatedMetadataSize_WithEmbeddedMetadata_ShouldIncludeByteArraySizes()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var exifData = new byte[1000];
		var iccProfile = new byte[2000];
		var xmpData = new byte[500];

		// Act
		var initialSize = tiffRaster.EstimatedMetadataSize;
		tiffRaster.Metadata.ExifIfd = exifData;
		tiffRaster.Metadata.IccProfile = iccProfile;
		tiffRaster.Metadata.XmpData = xmpData;
		var sizeWithEmbedded = tiffRaster.EstimatedMetadataSize;

		// Assert
		var expectedIncrease = exifData.Length + iccProfile.Length + xmpData.Length;
		Assert.Equal(expectedIncrease, sizeWithEmbedded - initialSize);
	}

	[Fact]
	public void EstimatedMetadataSize_WithCustomTags_ShouldCalculateAccurately()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var testString = "Custom tag value";
		var testBytes = new byte[100];
		var testInts = new int[10];

		// Act
		var initialSize = tiffRaster.EstimatedMetadataSize;
		tiffRaster.Metadata.CustomTags[256] = testString;
		tiffRaster.Metadata.CustomTags[257] = testBytes;
		tiffRaster.Metadata.CustomTags[258] = testInts;
		var sizeWithCustomTags = tiffRaster.EstimatedMetadataSize;

		// Assert
		var expectedIncrease = System.Text.Encoding.UTF8.GetByteCount(testString) +
		                      testBytes.Length +
		                      (testInts.Length * sizeof(int)) +
		                      (3 * 12); // 3 directory entries, 12 bytes each
		Assert.Equal(expectedIncrease, sizeWithCustomTags - initialSize);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeMetadata_ShouldReturnTrue()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		var largeData = new byte[ImageConstants.LargeMetadataThreshold + 1000];

		// Act
		tiffRaster.Metadata.IccProfile = largeData;

		// Assert
		Assert.True(tiffRaster.HasLargeMetadata);
		Assert.True(tiffRaster.EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold);
	}

	[Fact]
	public async Task DisposeAsync_WithLargeMetadata_ShouldClearAllMetadata()
	{
		// Arrange
		var tiffRaster = new TiffRaster();
		tiffRaster.Metadata.ImageDescription = "Test";
		tiffRaster.Metadata.StripOffsets = new int[100];
		tiffRaster.Metadata.IccProfile = new byte[ImageConstants.LargeMetadataThreshold + 1000];

		// Verify we have large metadata
		Assert.True(tiffRaster.HasLargeMetadata);

		// Act
		await tiffRaster.DisposeAsync();

		// Assert - All metadata should be cleared
		Assert.Null(tiffRaster.Metadata.ImageDescription);
		Assert.Null(tiffRaster.Metadata.StripOffsets);
		Assert.Null(tiffRaster.Metadata.IccProfile);
	}
}
