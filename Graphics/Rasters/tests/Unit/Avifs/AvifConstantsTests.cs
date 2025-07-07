// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Avifs;

public class AvifConstantsTests
{
	[Fact]
	public void FileTypeBoxType_ShouldHaveCorrectValue()
	{
		var expected = "ftyp"u8.ToArray();
		Assert.Equal(expected, AvifConstants.FileTypeBoxType.ToArray());
	}

	[Fact]
	public void AvifBrand_ShouldHaveCorrectValue()
	{
		var expected = "avif"u8.ToArray();
		Assert.Equal(expected, AvifConstants.AvifBrand.ToArray());
	}

	[Fact]
	public void AvisBrand_ShouldHaveCorrectValue()
	{
		var expected = "avis"u8.ToArray();
		Assert.Equal(expected, AvifConstants.AvisBrand.ToArray());
	}

	[Fact]
	public void Mif1Brand_ShouldHaveCorrectValue()
	{
		var expected = "mif1"u8.ToArray();
		Assert.Equal(expected, AvifConstants.Mif1Brand.ToArray());
	}

	[Fact]
	public void QualityRange_ShouldBeValid()
	{
		Assert.True(AvifConstants.MinQuality >= 0);
		Assert.True(AvifConstants.MaxQuality <= 100);
		Assert.True(AvifConstants.MinQuality < AvifConstants.MaxQuality);
	}

	[Fact]
	public void SpeedRange_ShouldBeValid()
	{
		Assert.True(AvifConstants.MinSpeed >= 0);
		Assert.True(AvifConstants.MaxSpeed >= AvifConstants.MinSpeed);
		Assert.True(AvifConstants.MaxSpeed <= 10);
	}

	[Fact]
	public void MaxDimension_ShouldBeReasonable()
	{
		Assert.True(AvifConstants.MaxDimension > 0);
		Assert.True(AvifConstants.MaxDimension >= 32768); // Should support at least 32K resolution
	}

	[Fact]
	public void DefaultThreadCount_ShouldBePositive()
	{
		Assert.True(AvifConstants.DefaultThreadCount > 0);
		Assert.True(AvifConstants.DefaultThreadCount <= Environment.ProcessorCount);
	}

	[Theory]
	[InlineData(nameof(AvifConstants.QualityPresets.Thumbnail))]
	[InlineData(nameof(AvifConstants.QualityPresets.Web))]
	[InlineData(nameof(AvifConstants.QualityPresets.Standard))]
	[InlineData(nameof(AvifConstants.QualityPresets.Professional))]
	[InlineData(nameof(AvifConstants.QualityPresets.NearLossless))]
	[InlineData(nameof(AvifConstants.QualityPresets.Lossless))]
	public void QualityPresets_ShouldBeInValidRange(string presetName)
	{
		var qualityValue = presetName switch
		{
			nameof(AvifConstants.QualityPresets.Thumbnail) => AvifConstants.QualityPresets.Thumbnail,
			nameof(AvifConstants.QualityPresets.Web) => AvifConstants.QualityPresets.Web,
			nameof(AvifConstants.QualityPresets.Standard) => AvifConstants.QualityPresets.Standard,
			nameof(AvifConstants.QualityPresets.Professional) => AvifConstants.QualityPresets.Professional,
			nameof(AvifConstants.QualityPresets.NearLossless) => AvifConstants.QualityPresets.NearLossless,
			nameof(AvifConstants.QualityPresets.Lossless) => AvifConstants.QualityPresets.Lossless,
			_ => throw new ArgumentException($"Unknown preset: {presetName}")
		};

		Assert.InRange(qualityValue, AvifConstants.MinQuality, AvifConstants.MaxQuality);
	}

	[Theory]
	[InlineData(nameof(AvifConstants.SpeedPresets.Slowest))]
	[InlineData(nameof(AvifConstants.SpeedPresets.VerySlow))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Slow))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Default))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Fast))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Fastest))]
	public void SpeedPresets_ShouldBeInValidRange(string presetName)
	{
		var speedValue = presetName switch
		{
			nameof(AvifConstants.SpeedPresets.Slowest) => AvifConstants.SpeedPresets.Slowest,
			nameof(AvifConstants.SpeedPresets.VerySlow) => AvifConstants.SpeedPresets.VerySlow,
			nameof(AvifConstants.SpeedPresets.Slow) => AvifConstants.SpeedPresets.Slow,
			nameof(AvifConstants.SpeedPresets.Default) => AvifConstants.SpeedPresets.Default,
			nameof(AvifConstants.SpeedPresets.Fast) => AvifConstants.SpeedPresets.Fast,
			nameof(AvifConstants.SpeedPresets.Fastest) => AvifConstants.SpeedPresets.Fastest,
			_ => throw new ArgumentException($"Unknown preset: {presetName}")
		};

		Assert.InRange(speedValue, AvifConstants.MinSpeed, AvifConstants.MaxSpeed);
	}

	[Fact]
	public void HdrConstants_ShouldHaveValidValues()
	{
		Assert.True(AvifConstants.Hdr.SdrPeakBrightness > 0);
		Assert.True(AvifConstants.Hdr.Hdr10PeakBrightness > AvifConstants.Hdr.SdrPeakBrightness);
		Assert.True(AvifConstants.Hdr.Hdr10PlusPeakBrightness > AvifConstants.Hdr.Hdr10PeakBrightness);
		Assert.True(AvifConstants.Hdr.DolbyVisionPeakBrightness > AvifConstants.Hdr.Hdr10PlusPeakBrightness);
	}

	[Fact]
	public void MemoryConstants_ShouldHaveReasonableValues()
	{
		Assert.True(AvifConstants.Memory.MaxPixelBufferSizeMB > 0);
		Assert.True(AvifConstants.Memory.MaxMetadataSizeMB > 0);
		Assert.True(AvifConstants.Memory.MaxThreads > 0);
		Assert.True(AvifConstants.Memory.MaxThreads <= 64); // Reasonable upper bound
	}

	[Fact]
	public void QualityPresets_ShouldBeInAscendingOrder()
	{
		Assert.True(AvifConstants.QualityPresets.Preview < AvifConstants.QualityPresets.Thumbnail);
		Assert.True(AvifConstants.QualityPresets.Thumbnail < AvifConstants.QualityPresets.Web);
		Assert.True(AvifConstants.QualityPresets.Web < AvifConstants.QualityPresets.Standard);
		Assert.True(AvifConstants.QualityPresets.Standard < AvifConstants.QualityPresets.Professional);
		Assert.True(AvifConstants.QualityPresets.Professional < AvifConstants.QualityPresets.NearLossless);
		Assert.True(AvifConstants.QualityPresets.NearLossless <= AvifConstants.QualityPresets.Lossless);
	}

	[Fact]
	public void SpeedPresets_ShouldBeInAscendingOrder()
	{
		Assert.True(AvifConstants.SpeedPresets.Slowest < AvifConstants.SpeedPresets.VerySlow);
		Assert.True(AvifConstants.SpeedPresets.VerySlow < AvifConstants.SpeedPresets.Slow);
		Assert.True(AvifConstants.SpeedPresets.Slow < AvifConstants.SpeedPresets.Default);
		Assert.True(AvifConstants.SpeedPresets.Default < AvifConstants.SpeedPresets.Fast);
		Assert.True(AvifConstants.SpeedPresets.Fast < AvifConstants.SpeedPresets.Fastest);
	}

	[Fact]
	public void BoxTypes_ShouldBeFourCharacters()
	{
		Assert.Equal(4, AvifConstants.FileTypeBoxType.Length);
		Assert.Equal(4, AvifConstants.MetaBoxType.Length);
		Assert.Equal(4, AvifConstants.HandlerBoxType.Length);
		Assert.Equal(4, AvifConstants.PrimaryItemBoxType.Length);
		Assert.Equal(4, AvifConstants.ItemLocationBoxType.Length);
		Assert.Equal(4, AvifConstants.ItemInfoBoxType.Length);
		Assert.Equal(4, AvifConstants.ItemPropertiesBoxType.Length);
	}

	[Fact]
	public void Brands_ShouldBeFourCharacters()
	{
		Assert.Equal(4, AvifConstants.AvifBrand.Length);
		Assert.Equal(4, AvifConstants.AvisBrand.Length);
		Assert.Equal(4, AvifConstants.Mif1Brand.Length);
	}
}