// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics;
using Wangkanai.Graphics.Rasters.WebPs;
using Wangkanai.Graphics.Rasters.Jpegs;
using Wangkanai.Graphics.Rasters.Tiffs;
using Wangkanai.Graphics.Rasters.Pngs;
using Wangkanai.Graphics.Vectors;

namespace Wangkanai.Graphics.UnitTests.Abstractions;

/// <summary>
/// Tests for GitHub Issue #80 - IAsyncDisposable implementation for IImage interface
/// to handle large metadata efficiently.
/// </summary>
public class IImageAsyncDisposalTests
{
	[Fact]
	public async Task IImage_SupportsAsyncDisposable()
	{
		// Arrange
		IImage image = new WebPRaster();
		
		// Act & Assert - Should compile and not throw
		await image.DisposeAsync();
	}

	[Fact]
	public void IImage_HasMetadataProperties()
	{
		// Arrange
		IImage image = new WebPRaster();
		
		// Act & Assert
		Assert.NotNull(image);
		Assert.True(image.HasLargeMetadata == false); // Should be false for empty metadata
		Assert.True(image.EstimatedMetadataSize >= 0);
	}

	[Fact]
	public void WebPRaster_WithLargeMetadata_ShouldDetectCorrectly()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB
		
		// Act & Assert
		Assert.True(webp.HasLargeMetadata);
		Assert.True(webp.EstimatedMetadataSize > 1_000_000);
	}

	[Fact]
	public async Task WebPRaster_AsyncDisposal_WithLargeMetadata_ShouldComplete()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB
		webp.Metadata.ExifData = new byte[500_000];     // 500KB
		webp.Metadata.XmpData = new byte[300_000];      // 300KB
		
		// Verify large metadata detection
		Assert.True(webp.HasLargeMetadata);
		
		// Act & Assert - Should not throw
		await webp.DisposeAsync();
		
		// Verify cleanup
		Assert.True(webp.Metadata.IccProfile.IsEmpty);
		Assert.True(webp.Metadata.ExifData.IsEmpty);
		Assert.True(webp.Metadata.XmpData.IsEmpty);
	}

	[Fact]
	public async Task JpegRaster_AsyncDisposal_WithMetadata_ShouldComplete()
	{
		// Arrange
		var jpeg = new JpegRaster();
		jpeg.Metadata.IccProfile = new byte[100_000]; // 100KB
		
		// Add many EXIF tags to increase metadata size
		for (int i = 0; i < 10000; i++)
		{
			jpeg.Metadata.CustomExifTags.Add(i, $"Tag value {i} with some content");
		}
		
		// Act & Assert
		await jpeg.DisposeAsync();
		
		// Verify cleanup
		Assert.Null(jpeg.Metadata.IccProfile);
		Assert.Empty(jpeg.Metadata.CustomExifTags);
	}

	[Fact]
	public async Task TiffRaster_AsyncDisposal_WithMetadata_ShouldComplete()
	{
		// Arrange
		var tiff = new TiffRaster();
		tiff.Metadata.ImageDescription = new string('A', 50_000); // 50KB string
		tiff.Metadata.Make = "Test Camera";
		tiff.Metadata.Model = "Test Model";
		tiff.Metadata.Software = "Test Software";
		
		// Add custom tags
		for (int i = 0; i < 1000; i++)
		{
			tiff.Metadata.CustomTags.Add(i, $"Custom tag value {i}");
		}
		
		// Act & Assert
		await tiff.DisposeAsync();
		
		// Verify cleanup
		Assert.Null(tiff.Metadata.ImageDescription);
		Assert.Null(tiff.Metadata.Make);
		Assert.Null(tiff.Metadata.Model);
		Assert.Null(tiff.Metadata.Software);
		Assert.Empty(tiff.Metadata.CustomTags);
	}

	[Fact]
	public async Task PngRaster_AsyncDisposal_WithLargeTextChunks_ShouldComplete()
	{
		// Arrange
		var png = new PngRaster();
		
		// Add many text chunks to create large metadata
		for (int i = 0; i < 5000; i++)
		{
			png.Metadata.TextChunks.Add($"key{i}", new string('X', 200)); // 200 chars each
		}
		
		// Add custom chunks
		for (int i = 0; i < 100; i++)
		{
			png.Metadata.CustomChunks.Add($"CHNK{i:D4}", new byte[1000]);
		}
		
		// Verify large metadata
		Assert.True(png.HasLargeMetadata);
		
		// Act & Assert
		await png.DisposeAsync();
		
		// Verify cleanup
		Assert.Empty(png.Metadata.TextChunks);
		Assert.Empty(png.Metadata.CustomChunks);
	}

	[Fact]
	public async Task AsyncDisposal_CalledMultipleTimes_ShouldNotThrow()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[100];
		
		// Act & Assert - Multiple calls should be safe
		await webp.DisposeAsync();
		await webp.DisposeAsync(); // Second call
		await webp.DisposeAsync(); // Third call
	}

	[Theory]
	[InlineData(typeof(WebPRaster))]
	[InlineData(typeof(JpegRaster))]
	[InlineData(typeof(TiffRaster))]
	[InlineData(typeof(PngRaster))]
	[InlineData(typeof(Vector))]
	public async Task AllImageTypes_SupportAsyncDisposal(Type imageType)
	{
		// Arrange
		var image = (IImage)Activator.CreateInstance(imageType)!;
		
		// Act & Assert - Should not throw
		await image.DisposeAsync();
	}

	[Fact]
	public void LargeMetadataThreshold_ShouldBe1MB()
	{
		// Arrange
		var webp = new WebPRaster();
		webp.Metadata.IccProfile = new byte[999_999]; // Just under 1MB
		
		// Act & Assert
		Assert.False(webp.HasLargeMetadata);
		
		// Add one more byte to cross threshold
		webp.Metadata.IccProfile = new byte[1_000_001]; // Just over 1MB
		Assert.True(webp.HasLargeMetadata);
	}

	[Fact]
	public async Task EstimatedMetadataSize_ShouldBeAccurate()
	{
		// Arrange
		var webp = new WebPRaster();
		var iccSize = 100_000;
		var exifSize = 50_000;
		var xmpSize = 30_000;
		
		webp.Metadata.IccProfile = new byte[iccSize];
		webp.Metadata.ExifData = new byte[exifSize];
		webp.Metadata.XmpData = new byte[xmpSize];
		
		// Act
		var estimatedSize = webp.EstimatedMetadataSize;
		
		// Assert
		Assert.Equal(iccSize + exifSize + xmpSize, estimatedSize);
		
		// Clean up
		await webp.DisposeAsync();
		
		// Verify size is now much smaller (should be 0 or minimal)
		Assert.True(webp.EstimatedMetadataSize < 100);
	}

	[Fact]
	public async Task Vector_AsyncDisposal_ShouldComplete()
	{
		// Arrange
		var vector = new Vector { Width = 800, Height = 600 };
		
		// Vector base class has no metadata, so should have small estimated size
		Assert.False(vector.HasLargeMetadata);
		Assert.Equal(0, vector.EstimatedMetadataSize);
		
		// Act & Assert - Should not throw
		await vector.DisposeAsync();
	}

	[Fact]
	public void Vector_ImplementsIImageInterface()
	{
		// Arrange & Act
		IImage vector = new Vector();
		
		// Assert
		Assert.NotNull(vector);
		Assert.False(vector.HasLargeMetadata);
		Assert.Equal(0, vector.EstimatedMetadataSize);
	}
}