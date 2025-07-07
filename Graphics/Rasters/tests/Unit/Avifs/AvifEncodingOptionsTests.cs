// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Avifs;

namespace Wangkanai.Graphics.Rasters.Avifs;

public class AvifEncodingOptionsTests
{
	[Fact]
	public void Constructor_ShouldInitializeDefaults()
	{
		var options = new AvifEncodingOptions();

		Assert.Equal(AvifConstants.DefaultQuality, options.Quality);
		Assert.Equal(AvifConstants.DefaultSpeed, options.Speed);
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
		Assert.Null(error);
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
		Assert.Contains("Thread count", error);
	}

	[Fact]
	public void Validate_WithLosslessButWrongQuality_ShouldReturnFalse()
	{
		var options = new AvifEncodingOptions
		{
			IsLossless = true,
			Quality = 85 // Should be 100 for lossless
		};

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("Lossless mode requires quality to be 100", error);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(51)]
	public void Validate_WithInvalidFilmGrainStrength_ShouldReturnFalse(int strength)
	{
		var options = new AvifEncodingOptions { FilmGrainStrength = strength };

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("Film grain strength", error);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(64)]
	public void Validate_WithInvalidQuantizer_ShouldReturnFalse(int quantizer)
	{
		var options = new AvifEncodingOptions { MinQuantizer = quantizer };

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("quantizer", error);
	}

	[Fact]
	public void Validate_WithInvalidQuantizerRange_ShouldReturnFalse()
	{
		var options = new AvifEncodingOptions
		{
			MinQuantizer = 30,
			MaxQuantizer = 20 // Min > Max
		};

		var isValid = options.Validate(out var error);

		Assert.False(isValid);
		Assert.Contains("Minimum quantizer cannot be greater than maximum quantizer", error);
	}

	[Fact]
	public void CreateWebOptimized_ShouldReturnWebSettings()
	{
		var options = AvifEncodingOptions.CreateWebOptimized();

		Assert.Equal(AvifConstants.QualityPresets.Web, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fast, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv420, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
		Assert.True(options.OptimizeForSize);
		Assert.True(options.AddPreviewImage);
		Assert.Equal(128, options.PreviewMaxDimension);
	}

	[Fact]
	public void CreateHighQuality_ShouldReturnHighQualitySettings()
	{
		var options = AvifEncodingOptions.CreateHighQuality();

		Assert.Equal(AvifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(AvifConstants.QualityPresets.Professional, options.AlphaQuality);
		Assert.Equal(AvifConstants.SpeedPresets.Slow, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
		Assert.True(options.UseAdaptiveQuantization);
	}

	[Fact]
	public void CreateLossless_ShouldReturnLosslessSettings()
	{
		var options = AvifEncodingOptions.CreateLossless();

		Assert.Equal(AvifConstants.QualityPresets.Lossless, options.Quality);
		Assert.Equal(AvifConstants.QualityPresets.Lossless, options.AlphaQuality);
		Assert.True(options.IsLossless);
		Assert.True(options.IsAlphaLossless);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.Equal(AvifConstants.SpeedPresets.Slow, options.Speed);
	}

	[Fact]
	public void CreateFast_ShouldReturnFastSettings()
	{
		var options = AvifEncodingOptions.CreateFast();

		Assert.Equal(AvifConstants.QualityPresets.Standard, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Fastest, options.Speed);
		Assert.False(options.EnableAutoTiling);
		Assert.False(options.UseAdaptiveQuantization);
	}

	[Fact]
	public void CreateHdr_ShouldReturnHdrSettings()
	{
		var options = AvifEncodingOptions.CreateHdr();

		Assert.Equal(AvifConstants.QualityPresets.Professional, options.Quality);
		Assert.Equal(AvifConstants.SpeedPresets.Default, options.Speed);
		Assert.Equal(AvifChromaSubsampling.Yuv444, options.ChromaSubsampling);
		Assert.False(options.IsLossless);
		Assert.True(options.IncludeIccProfile);
		Assert.True(options.UseAdaptiveQuantization);
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

		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.Contains("AvifEncodingOptions", result); // Should contain the class name
	}

	[Fact]
	public void AllFactoryMethods_ShouldCreateValidOptions()
	{
		var factoryMethods = new Func<AvifEncodingOptions>[]
		{
			AvifEncodingOptions.CreateWebOptimized,
			AvifEncodingOptions.CreateHighQuality,
			AvifEncodingOptions.CreateLossless,
			AvifEncodingOptions.CreateFast,
			AvifEncodingOptions.CreateHdr
		};

		foreach (var method in factoryMethods)
		{
			var options = method();
			var isValid = options.Validate(out var error);
			Assert.True(isValid, $"Factory method {method.Method.Name} created invalid options: {error}");
		}
	}

	// Negative test cases for improved coverage
	[Fact]
	public void Validate_WithNullError_ShouldHandleGracefully()
	{
		var options = new AvifEncodingOptions();
		var isValid = options.Validate(out var error);

		Assert.True(isValid);
		Assert.Null(error);
	}

	[Fact]
	public void EncodingOptions_WithExtremeValues_ShouldValidateCorrectly()
	{
		var options = new AvifEncodingOptions
		{
			Quality = AvifConstants.MinQuality,
			Speed = AvifConstants.MinSpeed,
			ThreadCount = 0, // Auto
			FilmGrainStrength = 0,
			MinQuantizer = 0,
			MaxQuantizer = 63,
			DenoisingStrength = 0,
			SharpeningStrength = 0
		};

		var isValid = options.Validate(out var error);
		Assert.True(isValid, $"Validation failed: {error}");
	}

	[Fact]
	public void EncodingOptions_WithMaxValues_ShouldValidateCorrectly()
	{
		var options = new AvifEncodingOptions
		{
			Quality = AvifConstants.MaxQuality,
			Speed = AvifConstants.MaxSpeed,
			ThreadCount = AvifConstants.Memory.MaxThreads,
			FilmGrainStrength = 50,
			MinQuantizer = 63,
			MaxQuantizer = 63,
			DenoisingStrength = 50,
			SharpeningStrength = 7
		};

		var isValid = options.Validate(out var error);
		Assert.True(isValid, $"Validation failed: {error}");
	}
}
