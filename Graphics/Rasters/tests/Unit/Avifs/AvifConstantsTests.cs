// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Avifs;

public class AvifConstantsTests
{
	[Fact]
	public void FileTypeBoxType_ShouldHaveCorrectValue()
	{
		Assert.Equal("ftyp", AvifConstants.FileTypeBoxType);
	}

	[Fact]
	public void AvifBrand_ShouldHaveCorrectValue()
	{
		Assert.Equal("avif", AvifConstants.AvifBrand);
	}

	[Fact]
	public void AvisBrand_ShouldHaveCorrectValue()
	{
		Assert.Equal("avis", AvifConstants.AvisBrand);
	}

	[Fact]
	public void Mif1Brand_ShouldHaveCorrectValue()
	{
		Assert.Equal("mif1", AvifConstants.Mif1Brand);
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
	[InlineData(nameof(AvifConstants.QualityPresets.Fast))]
	[InlineData(nameof(AvifConstants.QualityPresets.Standard))]
	[InlineData(nameof(AvifConstants.QualityPresets.Professional))]
	[InlineData(nameof(AvifConstants.QualityPresets.Archival))]
	[InlineData(nameof(AvifConstants.QualityPresets.Lossless))]
	public void QualityPresets_ShouldBeInValidRange(string presetName)
	{
		var qualityValue = presetName switch
		{
			nameof(AvifConstants.QualityPresets.Thumbnail) => AvifConstants.QualityPresets.Thumbnail,
			nameof(AvifConstants.QualityPresets.Fast) => AvifConstants.QualityPresets.Fast,
			nameof(AvifConstants.QualityPresets.Standard) => AvifConstants.QualityPresets.Standard,
			nameof(AvifConstants.QualityPresets.Professional) => AvifConstants.QualityPresets.Professional,
			nameof(AvifConstants.QualityPresets.Archival) => AvifConstants.QualityPresets.Archival,
			nameof(AvifConstants.QualityPresets.Lossless) => AvifConstants.QualityPresets.Lossless,
			_ => throw new ArgumentException($"Unknown preset: {presetName}")
		};

		Assert.InRange(qualityValue, AvifConstants.MinQuality, AvifConstants.MaxQuality);
	}

	[Theory]
	[InlineData(nameof(AvifConstants.SpeedPresets.Slowest))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Slow))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Standard))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Fast))]
	[InlineData(nameof(AvifConstants.SpeedPresets.Fastest))]
	public void SpeedPresets_ShouldBeInValidRange(string presetName)
	{
		var speedValue = presetName switch
		{
			nameof(AvifConstants.SpeedPresets.Slowest) => AvifConstants.SpeedPresets.Slowest,
			nameof(AvifConstants.SpeedPresets.Slow) => AvifConstants.SpeedPresets.Slow,
			nameof(AvifConstants.SpeedPresets.Standard) => AvifConstants.SpeedPresets.Standard,
			nameof(AvifConstants.SpeedPresets.Fast) => AvifConstants.SpeedPresets.Fast,
			nameof(AvifConstants.SpeedPresets.Fastest) => AvifConstants.SpeedPresets.Fastest,
			_ => throw new ArgumentException($"Unknown preset: {presetName}")
		};

		Assert.InRange(speedValue, AvifConstants.MinSpeed, AvifConstants.MaxSpeed);
	}

	[Fact]
	public void HdrConstants_ShouldHaveValidValues()
	{
		Assert.True(AvifConstants.Hdr.MinLuminance >= 0);
		Assert.True(AvifConstants.Hdr.MaxLuminance > AvifConstants.Hdr.MinLuminance);
		Assert.True(AvifConstants.Hdr.MaxContentLightLevel > 0);
		Assert.True(AvifConstants.Hdr.MaxFrameAverageLightLevel > 0);
		Assert.True(AvifConstants.Hdr.MaxFrameAverageLightLevel <= AvifConstants.Hdr.MaxContentLightLevel);
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
		Assert.True(AvifConstants.QualityPresets.Thumbnail < AvifConstants.QualityPresets.Fast);
		Assert.True(AvifConstants.QualityPresets.Fast < AvifConstants.QualityPresets.Standard);
		Assert.True(AvifConstants.QualityPresets.Standard < AvifConstants.QualityPresets.Professional);
		Assert.True(AvifConstants.QualityPresets.Professional < AvifConstants.QualityPresets.Archival);
		Assert.True(AvifConstants.QualityPresets.Archival <= AvifConstants.QualityPresets.Lossless);
	}

	[Fact]
	public void SpeedPresets_ShouldBeInAscendingOrder()
	{
		Assert.True(AvifConstants.SpeedPresets.Slowest < AvifConstants.SpeedPresets.Slow);
		Assert.True(AvifConstants.SpeedPresets.Slow < AvifConstants.SpeedPresets.Standard);
		Assert.True(AvifConstants.SpeedPresets.Standard < AvifConstants.SpeedPresets.Fast);
		Assert.True(AvifConstants.SpeedPresets.Fast < AvifConstants.SpeedPresets.Fastest);
	}

	[Fact]
	public void BoxTypes_ShouldBeFourCharacters()
	{
		Assert.Equal(4, AvifConstants.FileTypeBoxType.Length);
		Assert.Equal(4, AvifConstants.ImagePropertiesBoxType.Length);
		Assert.Equal(4, AvifConstants.ImageSpatialExtentsBoxType.Length);
		Assert.Equal(4, AvifConstants.AuxiliaryTypePropertyBoxType.Length);
		Assert.Equal(4, AvifConstants.CleanApertureBoxType.Length);
		Assert.Equal(4, AvifConstants.PixelAspectRatioBoxType.Length);
		Assert.Equal(4, AvifConstants.ColorPropertiesBoxType.Length);
	}

	[Fact]
	public void Brands_ShouldBeFourCharacters()
	{
		Assert.Equal(4, AvifConstants.AvifBrand.Length);
		Assert.Equal(4, AvifConstants.AvisBrand.Length);
		Assert.Equal(4, AvifConstants.Mif1Brand.Length);
		Assert.Equal(4, AvifConstants.HeicBrand.Length);
		Assert.Equal(4, AvifConstants.Av01Brand.Length);
	}
}