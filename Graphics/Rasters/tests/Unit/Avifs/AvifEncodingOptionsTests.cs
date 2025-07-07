// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;

namespace Wangkanai.Graphics.Rasters.UnitTests.Avifs;

public class AvifEncodingOptionsTests
{
	[Fact]
	public void Constructor_ShouldInitializeDefaults()
	{
		var options = new AvifEncodingOptions();

		Assert.Equal(AvifConstants.QualityPresets.Standard, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Standard, options.Speed);
		Assert.False(options.IsLossless);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.Equal(AvifConstants.DefaultThreadCount, options.ThreadCount);
		Assert.False(options.EnableFilmGrain);
		Assert.False(options.AddPreviewImage);
	}

	[Fact]
	public void Validate_WithValidSettings_ShouldReturnTrue()
	{
		var options = new AvifEncodingOptions
		{
			Quality = 85,
			Speed = 5,
			ChromaSubsampling = AvifChromaSubsampling.Yuv422,
			ThreadCount = 4
		};

		var isValid = options.Validate(out var error);

		Assert.True(isValid);
		Assert.Empty(error);
	}

	[Theory]
	[InlineData(AvifConstants.MinQuality - 1)]
	[InlineData(AvifConstants.MaxQuality + 1)]
	[InlineData(-10)]
	[InlineData(150)]
	public void Validate_WithInvalidQuality_ShouldReturnFalse(int quality)
	{
		var options = new AvifEncodingOptions { Quality = quality };

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("Quality", error);
	}

	[Theory]
	[InlineData(AvifConstants.MinSpeed - 1)]
	[InlineData(AvifConstants.MaxSpeed + 1)]
	[InlineData(-1)]
	[InlineData(20)]
	public void Validate_WithInvalidSpeed_ShouldReturnFalse(int speed)
	{
		var options = new AvifEncodingOptions { Speed = speed };

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("Speed", error);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(AvifConstants.Memory.MaxThreads + 1)]
	public void Validate_WithInvalidThreadCount_ShouldReturnFalse(int threadCount)
	{
		var options = new AvifEncodingOptions { ThreadCount = threadCount };

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("ThreadCount", error);
	}

	[Fact]
	public void CreateWebOptimized_ShouldReturnWebSettings()
	{
		var options = AvifEncodingOptions.CreateWebOptimized();

		Assert.Equal(AvifConstants.QualityPresets.Standard, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Standard, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
		Assert.True(options.ThreadCount > 0);
	}

	[Fact]
	public void CreateProfessional_ShouldReturnProfessionalSettings()
	{
		var options = AvifEncodingOptions.CreateProfessional();

		Assert.Equal(AvifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Slow, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
	}

	[Fact]
	public void CreateLossless_ShouldReturnLosslessSettings()
	{
		var options = AvifEncodingOptions.CreateLossless();

		Assert.Equal(AvifConstants.QualityPresets.Lossless, options.Quality);
		Assert.True(options.IsLossless);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
	}

	[Fact]
	public void CreateFast_ShouldReturnFastSettings()
	{
		var options = AvifEncodingOptions.CreateFast();

		Assert.Equal(AvifConstants.QualityPresets.Fast, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fastest, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
	}

	[Fact]
	public void CreateThumbnail_ShouldReturnThumbnailSettings()
	{
		var options = AvifEncodingOptions.CreateThumbnail();

		Assert.Equal(AvifConstants.QualityPresets.Thumbnail, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fast, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.False(options.AddPreviewImage);
	}

	[Fact]
	public void CreateHdr_ShouldReturnHdrSettings()
	{
		var options = AvifEncodingOptions.CreateHdr();

		Assert.Equal(AvifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(AvifChromaSubsampling.Yuv422, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
	}

	[Fact]
	public void Clone_ShouldCreateDeepCopy()
	{
		var original = new AvifEncodingOptions
		{
			Quality = 95,
			Speed = 8,
			IsLossless = true,
			ChromaSubsampling = AvifChromaSubsampling.Yuv444,
			ThreadCount = 8,
			EnableFilmGrain = true,
			AddPreviewImage = true
		};

		var clone = original.Clone();

		Assert.NotSame(original, clone);
		Assert.Equal(original.Quality, clone.Quality);
		Assert.Equal(original.Speed, clone.Speed);
		Assert.Equal(original.IsLossless, clone.IsLossless);
		Assert.Equal(original.ChromaSubsampling, clone.ChromaSubsampling);
		Assert.Equal(original.ThreadCount, clone.ThreadCount);
		Assert.Equal(original.EnableFilmGrain, clone.EnableFilmGrain);
		Assert.Equal(original.AddPreviewImage, clone.AddPreviewImage);
	}

	[Fact]
	public void ApplyPreset_WithWebOptimized_ShouldUpdateSettings()
	{
		var options = new AvifEncodingOptions
		{
			Quality = 50,
			Speed = 1
		};

		options.ApplyPreset(AvifPreset.WebOptimized);

		Assert.Equal(AvifConstants.QualityPresets.Standard, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Standard, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
	}

	[Fact]
	public void ApplyPreset_WithProfessional_ShouldUpdateSettings()
	{
		var options = new AvifEncodingOptions();

		options.ApplyPreset(AvifPreset.Professional);

		Assert.Equal(AvifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Slow, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
	}

	[Fact]
	public void ApplyPreset_WithLossless_ShouldUpdateSettings()
	{
		var options = new AvifEncodingOptions();

		options.ApplyPreset(AvifPreset.Lossless);

		Assert.True(options.IsLossless);
		Assert.Equal(AvifConstants.QualityPresets.Lossless, options.Quality);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
	}

	[Fact]
	public void ApplyPreset_WithFast_ShouldUpdateSettings()
	{
		var options = new AvifEncodingOptions();

		options.ApplyPreset(AvifPreset.Fast);

		Assert.Equal(AvifConstants.QualityPresets.Fast, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fastest, options.Speed);
	}

	[Fact]
	public void ApplyPreset_WithThumbnail_ShouldUpdateSettings()
	{
		var options = new AvifEncodingOptions();

		options.ApplyPreset(AvifPreset.Thumbnail);

		Assert.Equal(AvifConstants.QualityPresets.Thumbnail, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fast, options.Speed);
		Assert.False(options.AddPreviewImage);
	}

	[Fact]
	public void ToString_ShouldReturnDescriptiveString()
	{
		var options = new AvifEncodingOptions
		{
			Quality = 85,
			Speed = 5,
			IsLossless = false,
			ChromaSubsampling = AvifChromaSubsampling.Yuv422
		};

		var result = options.ToString();

		Assert.Contains("85", result);
		Assert.Contains("5", result);
		Assert.Contains("False", result);
		Assert.Contains("Yuv422", result);
	}

	[Fact]
	public void GetEstimatedEncodingTime_ShouldReturnReasonableEstimate()
	{
		var options = new AvifEncodingOptions
		{
			Speed = AvifConstants.SpeedPresets.Standard
		};

		var estimatedTime = options.GetEstimatedEncodingTime(1920, 1080);

		Assert.True(estimatedTime > TimeSpan.Zero);
		Assert.True(estimatedTime < TimeSpan.FromMinutes(10)); // Should be reasonable
	}

	[Fact]
	public void GetEstimatedEncodingTime_WithFasterSpeed_ShouldReturnShorterTime()
	{
		var slowOptions = new AvifEncodingOptions { Speed = AvifConstants.SpeedPresets.Slowest };
		var fastOptions = new AvifEncodingOptions { Speed = AvifConstants.SpeedPresets.Fastest };

		var slowTime = slowOptions.GetEstimatedEncodingTime(1920, 1080);
		var fastTime = fastOptions.GetEstimatedEncodingTime(1920, 1080);

		Assert.True(fastTime < slowTime);
	}

	[Fact]
	public void GetEstimatedEncodingTime_WithLargerImage_ShouldReturnLongerTime()
	{
		var options = new AvifEncodingOptions();

		var smallTime = options.GetEstimatedEncodingTime(640, 480);
		var largeTime = options.GetEstimatedEncodingTime(3840, 2160);

		Assert.True(largeTime > smallTime);
	}

	[Fact]
	public void Equals_WithSameSettings_ShouldReturnTrue()
	{
		var options1 = new AvifEncodingOptions
		{
			Quality = 85,
			Speed = 5,
			IsLossless = false
		};

		var options2 = new AvifEncodingOptions
		{
			Quality = 85,
			Speed = 5,
			IsLossless = false
		};

		Assert.True(options1.Equals(options2));
		Assert.Equal(options1.GetHashCode(), options2.GetHashCode());
	}

	[Fact]
	public void Equals_WithDifferentSettings_ShouldReturnFalse()
	{
		var options1 = new AvifEncodingOptions { Quality = 85 };
		var options2 = new AvifEncodingOptions { Quality = 90 };

		Assert.False(options1.Equals(options2));
	}

	[Fact]
	public void AllFactoryMethods_ShouldCreateValidOptions()
	{
		var factoryMethods = new Func<AvifEncodingOptions>[]
		{
			AvifEncodingOptions.CreateWebOptimized,
			AvifEncodingOptions.CreateProfessional,
			AvifEncodingOptions.CreateLossless,
			AvifEncodingOptions.CreateFast,
			AvifEncodingOptions.CreateThumbnail,
			AvifEncodingOptions.CreateHdr
		};

		foreach (var method in factoryMethods)
		{
			var options = method();
			var isValid = options.Validate(out var error);
			Assert.True(isValid, $"Factory method {method.Method.Name} created invalid options: {error}");
		}
	}
}