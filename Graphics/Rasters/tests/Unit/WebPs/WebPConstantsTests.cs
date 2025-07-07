// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.WebPs;

namespace Wangkanai.Graphics.Rasters.WebPs;

public class WebPConstantsTests
{
	[Fact]
	public void Signature_ShouldBeRIFF()
	{
		// Act
		var signature = WebPConstants.Signature;

		// Assert
		Assert.Equal([0x52, 0x49, 0x46, 0x46], signature); // "RIFF"
	}

	[Fact]
	public void FormatId_ShouldBeWEBP()
	{
		// Act
		var formatId = WebPConstants.FormatId;

		// Assert
		Assert.Equal([0x57, 0x45, 0x42, 0x50], formatId); // "WEBP"
	}

	[Fact]
	public void VP8ChunkId_ShouldBeCorrect()
	{
		// Act
		var chunkId = WebPConstants.VP8ChunkId;

		// Assert
		Assert.Equal([0x56, 0x50, 0x38, 0x20], chunkId); // "VP8 "
	}

	[Fact]
	public void VP8LChunkId_ShouldBeCorrect()
	{
		// Act
		var chunkId = WebPConstants.VP8LChunkId;

		// Assert
		Assert.Equal([0x56, 0x50, 0x38, 0x4C], chunkId); // "VP8L"
	}

	[Fact]
	public void VP8XChunkId_ShouldBeCorrect()
	{
		// Act
		var chunkId = WebPConstants.VP8XChunkId;

		// Assert
		Assert.Equal([0x56, 0x50, 0x38, 0x58], chunkId); // "VP8X"
	}

	[Fact]
	public void WidthConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1u, WebPConstants.MinWidth);
		Assert.Equal(16383u, WebPConstants.MaxWidth);
	}

	[Fact]
	public void HeightConstants_ShouldHaveCorrectValues()
	{
		// Assert
		Assert.Equal(1u, WebPConstants.MinHeight);
		Assert.Equal(16383u, WebPConstants.MaxHeight);
	}

	[Theory]
	[InlineData(WebPConstants.MinQuality, 0)]
	[InlineData(WebPConstants.MaxQuality, 100)]
	[InlineData(WebPConstants.DefaultQuality, 75)]
	public void QualityConstants_ShouldHaveCorrectValues(int actual, int expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(WebPConstants.MinCompressionLevel, 0)]
	[InlineData(WebPConstants.MaxCompressionLevel, 9)]
	[InlineData(WebPConstants.DefaultCompressionLevel, 6)]
	public void CompressionLevelConstants_ShouldHaveCorrectValues(int actual, int expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(WebPConstants.RiffHeaderSize, 12)]
	[InlineData(WebPConstants.ChunkHeaderSize, 8)]
	[InlineData(WebPConstants.VP8XChunkSize, 10)]
	[InlineData(WebPConstants.AnimChunkSize, 6)]
	[InlineData(WebPConstants.ContainerOverhead, 50)]
	public void SizeConstants_ShouldHaveCorrectValues(int actual, int expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(WebPConstants.BitDepth, 8)]
	[InlineData(WebPConstants.RgbChannels, 3)]
	[InlineData(WebPConstants.RgbaChannels, 4)]
	public void ChannelConstants_ShouldHaveCorrectValues(byte actual, byte expected)
	{
		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void MaxAnimationLoops_ShouldBeMaxUShort()
	{
		// Assert
		Assert.Equal(ushort.MaxValue, WebPConstants.MaxAnimationLoops);
	}

	[Fact]
	public void DefaultBackgroundColor_ShouldBeTransparent()
	{
		// Assert
		Assert.Equal(0x00000000u, WebPConstants.DefaultBackgroundColor);
	}

	[Fact]
	public void AllChunkIds_ShouldBe4BytesLong()
	{
		// Assert
		Assert.Equal(4, WebPConstants.Signature.Length);
		Assert.Equal(4, WebPConstants.FormatId.Length);
		Assert.Equal(4, WebPConstants.VP8ChunkId.Length);
		Assert.Equal(4, WebPConstants.VP8LChunkId.Length);
		Assert.Equal(4, WebPConstants.VP8XChunkId.Length);
		Assert.Equal(4, WebPConstants.AlphaChunkId.Length);
		Assert.Equal(4, WebPConstants.AnimChunkId.Length);
		Assert.Equal(4, WebPConstants.AnimFrameChunkId.Length);
		Assert.Equal(4, WebPConstants.IccProfileChunkId.Length);
		Assert.Equal(4, WebPConstants.ExifChunkId.Length);
		Assert.Equal(4, WebPConstants.XmpChunkId.Length);
	}
}
